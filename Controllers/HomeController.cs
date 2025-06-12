using System.Diagnostics;
using System.Security.Claims;
using japantune.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace japantune.Controllers
{
    public class HomeController : Controller
    {
        private readonly JapanTuneContext db;

        public HomeController(JapanTuneContext context)
        {
            db = context;
        }

        public IActionResult Login()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await db.Users.FirstOrDefaultAsync(u => u.ClientLogin == model.Login && u.ClientPassword == model.Password);
                if (user != null)
                {
                    await Authenticate(model.Login);
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Неверный логин или пароль");
            }
            return View(model);
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                if (await db.Users.AnyAsync(u => u.ClientLogin == model.Login))
                {
                    ModelState.AddModelError("Login", "Пользователь с таким логином уже существует");
                    return View(model);
                }

                var user = new User
                {
                    ClientLogin = model.Login,
                    ClientPassword = model.Password,
                    FirstName = model.FirstName,
                    SurName = model.SurName,
                    PhoneNumber = model.PhoneNumber,
                    RoleId = 2
                };

                db.Users.Add(user);
                await db.SaveChangesAsync();
                await Authenticate(model.Login);
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        private async Task Authenticate(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };

            var id = new ClaimsIdentity(
                claims,
                "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Home");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}