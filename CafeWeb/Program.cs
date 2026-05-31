using CafeWeb.Hubs;
using CafeWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Session;
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
    options.Cookie.Name = ".Cafe4Tvorozhka.Session";
    options.Cookie.SameSite = SameSiteMode.Strict;
}); // Сессии

builder.Services.AddScoped<IDbConnection>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new NpgsqlConnection(connectionString);
}); // Соединение с БД

builder.Services.AddSignalR(options =>
{
    // Настройки SignalR
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 1024 * 10; // 10 KB
    // Каждые 15 секунд сервер шлет "ты жив?" клиенту
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);

    // Если 30 секунд нет ответа - соединение разрывается
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
}); // SignalR для real-time изменения статуса заказов

// Добавляем HttpContextAccessor для сессий
builder.Services.AddHttpContextAccessor();

// Регистрируем сервисы в DI
builder.Services.AddScoped<ICafeService, CafeService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPromocodeService, PromocodeService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Регистрируем фоновый процесс в DI
builder.Services.AddHostedService<OrderStatusUpdaterService>();

// Логгирование
builder.Logging.ClearProviders().AddNLog(builder.Configuration);

// Настройки аутентификации
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/user/signin";
        options.AccessDeniedPath = "/access-denied";
        options.Cookie.Name = "CafeCookie";
        options.Cookie.HttpOnly = true; 
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

// Политики авторизации
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

// Подключаем необходимые middleware
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

app.MapGet("/cafe/getcart", (ICartService cartService) =>
{
    var cart = cartService.GetCart();
    return Results.Ok(new
    {
        success = true,
        items = cart.Items.Select(i => new
        {
            id = i.Food.Id,
            quantity = i.Quantity,
            total = i.Total
        }),
        offerItems = cart.OfferItems.Select(o => new
        {
            id = o.Id,
            quantity = o.Quantity,
            total = o.Total
        }),
        totalAmount = cart.TotalAmount,
        totalItems = cart.TotalItems
    });
});

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
});// endpoint для получения данных о пользователе

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

app.MapGet("/cart-count", (ICartService cartService) =>
{
    var cart = cartService.GetCart();
    return Results.Ok(new { count = cart.TotalItems });
}); // endpoint для получения количества позиций в корзине

app.MapControllerRoute(
    name: default,
    pattern: "{controller=Cafe}/{action=Index}");

// Добавляем SignalR Hub 
app.MapHub<CafeHub>("/cafeHub");

app.Run();
