using GroupTaskManager.Models;
using GroupTaskManager.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GroupTaskManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IRegister _register;
        private readonly ILoginLogout _loginLogout;

        public AccountController(ILogger<AccountController> logger, IRegister register, ILoginLogout loginLogout)
        {
            _logger = logger;
            _register = register;
            _loginLogout = loginLogout;
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
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("ErrorAction", "Account", new { errorinfo = "Register" });
            }

        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            if (await _loginLogout.Login(Email, Password))
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("ErrorAction", "Account", new { errorinfo = "Login" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            if(await _loginLogout.Logout())
            {
                return RedirectToAction("Index", "Home");
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
        public IActionResult ErrorAction(string errorinfo)
        {
            ViewBag.ErrorInfo = errorinfo;
            return View();
        }

    }
}
