using CafeWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using NLog.Extensions.Logging;
using Npgsql;
using System.Data;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IDbConnection>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new NpgsqlConnection(connectionString);
});

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Logging.ClearProviders().AddNLog(builder.Configuration);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User/SignIn";
        options.AccessDeniedPath = "/access-denied";
        options.Cookie.Name = "CafeCookie";
        options.Cookie.HttpOnly = true; 
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles(new StaticFileOptions
{ 
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "IMG")),
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append(
            "Cache-Control", "public, max-age=3600");
    }
}); // Кэширование изображений

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/access-denied", (HttpContext context) =>
{
    var filePath = Path.Combine(app.Environment.WebRootPath, "HTML", "access-denied.html");
    return Results.File(filePath, "text/html");
});// Путь к access-denied.html

app.MapGet("/about", (HttpContext context) =>
{
    var filePath = Path.Combine(app.Environment.WebRootPath, "HTML", "about.html");
    return Results.File(filePath, "text/html");
});// Путь к about.html

app.MapGet("/faq", (HttpContext context) =>
{
    var filePath = Path.Combine(app.Environment.WebRootPath, "HTML", "faq.html");
    return Results.File(filePath, "text/html");
});// Путь к faq.html

app.MapGet("/check-login/{login}", async (IUserService userService, string login) =>
{
    try
    {
        if(await userService.IsNewLogin(login))
            return Results.Ok();

        return Results.BadRequest(new
        {
            message = "Пользователь с таким логином уже существует"
        });
    }
    catch
    {
        return Results.StatusCode(500);
    }
}); // endpoint для проверки существования логина

app.MapGet("/signout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/User/SignIn");
}); // endpoint для выхода из аккаунта

app.MapGet("/me", (HttpContext context) =>
{
    var name = context.User.FindFirst(ClaimTypes.Name)?.Value;
    var role = context.User.FindFirst(ClaimTypes.Role)?.Value;

    return Results.Ok(new
    {
        name,
        role
    });
}).RequireAuthorization(); // endpoint для получения данных о пользователе

app.MapControllerRoute(
    name: default,
    pattern: "{controller=User}/{action=SignIn}");
app.Run();
