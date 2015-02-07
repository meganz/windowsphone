using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Models;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MegaApp.UserControls
{
    public partial class BreadCrumb : UserControl
    {
        public BreadCrumb()
        {
            InitializeComponent();

            InteractionEffectManager.AllowedTypes.Add(typeof(BreadCrumbButton));

            //Initial draw on the screen
            DrawBreadCrumb();            
        }

        #region Properties

        public event EventHandler<BreadCrumbTapEventArgs> OnBreadCrumbTap;
        public event EventHandler OnHomeTap;

        public NodeViewModel SelectedItem { get; set; }

        #endregion

        #region Dependency Properties

        public ObservableCollection<NodeViewModel> ItemsSource
        {
            get { return (ObservableCollection<NodeViewModel>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource",
            typeof(ObservableCollection<NodeViewModel>),          
            typeof(BreadCrumb),            
            new PropertyMetadata(null, new PropertyChangedCallback(OnItemsSourceChanged)));

        public ObservableCollection<NodeViewModel> Items
        {
            get { return (ObservableCollection<NodeViewModel>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            "Items",
            typeof(ObservableCollection<NodeViewModel>),         
            typeof(BreadCrumb),           
            new PropertyMetadata(null, new PropertyChangedCallback(OnItemsChanged)));        

        public string DisplayMember
        {
            get { return (string)GetValue(DisplayMemberProperty); }
            set { SetValue(DisplayMemberProperty, value); }
        }

        public static readonly DependencyProperty DisplayMemberProperty = DependencyProperty.Register(
            "DisplayMember",
            typeof(string),
            typeof(BreadCrumb),
            new PropertyMetadata(null));

        public string ValueMember
        {
            get { return (string)GetValue(ValueMemberProperty); }
            set { SetValue(ValueMemberProperty, value); }
        }

        public static readonly DependencyProperty ValueMemberProperty = DependencyProperty.Register(
            "ValueMember",
            typeof(string),
            typeof(BreadCrumb),
            new PropertyMetadata(null));

        public string RootName
        {
            get { return (string)GetValue(RootNameProperty); }
            set { SetValue(RootNameProperty, value); }
        }

        public static readonly DependencyProperty RootNameProperty = DependencyProperty.Register(
            "RootName",
            typeof(string),
            typeof(BreadCrumb),
            new PropertyMetadata("Root"));

        public ImageSource RootImage
        {
            get { return (ImageSource)GetValue(RootImageProperty); }
            set { SetValue(RootImageProperty, value); }
        }

        public static readonly DependencyProperty RootImageProperty = DependencyProperty.Register(
            "RootImage",
            typeof(ImageSource),
            typeof(BreadCrumb),
            new PropertyMetadata(null, new PropertyChangedCallback(OnRootImageSourceChanged)));

        public ImageSource SeperatorImage
        {
            get { return (ImageSource)GetValue(SeperatorImageProperty); }
            set { SetValue(SeperatorImageProperty, value); }
        }

        public static readonly DependencyProperty SeperatorImageProperty = DependencyProperty.Register(
            "SeperatorImage",
            typeof(ImageSource),
            typeof(BreadCrumb),
            new PropertyMetadata(null, new PropertyChangedCallback(OnRootImageSourceChanged)));

        public Color CrumbForegroundColor
        {
            get { return (Color)GetValue(CrumbForegroundColorProperty); }
            set { SetValue(CrumbForegroundColorProperty, value); }
        }

        public static readonly DependencyProperty CrumbForegroundColorProperty = DependencyProperty.Register(
            "CrumbForegroundColor",
            typeof(Color),
            typeof(BreadCrumb),
            new PropertyMetadata(Colors.White, new PropertyChangedCallback(OnCrumbStyleChanged)));

        public FontFamily CrumbFontFamily
        {
            get { return (FontFamily)GetValue(CrumbFontFamilyProperty); }
            set { SetValue(CrumbFontFamilyProperty, value); }
        }
        
        public static readonly DependencyProperty CrumbFontFamilyProperty = DependencyProperty.Register(
            "CrumbFontFamily",
            typeof(FontFamily),
            typeof(BreadCrumb),
            new PropertyMetadata(new FontFamily("Segoe WP"), new PropertyChangedCallback(OnCrumbStyleChanged)));

        public double CrumbFontSize
        {
            get { return (double)GetValue(CrumbFontSizeProperty); }
            set { SetValue(CrumbFontSizeProperty, value); }
        }

        public static readonly DependencyProperty CrumbFontSizeProperty = DependencyProperty.Register(
            "CrumbFontSize",
            typeof(double),
            typeof(BreadCrumb),
            new PropertyMetadata(15.0, new PropertyChangedCallback(OnCrumbStyleChanged)));

        public FontStyle CrumbFontStyle
        {
            get { return (FontStyle)GetValue(CrumbFontStyleProperty); }
            set { SetValue(CrumbFontStyleProperty, value); }
        }

        public static readonly DependencyProperty CrumbFontStyleProperty = DependencyProperty.Register(
            "CrumbFontStyle",
            typeof(FontStyle),
            typeof(BreadCrumb),
            new PropertyMetadata(FontStyles.Normal, new PropertyChangedCallback(OnCrumbStyleChanged)));        

        #endregion

        #region Private methods

        public void DrawBreadCrumb()
        {
            this.Content = null;

            var layoutRoot = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch                 
            };

            this.Content = layoutRoot;

            var stackPanel = new RadWrapPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            layoutRoot.Children.Add(stackPanel);
           
            stackPanel.Children.Add(GetHomeButton());

            if(Items != null)
            {
                if (Items.Count > 0)
                {
                    stackPanel.Children.RemoveAt(0);
                    stackPanel.Children.Add(GetHomeButtonExtended());
                }

                for(int i = 0; i <= Items.Count-2; i++)           
                {                
                    var button = new BreadCrumbButton();                   

                    button.SetValue(InteractionEffectManager.IsInteractionEnabledProperty, true);
                    
                    button.Foreground = new SolidColorBrush(CrumbForegroundColor);
                    button.FontFamily = CrumbFontFamily;
                    button.FontSize = CrumbFontSize;
                    button.FontStyle = CrumbFontStyle;                    
                    
                    button.DataContext = Items[i];

                    if(!String.IsNullOrEmpty(DisplayMember))
                        button.Content = Items[i].GetType().GetProperty(DisplayMember).GetValue(Items[i]).ToString();
                    else
                        button.Content = Items[i].ToString();

                    if(!String.IsNullOrEmpty(ValueMember))
                        button.Tag = Items[i].GetType().GetProperty(ValueMember).GetValue(Items[i]).ToString();
                    else
                        button.Tag = Items[i].ToString();
                   
                    button.Tap += OnButtonTap;
                   
                    stackPanel.Children.Add(button);
                }
            }

            var current = new TextBlock();
            if (Items != null && Items.Count > 0)
            {
                if (!String.IsNullOrEmpty(DisplayMember))
                    current.Text = Items[Items.Count - 1].GetType().GetProperty(DisplayMember).GetValue(Items[Items.Count - 1]).ToString();
                else
                    current.Text = Items[Items.Count - 1].ToString();
            }
            else
            {
                current.Text = RootName;
            }

            layoutRoot.Children.Add(current);
        }

        void OnButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (OnBreadCrumbTap == null) return;
            
            var tappedButton = (sender as Button);
            if (tappedButton == null) return;

            SelectedItem = (NodeViewModel)tappedButton.DataContext;
            var args = new BreadCrumbTapEventArgs()
            {
                Text = tappedButton.Content.ToString(),
                Value = tappedButton.Tag,
                Item = SelectedItem
            };
            OnBreadCrumbTap(this, args);
        }

        private BreadCrumbHomeButton GetHomeButton()
        {
            return new BreadCrumbHomeButton();
        }

        private BreadCrumbHomeExtended GetHomeButtonExtended()
        {
            var home = new BreadCrumbHomeExtended();
            home.Tap += HomeOnTap;
            return home;
        }

        private void HomeOnTap(object sender, GestureEventArgs gestureEventArgs)
        {
            if (OnHomeTap == null) return;
            
            var tappedButton = (sender as Button);
            if (tappedButton == null) return;

            OnHomeTap(this, new EventArgs());
        }

        #endregion

        #region Private Static Methods

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as BreadCrumb;
            if (control == null) return;
            control.Items = (ObservableCollection<NodeViewModel>)e.NewValue;
            control.Items.CollectionChanged += (sender, args) => control.DrawBreadCrumb();
            control.DrawBreadCrumb();
            //if (itemsSource != null)
            //{
            //    if (control.Items == null)
            //        control.Items = new ObservableCollection<object>();
            //    else
            //        control.Items.Clear();

            //    foreach (object o in itemsSource)
            //    {
            //        control.Items.Add(o);
            //    }
               
            //    control.DrawBreadCrumb();
            //}
            
            
            
        }

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as BreadCrumb;
            var items = (ObservableCollection<NodeViewModel>)e.NewValue;
            if (items != null)
            {
                control.DrawBreadCrumb();
            }
        }

        void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DrawBreadCrumb();
        }

        private static void OnCrumbStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as BreadCrumb;
            control.DrawBreadCrumb();            
        }

        private static void OnRootImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
