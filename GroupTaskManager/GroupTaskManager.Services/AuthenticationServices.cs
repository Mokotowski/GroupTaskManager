using GroupTaskManager.GroupTaskManager.Database;
using GroupTaskManager.Services.Interface;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GroupTaskManager.GroupTaskManager.Services
{
    public class AuthenticationServices : IRegister, ILoginLogout
    {
        private readonly SignInManager<UserModel> _signInManager;
        private readonly UserManager<UserModel> _userManager;
        private readonly ILogger<AuthenticationServices> _logger;

        public AuthenticationServices(UserManager<UserModel> userManager, SignInManager<UserModel> signInManager, ILogger<AuthenticationServices> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<bool> Register(string Firstname, string Lastname, string PhoneNumber, string Email, string Password)
        {
            var existingUserByEmail = await _userManager.FindByEmailAsync(Email);
            if (existingUserByEmail != null)
            {
                _logger.LogWarning("User with email {Email} already exists.", Email);
                return false;
            }

            bool phoneNumberExists = await _userManager.Users.AnyAsync(u => u.PhoneNumber == PhoneNumber);
            if (phoneNumberExists)
            {
                _logger.LogWarning("User with phone number {PhoneNumber} already exists.", PhoneNumber);
                return false;
            }

            var user = new UserModel
            {
                Firstname = Firstname,
                Lastname = Lastname,
                UserName = Email,
                Email = Email,
                PhoneNumber = PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} created successfully with Name: {FirstName} {LastName}.", Email, Firstname, Lastname);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to create user {Email}. Errors: {Errors}", Email, string.Join(", ", result.Errors));
                return false;
            }
        }


        public async Task<bool> Login(string Email, string Password)
        {
            _logger.LogInformation("Login attempt for user: {Nick}", Email);

            var result = await _signInManager.PasswordSignInAsync(Email, Password, true, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Nick} successfully logged in.", Email);
                return true;

            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User {Nick} is locked out due to too many failed login attempts.", Email);
                return false;
            }
            else if (result.IsNotAllowed)
            {
                _logger.LogWarning("Login attempt by user {Nick} is not allowed.", Email);
                return false;
            }
            else if (result.RequiresTwoFactor)
            {
                _logger.LogWarning("User {Nick} requires two-factor authentication.", Email);
                return false;
            }
            else
            {
                _logger.LogWarning("Failed login attempt for user: {Nick}", Email);
                _logger.LogWarning("Sign-in result: {Result}", result);
                return false;
            }
        }


        public async Task<bool> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation("User successfully logged out at {Time}.", DateTime.UtcNow);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging out the user at {Time}.", DateTime.UtcNow);
                return false;
            }
        }
    }
}
