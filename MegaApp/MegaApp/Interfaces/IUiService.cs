using System.Collections.Generic;
using System.Windows;

namespace MegaApp.Interfaces
{
    public interface IUiService
    {
        void RefreshViewport(IEnumerable<FrameworkElement> viewportItems);
    }
}