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
using MegaApp.Resources;
using Microsoft.Phone.Reactive;
using Microsoft.Phone.Tasks;

namespace MegaApp.Models
{
    class PreviewImageViewModel: BaseViewModel
    {
        private readonly CloudDriveViewModel _cloudDriveViewModel;
        public PreviewImageViewModel(CloudDriveViewModel cloudDriveViewModel)
        {
            _cloudDriveViewModel = cloudDriveViewModel;
        }

        #region Methods

       
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
            get { return _cloudDriveViewModel.ChildNodes.Where(n => n.IsImage && n.GetBaseNode().hasPreview()).ToList(); }
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
