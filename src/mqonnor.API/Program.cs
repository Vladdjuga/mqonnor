using mqonnor.API.DI;
using mqonnor.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddSignalRHub();
builder.Services.AddCommandHandlers();
builder.Services.AddMediator();
builder.Services.AddMappers();
builder.Services.AddWorkers(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddNotifications();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapHub<EventHub>("/hubs/events");

app.Run();
