using LinkedInAPI.DBModels;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LinkedInAPI.Models;

namespace LinkedInAPI.Controllers
{
    public class GoogleAccountController : Controller
    {
        private readonly LoginContext _dbContext;
        public GoogleAccountController(LoginContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpPost]
        public IActionResult Login()
        {
            var redirectUrl = Url.Action("GoogleMailResponse", "GoogleAccount");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GoogleMailResponse()
        {
            string providerName = "Gmail";
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (result.Succeeded)
            {
                var claims = result.Principal.Identities.First().Claims;

                var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var picture = claims?.First(e => e.Type.Contains("picture")).Value;
                var existingUser = _dbContext.LoginData.FirstOrDefault(x => x.Email == email && x.ProviderName == providerName);
                var userData = new GoogleDataModel
                {
                    Name = name,
                    Email = email,
                    Profile = picture,
                };
                if (existingUser == null)
                {
                    var newUser = new LoginData
                    {
                        Name = name,
                        Email = email,
                        ProviderName = providerName,
                    };
                    _dbContext.LoginData.Add(newUser);
                    await _dbContext.SaveChangesAsync();

                }
                return View("~/Views/GoogleHomePage.cshtml", userData);
            }
            return Redirect("/");
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/");
        }
    }
}
