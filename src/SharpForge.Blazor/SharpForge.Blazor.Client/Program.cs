using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SharpForge.Blazor.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<ThemeService>();

await builder.Build().RunAsync();
