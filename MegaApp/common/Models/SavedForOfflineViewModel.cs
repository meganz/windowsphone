using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MegaApp.Enums;
using MegaApp.Services;

namespace MegaApp.Models
{
    public class SavedForOfflineViewModel : BaseAppInfoAwareViewModel
    {
        public SavedForOfflineViewModel() 
            : base(App.MegaSdk, App.AppInformation)
        {
            this.SavedForOffline = new OfflineFolderViewModel();

            UpdateUserData();

            InitializeMenu(HamburgerMenuItemType.SavedForOffline);
        }

        #region Public methods

        #endregion

        public void LoadFolders()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (this.SavedForOffline.FolderRootNode == null)
                {
                    this.SavedForOffline.FolderRootNode = 
                        new OfflineFolderNodeViewModel(new DirectoryInfo(AppService.GetDownloadDirectoryPath()));
                }
                    

                this.SavedForOffline.LoadChildNodes();
            });
        }

        #region Properties

        private OfflineFolderViewModel _savedForOffline;
        public OfflineFolderViewModel SavedForOffline
        {
            get { return _savedForOffline; }
            set { SetField(ref _savedForOffline, value); }
        }

        #endregion
    }
}
