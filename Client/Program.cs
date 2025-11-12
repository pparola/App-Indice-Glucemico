using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using App_Indice_Glucemico.Client;
using App_Indice_Glucemico.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configurar HttpClient - En Blazor WebAssembly las cookies se manejan automáticamente por el navegador
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Registrar servicio de autenticación
builder.Services.AddScoped<AuthService>();

await builder.Build().RunAsync();
