using System;
using System.Collections;
using System.Linq;
using Microsoft.Phone.Shell;
using MegaApp.Models;
using MegaApp.Pages;
using MegaApp.Resources;

namespace MegaApp.Containers
{
    class LoginAndCreateAccountViewModelContainer : BaseSdkViewModel
    {
        public LoginViewModel LoginViewModel { get; private set; }
        public CreateAccountViewModel CreateAccountViewModel { get; private set; }

        public LoginAndCreateAccountViewModelContainer(LoginPage loginPage)
            :base(App.MegaSdk)
        {
            LoginViewModel = new LoginViewModel(App.MegaSdk, loginPage);
            CreateAccountViewModel = new CreateAccountViewModel(App.MegaSdk, loginPage);
        }

        public void ChangeMenu(IList iconButtons, IList menuItems)
        {
            this.TranslateAppBarItems(
                iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                new[] { UiResources.Accept, UiResources.Cancel },
                null);
        }
    }
}
