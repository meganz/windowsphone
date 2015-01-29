using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Models;
using MegaApp.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;

namespace MegaApp.Pages
{
    public partial class TransferPage : PhoneApplicationPage
    {
        public TransferPage()
        {
            var transfersViewModel = new TransfersViewModel(App.MegaSdk, App.MegaTransfers);
            this.DataContext = transfersViewModel;
            InitializeComponent();

            InteractionEffectManager.AllowedTypes.Add(typeof (RadDataBoundListBoxItem));            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationParameter navParam = NavigateService.ProcessQueryString(NavigationContext.QueryString);

            if (navParam == NavigationParameter.Downloads)
                Transfers.SelectedItem = Downloads;

            if (navParam == NavigationParameter.PictureSelected)
                NavigationService.RemoveBackEntry();

            if (navParam == NavigationParameter.AlbumSelected || navParam == NavigationParameter.SelfieSelected)
            {
                NavigationService.RemoveBackEntry();
                NavigationService.RemoveBackEntry();
            }

            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();
        }

        private void OnPauseAllClick(object sender, System.EventArgs e)
        {        	
            if (App.MegaTransfers.Count < 1) return;

            App.MegaSdk.pauseTransfers(true, new PauseTransferRequestListener(true));
        }

        private void OnStartResumeAllClick(object sender, EventArgs e)
        {
            if (App.MegaTransfers.Count < 1) return;

            App.MegaSdk.pauseTransfers(false, new PauseTransferRequestListener(false));
        }

        private void OnCancelAllClick(object sender, EventArgs e)
        {
            if (App.MegaTransfers.Count < 1) return;
                        
            foreach (var item in App.MegaTransfers)
            {
                var transfer = (TransferObjectModel)item;
                if (transfer == null) continue;
                
                transfer.CancelTransfer();
            }
        }

        private void OnCleanUpTransfersClick(object sender, EventArgs e)
        {
            if (App.MegaTransfers.Count < 1) return;

            var transfersToRemove = new List<TransferObjectModel>();
            foreach (var item in App.MegaTransfers)
            {
                var transfer = (TransferObjectModel)item;
                    if (transfer == null) continue;
                if (transfer.Status == TransferStatus.Finished || transfer.Status == TransferStatus.Canceled ||
                    transfer.Status == TransferStatus.Error)
                {
                    transfersToRemove.Add(transfer);
                    // Clean up: remove the upload copied file from the cache
                    if (transfer.Type == TransferType.Upload)
                        File.Delete(transfer.FilePath);
                }
            }

            foreach (var item in transfersToRemove)
                App.MegaTransfers.Remove(item);
        }
        
    }
}