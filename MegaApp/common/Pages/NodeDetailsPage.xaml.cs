using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;

namespace MegaApp.Pages
{
    public partial class NodeDetailsPage : MegaPhoneApplicationPage
    {
        private readonly NodeViewModel _nodeViewModel;
        private DownloadNodeViewModel _downloadViewModel;

        public NodeDetailsPage()
        {
            _nodeViewModel = NavigateService.GetNavigationData<NodeViewModel>();
            this.DataContext = _nodeViewModel;

            InitializeComponent();

            SetApplicationBar();
        }

        private void SetApplicationBar()
        {
            this.ApplicationBar = (ApplicationBar)Resources["NodeDetailsMenu"];

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Ok.ToLower();
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = UiResources.Download.ToLower();
            ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).Text = UiResources.Remove.ToLower();
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            
        }

        private void OnDownloadClick(object sender, EventArgs e)
        {

        }

        private void OnRemoveClick(object sender, EventArgs e)
        {

        }

        private void BtnAvailableOfflineSwitch_CheckedChanged(object sender, Telerik.Windows.Controls.CheckedChangedEventArgs e)
        {
            if (e.NewState)
                _downloadViewModel = new DownloadNodeViewModel(this._nodeViewModel);
        }
    }
}