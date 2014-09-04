using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaApp.Models
{
    class MyAccountPageViewModel:BaseViewModel
    {
        public MyAccountPageViewModel()
        {
            //
        }

        #region Properties

        private string _userName;

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged("UserName");
            }
        }

        #endregion
    }
}
