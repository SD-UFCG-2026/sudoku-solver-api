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
