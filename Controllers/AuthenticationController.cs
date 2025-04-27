using System.Text.Json;
using LinkedInAPI.DBModels;
using LinkedInAPI.Helper;
using LinkedInAPI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LinkedInAPI.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly LoginContext _dbContext;
        private readonly MailService _mailService;
        public AuthenticationController(LoginContext dbContext, MailService mailService)
        {
            _dbContext = dbContext;
            _mailService = mailService;
        }
        [HttpPost]
        public IActionResult SignIn()
        {
            if (User.Identity.IsAuthenticated)
            {
               return RedirectToAction("HomePage"); // Or some other action like RedirectToAction("Dashboard")
            }
            var redirectUrl = "https://localhost:44325/Authentication/ExternalLoginCallback";
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);

        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> ExternalLoginCallback()
        {
            var authenticationResult = await HttpContext.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);
            if (!authenticationResult.Succeeded)
            {
                TempData["ErrorMesssage"] = "Authentication Failed";
                return RedirectToAction("/");
            }
            string providerName = "LinkedIn";
            var accessToken = authenticationResult.Properties.GetTokenValue("access_token");
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var userInfo = await httpClient.GetAsync("https://api.linkedin.com/v2/userinfo");
            if (!userInfo.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Authentication Provider Sorry";
                return RedirectToAction("/");
            }
            if (userInfo.IsSuccessStatusCode)
            {
                var userProfileInfo = await userInfo.Content.ReadAsStringAsync();
                var userProfile = JsonSerializer.Deserialize<Root>(userProfileInfo);
                var existingUser = _dbContext.LoginData.FirstOrDefault(x => x.Email == userProfile!.email && x.ProviderName !=providerName);
                if(existingUser == null)
                {
                    var newUser = new LoginData
                    {
                        Name = userProfile!.name,
                        Email = userProfile.email,
                        ProviderName = providerName,
                    };
                    _mailService.SendEmailAsync(userProfile.email, "Socail Login Successfully", "Thank you for signing up!");
                    _dbContext.LoginData.Add(newUser);
                    await _dbContext.SaveChangesAsync();

                }
                 var userData = new GoogleDataModel
                {
                    Name = userProfile!.name,
                    Email = userProfile.email,
                    Profile = userProfile.picture,
                };
                return View("~/Views/HomePage.cshtml", userData);
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
public class Root
{
    public string sub { get; set; }
    public bool email_verified { get; set; }
    public string name { get; set; }
    public string given_name { get; set; }
    public string family_name { get; set; }
    public string email { get; set; }
    public string picture { get; set; }
}