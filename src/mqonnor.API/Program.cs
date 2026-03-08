using mqonnor.API.DI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddCommandHandlers();
builder.Services.AddMediator();
builder.Services.AddMappers();
builder.Services.AddWorkers();
builder.Services.AddPersistence(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
