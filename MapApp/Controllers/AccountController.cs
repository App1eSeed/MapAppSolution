using MapApp.Models.EF;
using MapApp.Models.EF.Entities;
using MapApp.Models.QueryModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MapApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;      

        public IActionResult LoginReroute(string ReturnUrl)
        {
            ViewBag.OpenAuthorizationModal = true;
            ViewBag.ReturnUrl = ReturnUrl;
            return View("../Home/Index");
        }

        //private MapAppContext db;
        //public AccountController(MapAppContext context)
        //{
        //    db = context;
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (ModelState.IsValid)
            {
                using (var db = new MapAppContext())
                {
                    User user = await db.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
                    if (user != null)
                    {
                        await Authenticate(user); // аутентификация

                        return RedirectToAction("Index", "Home");
                    }
                    ModelState.AddModelError("", "Incorrect username and/or password");
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet("login/{provider}")]
        public IActionResult LoginExternal([FromRoute] string provider, [FromQuery] string returnUrl)
        {
            if (User != null && User.Identities.Any(identity => identity.IsAuthenticated))
            {
                return RedirectToAction("", "Home");
            }

            // By default the client will be redirect back to the URL that issued the challenge (/login?authtype=foo),
            // send them to the home page instead (/).
            returnUrl = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;
            var authenticationProperties = new AuthenticationProperties { RedirectUri = returnUrl };
            // authenticationProperties.SetParameter("prompt", "select_account");
            return new ChallengeResult(provider, authenticationProperties);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new MapAppContext())
                {
                    User user = await db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                    if (user == null)
                    {
                        // добавляем пользователя в бд
                        User newUser;
                        db.Users.Add(newUser = new User
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserRoleId = "1",
                            Email = model.Email,
                            Password = model.Password,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            DateOfBirth = model.DateOfBirth,
                            PhoneNumber = model.PhoneNumber
                        });
                        await db.SaveChangesAsync();

                        await Authenticate(newUser); // аутентификация

                        return RedirectToAction("Index", "Home");
                    }
                
                    else
                        ModelState.AddModelError("", "Incorrect username and/or password");
                }
            }
            return View(model);
        }

        private async Task Authenticate(User user)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim("preferred_username", user.Email),
                new Claim("name", user.FirstName + " " + user.LastName),
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
