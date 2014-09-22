using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaApp.Models
{
    class DownloadImageViewModel : BaseViewModel
    {
        private readonly CloudDriveViewModel _cloudDriveViewModel;
        public DownloadImageViewModel(CloudDriveViewModel cloudDriveViewModel)
        {
            _cloudDriveViewModel = cloudDriveViewModel;
        }

        #region Properties

        public List<NodeViewModel> DownloadItems
        {
            get { return _cloudDriveViewModel.ChildNodes.Where(n => n.IsImage).ToList(); }
        }

        private NodeViewModel _selectedDownload;

        public NodeViewModel SelectedDownload
        {
            get { return _selectedDownload; }
            set
            {
                _selectedDownload = value;
                _selectedDownload.SetImage();
                OnPropertyChanged("SelectedDownload");
            }
        }
        #endregion
    }
}
