using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Reactive;

namespace MegaApp.Models
{
    class PreviewImageViewModel: BaseViewModel
    {
        private readonly CloudDriveViewModel _cloudDriveViewModel;
        public PreviewImageViewModel(CloudDriveViewModel cloudDriveViewModel)
        {
            _cloudDriveViewModel = cloudDriveViewModel;
        }


        #region Properties

        public List<NodeViewModel> PreviewItems
        {
            get { return _cloudDriveViewModel.ChildNodes.Where(n => n.IsImage).ToList(); }
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
