using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Services;
using Microsoft.Xna.Framework.Media;

namespace MegaApp.ViewModels
{
    class SongSelectionViewModel: BaseSdkViewModel
    {

        public SongSelectionViewModel(MegaSDK megaSdk)
            : base(megaSdk)
        {
            Songs = new ObservableCollection<BaseMediaViewModel<Song>>(MediaService.GetSongs());
        }

        #region Properties
        
        public ObservableCollection<BaseMediaViewModel<Song>> Songs { get; set; }

        #endregion
    }
}
