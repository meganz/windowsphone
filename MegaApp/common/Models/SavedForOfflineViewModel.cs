using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Shell;

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

        public void ChangeMenu(OfflineFolderViewModel currentFolderViewModel, IList iconButtons, IList menuItems)
        {
            switch (currentFolderViewModel.CurrentDisplayMode)
            {
                case DriveDisplayMode.SavedForOffline:
                    {
                        this.TranslateAppBarItems(
                            iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                            menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                            null,
                            new[] { UiResources.Refresh, UiResources.Sort, UiResources.MultiSelect });
                        break;
                    }                
                case DriveDisplayMode.MultiSelect:
                    {
                        this.TranslateAppBarItems(
                            iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                            menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                            new[] { UiResources.Remove },
                            new[] { UiResources.Cancel });
                        break;
                    }                
                default:
                    throw new ArgumentOutOfRangeException("currentFolderViewModel");
            }
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
