using Employee.API.Healthcheck;
using Employee.API.Middleware;
using Employee.API.Validators;
using Employee.BusinessLogic;
using Employee.Data.Data;
using Employee.Data.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


//Healthcheck for MYSQL 
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "DefaultConnection is not configured.");

builder.Services
    .AddHealthChecks()
    .AddCheck(
        "mysql",
        new MySqlHealthCheck(connectionString));



Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog(
    (context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());


builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]!)),


                RoleClaimType = ClaimTypes.Role,

                NameClaimType = ClaimTypes.NameIdentifier
            };
    });



builder.Services.AddAuthorization();



//builder.Services.AddControllers();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });



//// sqlite

//string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//builder.Services.AddDbContext<EmployeeDbContext>(options =>
//    options.UseSqlite(connectionString));
//builder.Services.AddHealthChecks()
//    .AddCheck("sqlite_db_check", new SqliteHealthCheck(connectionString),
//              failureStatus: HealthStatus.Unhealthy,
//              tags: new[] { "ready" });


//MYSQL 

builder.Services.AddDbContext<EmployeeDbContext>(
    options =>
        options.UseMySql(
            builder.Configuration
                .GetConnectionString("DefaultConnection"),

            ServerVersion.AutoDetect(
                builder.Configuration
                    .GetConnectionString("DefaultConnection")
            )
        ));



builder.Services.AddScoped<
    IEmployeeRepository,
    EmployeeRepository>();

builder.Services.AddScoped<
    IEmployeeService,
    EmployeeService>();



builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>>();



builder.Services.AddValidatorsFromAssemblyContaining<
    EmployeeValidator>();



builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT",

            Name = "Authorization",

            In = ParameterLocation.Header,

            Description =
                "Enter JWT token"
        });

    options.AddSecurityRequirement(
        document =>
            new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecuritySchemeReference(
                        "Bearer",
                        document)
                ] = []
            });
});

 
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


var app = builder.Build();


app.UseSerilogRequestLogging();
//app.MapHealthChecks("/healthz");
app.MapHealthChecks("/health");


////sqlite

//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();
//    dbContext.Database.EnsureCreated();
//}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();


//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Employee}/{action=Index}/{id?}");

app.Run();
