using LinkedInAPI.DBModels;
using LinkedInAPI.Helper;
using LinkedInAPI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("LinkedApi") ??
    Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
var conStr = builder.Configuration.GetConnectionString("LinkedApi");
var oAuth =builder.Configuration["LinkedIn:OAuth"];
var authorization = builder.Configuration["LinkedIn:Authorization"];
var accessToken = builder.Configuration["LinkedIn:AccessToken"];
var linkedInClientId = builder.Configuration["LinkedIn:LinkedInClientId"];
var linkedInSecretKey = builder.Configuration["LinkedIn:LinkedInSecretKey"];
builder.Services.AddDbContext<LoginContext>(options => options.UseSqlServer(connectionString));
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddTransient<MailService>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.ClientId = linkedInClientId;
    options.ClientSecret = linkedInSecretKey;
    options.Authority = oAuth;
    options.SaveTokens = true;
    options.Configuration = new OpenIdConnectConfiguration
    {
        AuthorizationEndpoint = authorization,
        TokenEndpoint = accessToken,
    };
    options.SkipUnrecognizedRequests = true;
    options.ResponseType = "code";
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");

    options.CallbackPath = new PathString("/Authentication/ExternalLoginCallback");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = oAuth,
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        SignatureValidator = (token, parameters) => new JsonWebToken(token),

    };
    options.Events = OpenIDConnect.Create();
})
.AddGoogle(options =>
 {
     options.ClientId = builder.Configuration["Google:ClientId"]!;
     options.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
     options.CallbackPath = new PathString("/signin-google");
     options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
 });

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
