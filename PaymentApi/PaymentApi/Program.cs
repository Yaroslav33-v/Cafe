using Npgsql;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDbConnection>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new NpgsqlConnection(connectionString);
});

var app = builder.Build();

app.MapGet("/balance/{cardLastFour}", (string cardLastFour) =>
{
    try
    {

    }
    catch
    { 
    
    }
});

app.Run();
