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

        public void SharePreview()
        {
            //string path = SelectedPreview.PreviewImage.UriSource.ToString();
            //var shareMediaTask = new ShareMediaTask
            //{
            //    FilePath = path // Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.PreviewsDirectory, SelectedPreview.GetBaseNode().getBase64Handle())
            //};
            //shareMediaTask.Show();
        }

        #endregion


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
                _selectedPreview.SetPreviewImage();
                OnPropertyChanged("SelectedPreview");
            }
        }
        #endregion
    }
}
