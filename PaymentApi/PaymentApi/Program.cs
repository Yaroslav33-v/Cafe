using NLog.Extensions.Logging;
using Npgsql;
using PaymentApi.Dto;
using PaymentApi.Services;
using System.Data;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:7003");

builder.Services.AddScoped<IDbConnection>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new NpgsqlConnection(connectionString);
});
builder.Services.AddScoped<IPaymentService,PaymentService>();
builder.Logging.ClearProviders().AddNLog(builder.Configuration);

var app = builder.Build();

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
            created_at = DateTime.Now
        });
    }
    catch (Exception ex) when (ex.Message.Contains("Недостаточно средств"))
    {
        logger.LogInformation("На карте {LastFour} недостаточно средств", paymentDto.LastFour);
        return Results.BadRequest(new
        {
            message = ex.Message,
            created_at = DateTime.Now
        });
    }
    catch (Exception ex) when (ex.Message.Contains("Карта не найдена"))
    {
        logger.LogInformation("Карта {LastFour} не найдена", paymentDto.LastFour);
        return Results.BadRequest(new
        {
            message = ex.Message,
            created_at = DateTime.Now
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
});

app.Run();
