using System.Collections.Generic;
using Microsoft.Phone.Shell;

namespace MegaApp.Interfaces
{
    public interface IApplicationBar
    {
        void TranslateAppBarItems(IList<ApplicationBarIconButton> iconButtons, 
            IList<ApplicationBarMenuItem> menuItems, IList<string> iconStrings, IList<string> menuStrings);
    }
}