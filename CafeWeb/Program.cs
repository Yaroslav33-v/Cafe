using CafeWeb.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using NLog.Extensions.Logging;
using Npgsql;
using System.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IDbConnection>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new NpgsqlConnection(connectionString);
});

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAdminService, AdminService>();

builder.Logging.ClearProviders().AddNLog(builder.Configuration);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User/Login";
        options.AccessDeniedPath = "/User/AccessDenied";
        options.Cookie.Name = "CafeCookie";
        options.Cookie.HttpOnly = true; 
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
});

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();

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

app.MapPost("/login", (string? returnUrl, HttpContext context) =>
{

});

app.MapControllerRoute(
    name: default,
    pattern: "{controller=Admin}/{action=NewOffer}");
app.Run();
