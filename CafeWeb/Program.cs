using CafeWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using NLog.Extensions.Logging;
using Npgsql;
using System.Data;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".BankSystem.Session";
    options.Cookie.SameSite = SameSiteMode.Strict;
}); // Сессии

builder.Services.AddScoped<IDbConnection>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new NpgsqlConnection(connectionString);
});

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPromocodeService, PromocodeService>();

builder.Logging.ClearProviders().AddNLog(builder.Configuration);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/user/signin";
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
    app.UseExceptionHandler("/error");
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
app.UseSession();
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

app.MapGet("/error", (HttpContext context) =>
{
    var filePath = Path.Combine(app.Environment.WebRootPath, "HTML", "error.html");
    return Results.File(filePath, "text/html");
});// Путь к error.html

app.MapGet("/check-login/{login}", async (IUserService userService, string login) =>
{
    try
    {
        bool isAvailable = await userService.IsNewLogin(login);

        return Results.Ok(new
        {
            available = isAvailable,
            message = isAvailable ? "Логин свободен" : "Пользователь с таким логином уже существует"
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
    return Results.Redirect("/user/signin");
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

app.MapGet("/is-valid-promo/{promo}",async (string promo, IPromocodeService promocodeService) => {
    try
    {
        var promocode = await promocodeService.GetPromocodeInfo(promo);

        if (promocode is null)
            return Results.Ok(new
            {
                available = false,
                message = "Промокод не существует",
            });

        return Results.Ok(new
        {
            available = true,
            message = "Промокод существует",
            fromSum = promocode.FromSum,
            discount = promocode.Discount
        });
    }
    catch
    {
        return Results.StatusCode(500);
    }
}); // endpoint для проверки существования промокода

app.MapControllerRoute(
    name: default,
    pattern: "{controller=Cafe}/{action=Index}");
app.Run();
