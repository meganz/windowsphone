using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Resources;

namespace MegaApp.UserControls
{
    public partial class BreadCrumb : UserControl
    {
        public BreadCrumb()
        {
            InitializeComponent();
        }

        #region Dependency Properties

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource",
            typeof(ObservableCollection<IMegaNode>),
            typeof(BreadCrumb),
            new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty ItemsSourceTypeProperty = DependencyProperty.Register(
            "ItemsSourceType",
            typeof(ContainerType),
            typeof(BreadCrumb),
            new PropertyMetadata(ContainerType.CloudDrive, OnItemsSourceTypeChanged));

        #endregion

        #region Events

        public event EventHandler HomeTap;
        public event EventHandler<BreadCrumbTapEventArgs> BreadCrumbTap;

        #endregion

        #region Private Methods

        private void Draw()
        {
            this.Content = null;
            
            var mainRoot = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0),
            };
            this.Content = mainRoot;

            DrawMegaHomeButton(mainRoot, this.ItemsSource != null && this.ItemsSource.Count > 0);

            if (this.ItemsSource == null || this.ItemsSource.Count == 0) return;

            var buttonList = new List<Button>(this.ItemsSource.Count);

            foreach (var megaNode in ItemsSource)
            {
                buttonList.Add(DrawMegaFolderButton(mainRoot, megaNode));
                this.UpdateLayout();

                int i = 0;
                while (mainRoot.ActualWidth > this.ActualWidth)
                {
                    if(i < buttonList.Count - 1)
                    {
                        buttonList[i].Content = "...";
                        i++;
                    }
                    else
                    {
                        mainRoot.Children.RemoveAt(1);
                    }
                    this.UpdateLayout();
                }
            }
            
        }

        private void DrawMegaHomeButton(Panel parentControl, bool allowTap)
        {
            var homeButton = new Button();

            if(ItemsSourceType == ContainerType.RubbishBin)
                homeButton.Style = (Style)Application.Current.Resources["RubbishBinHomeCrumbStyle"];
            else
                homeButton.Style = (Style)Application.Current.Resources["CloudDriveHomeCrumbStyle"];

            if (allowTap)
            {
                homeButton.Tap += (sender, args) => OnMegaHomeTap();
            }

            parentControl.Children.Add(homeButton);
        }

        private Button DrawMegaFolderButton(Panel parentControl, IMegaNode node)
        {
            var folderButton = new Button
            {
                Style = (Style) Application.Current.Resources["BreadCrumbStyle"],
                Content = node.Name
            };
            folderButton.Tap += (sender, args) => OnBreadCrumbTap(node);

            parentControl.Children.Add(folderButton);

            return folderButton;
        }

        private void OnMegaHomeTap()
        {
            if (HomeTap == null) return;
            HomeTap(this, new EventArgs());
        }

        private void OnBreadCrumbTap(IMegaNode selectedNode)
        {
            if (BreadCrumbTap == null) return;
            BreadCrumbTap(this, new BreadCrumbTapEventArgs()
            {
                Item = selectedNode
            });
        }

        #endregion

        #region Properties

        public ObservableCollection<IMegaNode> ItemsSource
        {
            get { return (ObservableCollection<IMegaNode>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public ContainerType ItemsSourceType
        {
            get { return (ContainerType)GetValue(ItemsSourceTypeProperty); }
            set { SetValue(ItemsSourceTypeProperty, value); }
        }

        #endregion

        #region Private Static Methods

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var breadCrumb = d as BreadCrumb;
           
            if (breadCrumb == null) return;
            if (breadCrumb.ItemsSource == null) return;

            breadCrumb.Draw();

            breadCrumb.ItemsSource.CollectionChanged += (sender, args) =>
            {
                breadCrumb.Draw();
            };

        }

        private static void OnItemsSourceTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var breadCrumb = d as BreadCrumb;

            if (breadCrumb == null) return;
            if (breadCrumb.ItemsSource == null) return;

            breadCrumb.Draw();
        }

        #endregion
    }
}
