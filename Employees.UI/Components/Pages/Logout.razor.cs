//using Employees.UI.Services;
//using Microsoft.AspNetCore.Components;

//namespace Employees.UI.Components.Pages
//{
//    public partial class Logout
//    {
//        [Inject]
//        public UserSession Session { get; set; }
//        UserSession session;
//        protected override void OnInitialized()
//        {
//            Session.Logout();
//            Navigation.NavigateTo("/login");
//        }
//    }
//}

//29
using Employees.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Cryptography.X509Certificates;

namespace Employees.UI.Components.Pages
{
    public partial class Logout
    {
        [Inject]
        public UserSession Session { get; set; } = default!;

        [Inject]
        public AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

        protected override void OnInitialized()
        {
            Session.Logout();

            if (AuthStateProvider is CustomAuthStateProvider authProvider)
            {
                authProvider.NotifyUserLogout();
            }

            Navigation.NavigateTo("/login");
        }
    }
}