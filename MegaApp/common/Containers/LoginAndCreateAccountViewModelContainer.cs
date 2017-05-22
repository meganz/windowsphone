using System;
using System.Collections;
using System.Linq;
using Microsoft.Phone.Shell;
using MegaApp.Models;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Containers
{
    class LoginAndCreateAccountViewModelContainer : BaseSdkViewModel
    {
        public LoginViewModel LoginViewModel { get; private set; }
        public CreateAccountViewModel CreateAccountViewModel { get; private set; }

        public LoginAndCreateAccountViewModelContainer(LoginPage loginPage)
            :base(SdkService.MegaSdk)
        {
            LoginViewModel = new LoginViewModel(SdkService.MegaSdk, loginPage);
            CreateAccountViewModel = new CreateAccountViewModel(SdkService.MegaSdk, loginPage);
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
