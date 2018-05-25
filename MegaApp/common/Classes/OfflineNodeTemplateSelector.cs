using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;
using MegaApp.ViewModels;

namespace MegaApp.Classes
{
    public class OfflineNodeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FolderItemTemplate { get; set; }
        public DataTemplate FileItemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var offlineNodeViewModel = item as OfflineNodeViewModel;

            if (offlineNodeViewModel == null) return base.SelectTemplate(item, container);

            if (offlineNodeViewModel.IsFolder)
                return FolderItemTemplate;
            else
                return FileItemTemplate;            
        }
    }
}
