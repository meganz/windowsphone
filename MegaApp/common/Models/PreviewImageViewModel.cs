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
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Notification;
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
            PreviewItems = new ObservableCollection<ImageNodeViewModel>(
                cloudDriveViewModel.ChildNodes.Where(n => n is ImageNodeViewModel).Cast<ImageNodeViewModel>());

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
            ((ApplicationBarIconButton)iconButtons[0]).Text = UiResources.Previous.ToLower();
            ((ApplicationBarIconButton)iconButtons[1]).Text = UiResources.Download.ToLower();
            ((ApplicationBarIconButton)iconButtons[2]).Text = UiResources.GetPreviewLink.ToLower();
            ((ApplicationBarIconButton)iconButtons[3]).Text = UiResources.Next.ToLower();

            ((ApplicationBarMenuItem)menuItems[0]).Text = UiResources.Rename.ToLower();
            ((ApplicationBarMenuItem)menuItems[1]).Text = UiResources.Remove.ToLower();
        }

        #endregion

        #region Properties

        public ObservableCollection<ImageNodeViewModel> PreviewItems { get; private set; }

        public GalleryDirection GalleryDirection { get; set; }


        private ImageNodeViewModel _selectedPreview;

        public ImageNodeViewModel SelectedPreview
        {
            get { return _selectedPreview; }
            set
            {
                bool initialize = _selectedPreview == null;
                _selectedPreview = value;
                SetViewingRange(3, initialize);
                CleanUpMemory(4);
                OnPropertyChanged("SelectedPreview");
            }
        }

        private void SetViewingRange(int inViewRange, bool initialize)
        {
            try
            {
                int currentIndex = PreviewItems.IndexOf(SelectedPreview);
                int lowIndex = currentIndex - inViewRange;
                if (lowIndex < 0) lowIndex = 0;
                int highIndex = currentIndex + inViewRange;
                if (highIndex > PreviewItems.Count - 1) highIndex = PreviewItems.Count - 1;

                if (initialize)
                {
                    for (int i = currentIndex; i >= lowIndex; i--)
                        PreviewItems[i].InViewingRange = true;
                    for (int i = currentIndex; i <= highIndex; i++)
                        PreviewItems[i].InViewingRange = true;
                }
                else
                {
                    switch (GalleryDirection)
                    {
                        case GalleryDirection.Next:
                            PreviewItems[highIndex].InViewingRange = true;
                            break;
                        case GalleryDirection.Previous:
                            PreviewItems[lowIndex].InViewingRange = true;
                            break;
                    }
                }
            }
            catch(ArgumentOutOfRangeException)
            {
                return;
            }
        }

        private void CleanUpMemory(int cleanRange)
        {
            int currentIndex = PreviewItems.IndexOf(SelectedPreview);
            int previewItemsCount = PreviewItems.Count-1;

            switch (GalleryDirection)
            {
                case GalleryDirection.Next:
                    if ((currentIndex - cleanRange) >= 0)
                    {
                        int cleanIndex = currentIndex - cleanRange;
                        if (PreviewItems[cleanIndex].IsBusy)
                            PreviewItems[cleanIndex].CancelPreviewRequest();
                        PreviewItems[cleanIndex].InViewingRange = false;
                        PreviewItems[cleanIndex].PreviewImageUri = null;
                    }
                    break;
                case GalleryDirection.Previous:
                    if ((currentIndex + cleanRange) <= previewItemsCount)
                    {
                        int cleanIndex = currentIndex + cleanRange;
                        if (PreviewItems[cleanIndex].IsBusy)
                            PreviewItems[cleanIndex].CancelPreviewRequest();
                        PreviewItems[cleanIndex].InViewingRange = false;
                        PreviewItems[cleanIndex].PreviewImageUri = null;
                    }
                    break;

            }

        }

        #endregion
    }
}
