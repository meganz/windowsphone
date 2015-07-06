using System;
using MegaApp.Models;

namespace MegaApp.Containers
{
    class LoginAndCreateAccountViewModelContainer
    {
        public LoginViewModel LoginViewModel { get; private set; }
        public CreateAccountViewModel CreateAccountViewModel { get; private set; }

        public LoginAndCreateAccountViewModelContainer()
        {
            LoginViewModel = new LoginViewModel(App.MegaSdk);
            CreateAccountViewModel = new CreateAccountViewModel(App.MegaSdk);
        }
    }
}
