using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Resources;
using Microsoft.Phone.Shell;

namespace MegaApp.Models
{
    class PreviewImageViewModel : BaseAppInfoAwareViewModel
    {
        public PreviewImageViewModel(MegaSDK megaSdk, AppInformation appInformation, FolderViewModel folder)
            : base(megaSdk, appInformation)
        {
            if(folder == null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                        AppMessages.GetPreviewFailed_Title,
                        AppMessages.GetPreviewFailed,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                });                

                return;
            }

            PreviewItems = new ObservableCollection<ImageNodeViewModel>(
                folder.ChildNodes.Where(n => n is ImageNodeViewModel).Cast<ImageNodeViewModel>());

            folder.ChildNodes.CollectionChanged += (sender, args) =>
            {
                if (args.Action != NotifyCollectionChangedAction.Remove) return;

                var removedNode = (NodeViewModel)args.OldItems[0];

                PreviewItems.Remove(PreviewItems.FirstOrDefault(n => n.OriginalMNode.getBase64Handle() ==
                    removedNode.OriginalMNode.getBase64Handle()));
            };

            SelectedPreview = (ImageNodeViewModel)folder.FocusedNode;
        }

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
