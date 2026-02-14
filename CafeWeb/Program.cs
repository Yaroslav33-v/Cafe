using CafeWeb.Services;
using NLog.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Logging.ClearProviders().AddNLog(builder.Configuration);

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: default,
    pattern: "{controller=Payment}/{action=Index}");
app.Run();
