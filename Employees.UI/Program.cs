using Employees.UI.Components;
using Employees.UI.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddHttpClient<HttpClientWrapper>(client =>
{
    //client.BaseAddress = new Uri("https://localhost:44347/");
    client.BaseAddress = new Uri("https://localhost:44397/");
    // client.BaseAddress = new Uri("https://localhost:7219/");
});


builder.Services.AddSingleton<UserSession>();


builder.Services.AddScoped<
    AuthenticationStateProvider,
    CustomAuthStateProvider>();

builder.Services.AddAuthorizationCore();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.Run();