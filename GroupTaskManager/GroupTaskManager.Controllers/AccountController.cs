using GroupTaskManager.GroupTaskManager.Database;
using GroupTaskManager.GroupTaskManager.Services.Interface;
using GroupTaskManager.Models;
using GroupTaskManager.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GroupTaskManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRegister _register;
        private readonly ILoginLogout _loginlogout;
        private readonly ISendEmail _sendEmail;
        private readonly IFunctionsFromEmail _functionsEmail;
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;
        public AccountController(IRegister register, ILoginLogout loginlogout, ISendEmail sendEmail, IFunctionsFromEmail functionsEmail, UserManager<UserModel> userManager, SignInManager<UserModel> signInManager)
        {
            _register = register;
            _loginlogout = loginlogout;
            _sendEmail = sendEmail;
            _functionsEmail = functionsEmail;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(string Firstname, string Lastname, string PhoneNumber, string Email, string Password)
        {
            if (await _register.Register(Firstname, Lastname, PhoneNumber, Email, Password))
            {
                await _sendEmail.SendConfirmedEmail(Email);
                return RedirectToAction("PleaseCheckEmail", new { Email = Email });
            }
            else
            {
                return RedirectToAction("ErrorAction", "Account", new { errorinfo = "Register" });
            }

        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            if (await _loginlogout.Login(Email, Password))
            {
                return RedirectToAction("SuccessfulLogin");
            }
            else
            {
                return RedirectToAction("ErrorAction", "Account", new { errorinfo = "Login" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            if(await _loginlogout.Logout())
            {
                return RedirectToAction("LogoutInfo"); 
            }
            else
            {
                return RedirectToAction("ErrorAction", "Account", new { errorinfo = "Logout" });
            }
        }

        [HttpGet]
        public IActionResult LogoutInfo()
        {
            return View();
        }




        [HttpGet]
        public IActionResult ResetPassword(string code, string Email)
        {
            ViewBag.Email = Email;
            ViewBag.Code = code;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string code, string Email, string Password)
        {
            await _functionsEmail.ResetPassword(code, Email, Password);
            return RedirectToAction("Login");
        }









        [HttpGet]
        public IActionResult ErrorAction(string errorinfo)
        {
            ViewBag.ErrorInfo = errorinfo;
            return View();
        }

        [HttpGet]
        public IActionResult SuccessfulLogin()
        {
            return View();
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            await _sendEmail.SendResetPasswordEmail(Email);
            return RedirectToAction("ForgotPasswordInform", new { Email = Email });

        }
        [HttpGet]
        public IActionResult ForgotPasswordInform(string Email)
        {
            ViewBag.Email = Email;
            return View();
        }




        [HttpGet]
        public IActionResult PleaseCheckEmail(string Email)
        {
            ViewBag.Email = Email;
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string code, string Email)
        {
            ViewBag.Code = code;
            ViewBag.Email = Email;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmedEmail(string code, string Email)
        {
            await _functionsEmail.ConfirmedEmail(code, Email);
            return RedirectToAction("ConfirmedEmail");
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmedEmail()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ResendConfirmationEmail(string Email)
        {
            await _sendEmail.SendConfirmedEmail(Email);
            ViewBag.Email = Email;
            return View();
        }
    }
}
