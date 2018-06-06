using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;
using mega;
using MegaApp.ViewModels;

namespace MegaApp.Classes
{
    public class NodeTemplateSelector: DataTemplateSelector
    {
        public DataTemplate FolderItemTemplate { get; set; }
        public DataTemplate FileItemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var nodeViewModel = item as NodeViewModel;

            if (nodeViewModel == null) return base.SelectTemplate(item, container);

            switch (nodeViewModel.Type)
            {
                case MNodeType.TYPE_FOLDER:
                {
                    return FolderItemTemplate;
                }
                default:
                {
                    return FileItemTemplate;
                }
            }
        }
        
    }
}
