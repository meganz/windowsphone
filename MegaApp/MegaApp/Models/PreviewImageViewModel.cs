using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Storage;
using mega;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Reactive;
using Microsoft.Phone.Tasks;

namespace MegaApp.Models
{
    class PreviewImageViewModel : BaseSdkViewModel
    {
        private readonly CloudDriveViewModel _cloudDriveViewModel;
        public PreviewImageViewModel(MegaSDK megaSdk, CloudDriveViewModel cloudDriveViewModel)
            : base(megaSdk)
        {
            _cloudDriveViewModel = cloudDriveViewModel;
            GetPreviewsFromCache();
        }

        #region Methods

        public void GetPreviewLink()
        {
            if (!IsUserOnline()) return;

            MegaService.GetPreviewLink(this.MegaSdk, SelectedPreview);
        }

        private void GetPreviewsFromCache()
        {
            foreach (var previewItem in PreviewItems.Where(p => p.HasPreviewInCache()))
            {
                previewItem.LoadPreviewImage(previewItem.PreviewPath);
            }

            foreach (var previewItem in PreviewItems.Where(p => p.PreviewImage == null && !p.ThumbnailIsDefaultImage))
            {
                previewItem.PreviewImage = previewItem.ThumbnailImage;
            }
        }

        #endregion

        private void PreloadPreviews(NodeViewModel selectedPreview)
        {
            selectedPreview.SetPreviewImage();
            int previousIndex = PreviewItems.IndexOf(selectedPreview) - 1;
            if(previousIndex >= 0)
                PreviewItems[previousIndex].SetPreviewImage();
            int nextIndex = PreviewItems.IndexOf(selectedPreview) + 1;
            if (nextIndex <= PreviewItems.Count-1)
                PreviewItems[nextIndex].SetPreviewImage(); 
        }

        #region Properties

        public List<NodeViewModel> PreviewItems
        {
            get { return _cloudDriveViewModel.ChildNodes.Where(n => n.IsImage || n.GetBaseNode().hasPreview()).ToList(); }
        }

        private NodeViewModel _selectedPreview;

        public NodeViewModel SelectedPreview
        {
            get { return _selectedPreview; }
            set
            {
                _selectedPreview = value;
                PreloadPreviews(_selectedPreview);
                OnPropertyChanged("SelectedPreview");
            }
        }
        #endregion
    }
}
