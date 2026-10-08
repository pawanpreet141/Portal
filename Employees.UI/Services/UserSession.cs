using Microsoft.AspNetCore.Identity;

namespace Employees.UI.Services
{
    public class UserSession
    {
        public int UserId { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string Email { get; private set; } = string.Empty;

        public string Department { get; private set; } = string.Empty;

        public string Token { get; private set; } = string.Empty;


        public bool IsLoggedIn =>
            UserId > 0 && !string.IsNullOrEmpty(Token);
        

        public void Login(
            int userId,
            string name,
            string email,
            string department,
             string token)
        {
            UserId = userId;

            Name = name;

            Email = email;

            Department = department;

            Token = token;
        }


        public void Logout()
        {
            UserId = 0;

            Name = string.Empty;

            Email = string.Empty;

            Department = string.Empty;

            Token = string.Empty;

        }
    }
}


