using Employee.Data.Data;
using Employee.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using Employee.API.Validators;
using Employee.BusinessLogic;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


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


var app = builder.Build();


app.UseSerilogRequestLogging();


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


app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();


//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Employee}/{action=Index}/{id?}");

app.Run();
