using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Media.PhoneExtensions;
using Telerik.Windows.Data;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    public partial class SongSelectionPage : MegaPhoneApplicationPage
    {
        public SongSelectionPage()
        {

            var songSelectioViewModel = new SongSelectionViewModel(SdkService.MegaSdk);
            this.DataContext = songSelectioViewModel;

            InitializeComponent();


            const string alphabet = "#abcdefghijklmnopqrstuvwxyz";
            var groupPickerItems = new List<string>(32);
            groupPickerItems.AddRange(alphabet.Select(c => new string(c, 1)));
            LstSongs.GroupPickerItemsSource = groupPickerItems;

            var groupBySongName = new GenericGroupDescriptor<BaseMediaViewModel<Song>, string>(s => s.Name.Substring(0, 1).ToLower());
            LstSongs.GroupDescriptors.Add(groupBySongName);
        }

        private void OnGroupPickerItemTap(object sender, Telerik.Windows.Controls.GroupPickerItemTapEventArgs e)
        {
            foreach (DataGroup group in LstSongs.Groups)
            {
                if (object.Equals(e.DataItem, group.Key))
                {
                    e.DataItemToNavigate = group;
                    return;
                }
            }

            e.ScrollToItem = false;
        }

        private  void OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //
        }
    }
}