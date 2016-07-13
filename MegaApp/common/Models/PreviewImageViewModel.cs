using System;
using System.Collections;
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
        private readonly FolderViewModel _foderViewModel;

        public PreviewImageViewModel(MegaSDK megaSdk, AppInformation appInformation, FolderViewModel folderViewModel)
            : base(megaSdk, appInformation)
        {
            _foderViewModel = folderViewModel;

            if (_foderViewModel == null)
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
                _foderViewModel.ChildNodes.Where(n => n is ImageNodeViewModel).Cast<ImageNodeViewModel>());

            _foderViewModel.ChildNodes.CollectionChanged += (sender, args) =>
            {
                if (args.Action != NotifyCollectionChangedAction.Remove) return;

                var removedNode = (NodeViewModel)args.OldItems[0];

                PreviewItems.Remove(PreviewItems.FirstOrDefault(n => n.OriginalMNode.getBase64Handle() ==
                    removedNode.OriginalMNode.getBase64Handle()));
            };

            SelectedPreview = (ImageNodeViewModel)_foderViewModel.FocusedNode;
        }

        public void ChangeMenu(IList iconButtons, IList menuItems)
        {
            if (_foderViewModel.Type == ContainerType.FolderLink)
            {
                this.TranslateAppBarItems(
                    iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                    menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                    new[] { UiResources.Previous, UiResources.Download, UiResources.Import, UiResources.Next.ToLower() },
                    null);
            }
            else
            {
                this.TranslateAppBarItems(
                    iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                    menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                    new[] { UiResources.Previous, UiResources.Download, UiResources.UI_GetLink, UiResources.Next.ToLower() },
                    new[] { UiResources.Rename, UiResources.Remove });
            }            
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
