using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Phone.Reactive;

namespace MegaApp.Models
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        protected BaseViewModel()
        {
            this.ControlState = true;
            this.IsBusy = false;
        }

        #region Protected Methods

        /// <summary>
        /// Invoke the code/action on the UI Thread. If not on UI thread, dispatch to UI with the Dispatcher
        /// </summary>
        /// <param name="action">Action to invoke on the user interface thread</param>
        protected static void OnUiThread(Action action)
        {
            // If no action then do nothing and return
            if(action == null) return;


            if (Deployment.Current.Dispatcher.CheckAccess())
            {
                // We are already on UI thread. Just invoke the action
                action.Invoke();
            }
            else
            {
                // We are on a background thread. Dispatch the action to the UI thread
                Deployment.Current.Dispatcher.BeginInvoke(action);
            }
        }

        #endregion

        #region Properties

        private bool _controlState;
        public bool ControlState
        {
            get { return _controlState; }
            set { SetField(ref _controlState, value); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetField(ref _isBusy, value); }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        // Example to use 'SetField' in properties
        // private string name;
        // public string Name
        // {
        //     get { return name; }
        //     set { SetField(ref name, value); }
        // }
        // [CallerMemberName] will add the property name automatic on compilation

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}
