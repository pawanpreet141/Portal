using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Employees.UI.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly UserSession _userSession;

        public CustomAuthStateProvider(UserSession userSession)
        {
            _userSession = userSession;
        }

        public override Task<AuthenticationState>
            GetAuthenticationStateAsync()
        {
            if (!_userSession.IsLoggedIn)
            {
                return Task.FromResult(
                    new AuthenticationState(
                        new ClaimsPrincipal(
                            new ClaimsIdentity())));
            }

            var claims = GetClaimsFromToken(
                _userSession.Token);

            var identity = new ClaimsIdentity(
                claims,
                "jwt");

            var user = new ClaimsPrincipal(identity);

            return Task.FromResult(
                new AuthenticationState(user));
        }

        public void NotifyUserLogin()
        {
            NotifyAuthenticationStateChanged(
                GetAuthenticationStateAsync());
        }

        public void NotifyUserLogout()
        {
            NotifyAuthenticationStateChanged(
                GetAuthenticationStateAsync());
        }

        private IEnumerable<Claim> GetClaimsFromToken(
            string token)
        {
            var handler = new JwtSecurityTokenHandler();

            var jwtToken =
                handler.ReadJwtToken(token);

            return jwtToken.Claims;
        }
    }
}