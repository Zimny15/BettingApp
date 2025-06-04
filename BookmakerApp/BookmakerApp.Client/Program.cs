using BookmakerApp.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddScoped(sp =>
{
    var handler = new HttpClientHandler
    {
        UseCookies = true,
        UseDefaultCredentials = true
    };

    return new HttpClient(handler)
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    };
});


await builder.Build().RunAsync();
