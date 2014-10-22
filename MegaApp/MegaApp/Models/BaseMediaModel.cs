using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MegaApp.Enums;

namespace MegaApp.Models
{
    class BaseMediaModel<T>: BaseViewModel
    {
        #region Properties

        public string Name { get; set; }
        public MediaType Type { get; set; }
        public BitmapImage DisplayImage { get; set; }

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
