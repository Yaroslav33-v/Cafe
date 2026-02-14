using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NLog.Extensions.Logging;
using Npgsql;
using PaymentApi;
using PaymentApi.Dto;
using PaymentApi.Services;
using System.Data;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("https://localhost:7003");

var authOptions = new AuthOptions(builder.Configuration);
builder.Services.AddSingleton(authOptions);

builder.Services.AddScoped<IDbConnection>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new NpgsqlConnection(connectionString);
});
builder.Services.AddScoped<IPaymentService,PaymentService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = authOptions.Audience,
            ValidateLifetime = true,
            IssuerSigningKey = authOptions.GetSymmetricSecurityKey(),
            ValidateIssuerSigningKey = true
        };
    });

builder.Logging.ClearProviders().AddNLog(builder.Configuration);

var app = builder.Build();

app.MapGet("/login/{cardToken}", async (string cardToken, IAuthService authService, ILogger<Program> logger) =>
{
    logger.LogInformation("Получена заявка на авторизацию от {CardToken}", cardToken);
    try
    {
        string jwt = await authService.Login(cardToken);

        logger.LogInformation("Доступ для {CardToken} разрешен", cardToken);

        return Results.Ok(new 
        {
            token = jwt,
            createdAt = DateTime.Now,
            expiresAt = DateTime.Now.AddMinutes(2)
        });
    }
    catch
    {
        logger.LogInformation("Доступ для {CardToken} запрещен", cardToken);
        return Results.Unauthorized();
    }
});

app.MapPost("/payment_attempt", async (PaymentDto paymentDto, IPaymentService paymentService, ILogger<Program> logger) =>
{
    logger.LogInformation("Получена заявка на оплату в размере {Total} с карты {LastFour}", paymentDto.Total, paymentDto.LastFour);
    try
    {
        await paymentService.TryToPay(paymentDto);

        logger.LogInformation("Оплата картой {LastFour} проведена успешно", paymentDto.LastFour);

        return Results.Ok(new
        {
            message = "Оплата проведена успешно!",
            createdAt = DateTime.Now
        });
    }
    catch (Exception ex) when (ex.Message.Contains("Недостаточно средств"))
    {
        logger.LogInformation("На карте {LastFour} недостаточно средств", paymentDto.LastFour);
        return Results.BadRequest(new
        {
            message = ex.Message,
            createdAt = DateTime.Now
        });
    }
    catch (Exception ex) when (ex.Message.Contains("Карта не найдена"))
    {
        logger.LogInformation("Карта {LastFour} не найдена", paymentDto.LastFour);
        return Results.BadRequest(new
        {
            message = ex.Message,
            createdAt = DateTime.Now
        });
    }
    catch (Exception ex) when (ex.Message.Contains("Карта просрочена"))
    {
        logger.LogInformation("Карта {LastFour} просрочена", paymentDto.LastFour);
        return Results.BadRequest(new
        {
            message = ex.Message,
            createdAt = DateTime.Now
        });
    }
    catch (PostgresException ex)
    {
        logger.LogError(ex.MessageText);
        return Results.StatusCode(500);
    }
    catch (Exception ex)
    {
        logger.LogError(ex.Message);
        return Results.StatusCode(500);
    }
}).RequireAuthorization();

app.Run();
