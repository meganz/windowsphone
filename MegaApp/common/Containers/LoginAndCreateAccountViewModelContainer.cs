using System;
using MegaApp.Models;
using MegaApp.Pages;

namespace MegaApp.Containers
{
    class LoginAndCreateAccountViewModelContainer
    {
        public LoginViewModel LoginViewModel { get; private set; }
        public CreateAccountViewModel CreateAccountViewModel { get; private set; }

        public LoginAndCreateAccountViewModelContainer(LoginPage loginPage)
        {
            LoginViewModel = new LoginViewModel(App.MegaSdk, loginPage);
            CreateAccountViewModel = new CreateAccountViewModel(App.MegaSdk, loginPage);
        }
    }
}
