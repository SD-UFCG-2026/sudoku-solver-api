using MongoDB.Driver;
using SudokuSolverAPI.BackgroundServices;
using SudokuSolverAPI.Channels;
using SudokuSolverAPI.Controllers;
using SudokuSolverAPI.Interfaces;
using SudokuSolverAPI.Services;
using SudokuSolverAPI.Utils;

var builder = WebApplication.CreateBuilder(args);


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
