using Livraria.Blazor;
using Livraria.Blazor.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configurar HttpClient para consumir a API
// Usa a URL base do host (funciona com proxy nginx em Docker)
var configuredAddress = builder.Configuration["ApiBaseAddress"];
var baseAddress = string.IsNullOrEmpty(configuredAddress) || configuredAddress == "/"
    ? builder.HostEnvironment.BaseAddress
    : configuredAddress;
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });

// Registrar Services HTTP
builder.Services.AddScoped<ILivroService, LivroService>();
builder.Services.AddScoped<IAutorService, AutorService>();
builder.Services.AddScoped<IAssuntoService, AssuntoService>();
builder.Services.AddScoped<IFormaCompraService, FormaCompraService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();

await builder.Build().RunAsync();