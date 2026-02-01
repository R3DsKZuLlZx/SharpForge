using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SharpForge.Blazor.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<CourseProgressService>();

await builder.Build().RunAsync();
