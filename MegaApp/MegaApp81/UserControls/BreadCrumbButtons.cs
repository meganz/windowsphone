using System;
using System.Windows;
using System.Windows.Controls;


namespace MegaApp.UserControls
{
    public class BreadCrumbButton: Button
    {
        public BreadCrumbButton()
        {

            this.Style = new BreadCrumbStyle().Resources["BreadCrumbStyle"] as Style;

            this.ContentTemplate = new BreadCrumbStyle().Resources["BreadCrumbContentTemplate"] as DataTemplate;
        }

    }
     
    public class BreadCrumbHomeButton : Button
    {
        public BreadCrumbHomeButton()
        {
            this.Style = new BreadCrumbStyle().Resources["BreadCrumbStyle"] as Style;

            this.ContentTemplate = new BreadCrumbStyle().Resources["BreadCrumbHomeContentTemplate"] as DataTemplate;

            this.IsEnabled = false;
        }

    }

    public class BreadCrumbHomeExtended : Button
    {
        public BreadCrumbHomeExtended()
        {
            this.Style = new BreadCrumbStyle().Resources["BreadCrumbStyle"] as Style;

            this.ContentTemplate = new BreadCrumbStyle().Resources["BreadCrumbHomeExtendedContentTemplate"] as DataTemplate;
        }

    }
}
