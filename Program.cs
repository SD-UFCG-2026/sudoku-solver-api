using SudokuSolverAPI.BackgroundServices;
using SudokuSolverAPI.Channels;
using SudokuSolverAPI.Interfaces;
using SudokuSolverAPI.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
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
