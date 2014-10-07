using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MegaApp.Interfaces;
using MegaApp.Models;

namespace MegaApp.Services
{
    class UiService: IUiService
    {
        public void RefreshViewport(IEnumerable<System.Windows.FrameworkElement> viewportItems)
        {
            foreach (var viewportItem in viewportItems)
            {
                ((NodeViewModel)viewportItem.DataContext).SetThumbnailImage();
            }
        }
    }
}
