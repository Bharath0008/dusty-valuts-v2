using System.Text.Json;
using LinkedInAPI.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace LinkedInAPI.Helper
{
    public class OpenIDConnect
    {
        public static OpenIdConnectEvents Create()
        {
            var events = new OpenIdConnectEvents{
                OnAuthorizationCodeReceived = async context =>
                {
                    var requestURL = context.HttpContext.Request;
                    var baseURL = $"{requestURL.Scheme}://{requestURL.Host}";
                    var redirectUrl = $"{baseURL}/Authentication/ExternalLoginCallback";
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://www.linkedin.com/oauth/v2/accessToken")
                    {
                        Content = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            {"grant_type","authorization_code" },
                            {"code",context.ProtocolMessage.Code },
                            {"redirect_uri",redirectUrl },
                            {"client_id", context.Options.ClientId! },
                            {"client_secret",context.Options.ClientSecret!},
                        })
                    };
                    var httpClient = new HttpClient();
                    var response = await httpClient.SendAsync(request);
                    if(response.IsSuccessStatusCode)
                    {
                        var responseValue = await response.Content.ReadAsStringAsync();
                        var tokenResponse = JsonSerializer.Deserialize<LinkedInResponseModel>(responseValue);

                        context.HandleCodeRedemption(tokenResponse?.access_token!, tokenResponse?.id_token!);
                    }
                    else
                    {
                        context.Fail("Token Changing Failed:" + await response.Content.ReadAsStringAsync());
                    }
                },
                OnTokenValidated = context =>
                {
                    Console.WriteLine("Token generated Sucessfully");
                    return Task.CompletedTask;
                },
                OnRedirectToIdentityProvider = context =>
                {
                    context.ProtocolMessage.State = context.Options.StateDataFormat.Protect(context.Properties);
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    return Task.CompletedTask;
                },
                OnRemoteFailure = context =>
                {
                    context.Response.Redirect("/" + context?.Failure?.Message);
                    context?.HandleResponse();
                   return Task.CompletedTask;
                }
            };
            return events;
        }
    }
}
