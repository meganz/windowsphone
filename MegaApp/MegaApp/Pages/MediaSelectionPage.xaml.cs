using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Models;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;

namespace MegaApp.Pages
{
    public partial class MediaSelectionPage : PhoneApplicationPage
    {
        public MediaSelectionPage()
        {
            var mediaSelectionPageModel = new MediaSelectionPageModel(App.MegaSdk);
            this.DataContext = mediaSelectionPageModel;
            InitializeComponent();

            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));
        }

        private void OnAcceptClick(object sender, System.EventArgs e)
        {
        	// TODO: Add event handler implementation here.
        }

        private void OnItemStateChanged(object sender, Telerik.Windows.Controls.ItemStateChangedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
        }
    }
}