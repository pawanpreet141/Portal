using Employees.UI.Components;
using Employees.UI.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Http.Resilience;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

//builder.Services.AddHttpClient<HttpClientWrapper>(client =>
//{
//    client.BaseAddress =
//        new Uri("https://localhost:44397/");
//});

// Use polly
builder.Services
    .AddHttpClient<HttpClientWrapper>(client =>
    {
        client.BaseAddress =
            new Uri("https://localhost:44397/");
    })
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 3;

        // Prevent automatic retries for write operations.
        options.Retry.DisableForUnsafeHttpMethods();

        options.AttemptTimeout.Timeout =
            TimeSpan.FromSeconds(10);

        options.TotalRequestTimeout.Timeout =
            TimeSpan.FromSeconds(30);
    });


builder.Services.AddScoped<UserSession>();

builder.Services.AddScoped<CustomAuthStateProvider>();

builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<CustomAuthStateProvider>());

builder.Services.AddAuthorizationCore();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Error",
        createScopeForErrors: true);

    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();