using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MegaApp.Enums;
using MegaApp.Services;
using Microsoft.Xna.Framework.Media;

namespace MegaApp.Models
{
    class BaseMediaViewModel<T>: BaseViewModel
    {
        #region Properties

        public string Name { get; set; }
        public MediaType Type { get; set; }

        private BitmapImage _displayImage;
        public BitmapImage DisplayImage
        {
            get
            {
                if (_displayImage == null)
                {
                    var picture = BaseObject as Picture;
                    if (picture != null)
                    {
                        using (var stream = picture.GetThumbnail())
                        {
                            DisplayImage = ImageService.GetBitmapFromStream(stream, 100, 100);
                            stream.Close();
                        }
                    }
                        
                }
                return _displayImage;
            }
            set
            {
                _displayImage = value;
                OnPropertyChanged("DisplayImage");
            }
        }

        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                OnPropertyChanged("Selected");
            }
        }

        public T BaseObject { get; set; }

        #endregion
    }
}
