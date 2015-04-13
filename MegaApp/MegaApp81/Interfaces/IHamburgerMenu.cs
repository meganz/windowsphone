using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using MegaApp.Classes;

namespace MegaApp.Interfaces
{
    public interface IHamburgerMenu
    {
        IList<HamburgerMenuItem> MenuItems { get; set; }  
    }
}