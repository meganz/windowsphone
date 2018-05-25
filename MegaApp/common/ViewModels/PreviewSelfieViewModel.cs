using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MegaApp.ViewModels
{
    class PreviewSelfieViewModel : BaseViewModel
    {
        public PreviewSelfieViewModel(BitmapImage selfieImage)
        {
            Selfie = selfieImage;
        }

        #region Properties

        public BitmapImage Selfie { get; set; }

        #endregion
    }
}
