using System.Threading.RateLimiting;
using MongoDB.Driver;
using SudokuSolverAPI.BackgroundServices;
using SudokuSolverAPI.Channels;
using SudokuSolverAPI.Controllers;
using SudokuSolverAPI.Interfaces;
using SudokuSolverAPI.Services;
using SudokuSolverAPI.Utils;

var builder = WebApplication.CreateBuilder(args);

if (!int.TryParse(builder.Configuration["PERMIT_LIMIT"], out var permitLimit))
{
    permitLimit = 10;
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .WithMethods("GET", "POST");
        });
});

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var clientIp = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                       ?? context.Connection.RemoteIpAddress?.ToString()
                       ?? new Random().Next().ToString();

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: clientIp,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync("{\"error\": \"Too many requests. Please try again in 1 minute.\"}", cancellationToken);
    };
});

MongoConfig.RegisterCustomSerializers();

builder.Services.AddControllers()
    .AddApplicationPart(typeof(RunController).Assembly)
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        opt.JsonSerializerOptions.Converters.Add(new MultidimensionalArrayConverter());
    });
builder.Services.AddOpenApi();

string mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb") ?? "mongodb://mongodb:27017";
string mongoDatabaseName = builder.Configuration["MongoDatabaseName"] ?? "SudokuSolverDB";

builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDatabaseName);
});

builder.Services.AddSingleton<ValidationChannel>();
builder.Services.AddSingleton<ProcessingChannel>();

builder.Services.AddSingleton<IBoardValidatorService, BoardValidatorService>();
builder.Services.AddSingleton<IBoardProcesserService, BoardProcesserService>();
builder.Services.AddSingleton<IBoardPersisterService, BoardPersisterService>();

builder.Services.AddHostedService<ValidationBackgroundService>();
builder.Services.AddHostedService<ProcessingBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
