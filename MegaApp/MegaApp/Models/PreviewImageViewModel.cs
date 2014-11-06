using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Windows.Networking.Connectivity;
using Windows.Phone.System.Memory;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Reactive;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace MegaApp.Models
{
    class PreviewImageViewModel : BaseSdkViewModel
    {
        public PreviewImageViewModel(MegaSDK megaSdk, CloudDriveViewModel cloudDriveViewModel)
            : base(megaSdk)
        {
            PreviewItems = new ObservableCollection<NodeViewModel>(
                cloudDriveViewModel.ChildNodes.Where(n => n.IsImage || n.GetMegaNode().hasPreview()));

            cloudDriveViewModel.ChildNodes.CollectionChanged += CloudDriveNodesOnCollectionChanged;
        }

        private void CloudDriveNodesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Remove) return;
            var removedNode = (NodeViewModel) e.OldItems[0];
            PreviewItems.Remove(PreviewItems.FirstOrDefault(n=> n.GetMegaNode().getBase64Handle() == 
                removedNode.GetMegaNode().getBase64Handle()));
        }

        #region Methods

        public void TranslateAppBar(IList iconButtons, IList menuItems)
        {
            ((ApplicationBarIconButton)iconButtons[0]).Text = UiResources.Previous;
            ((ApplicationBarIconButton)iconButtons[1]).Text = UiResources.ViewOrginal;
            ((ApplicationBarIconButton)iconButtons[2]).Text = UiResources.GetPreviewLink;
            ((ApplicationBarIconButton)iconButtons[3]).Text = UiResources.Next;

            ((ApplicationBarMenuItem)menuItems[0]).Text = UiResources.Rename;
            ((ApplicationBarMenuItem)menuItems[1]).Text = UiResources.Remove;
        }

        #endregion

        #region Properties

        public ObservableCollection<NodeViewModel> PreviewItems { get; private set; }
        

        private NodeViewModel _selectedPreview;

        public NodeViewModel SelectedPreview
        {
            get { return _selectedPreview; }
            set
            {
                _selectedPreview = value;
                OnPropertyChanged("SelectedPreview");
            }
        }

        #endregion
    }
}
