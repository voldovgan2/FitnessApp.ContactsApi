using FitnessApp.Common.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureMongo(builder.Configuration);

var app = builder.Build();

app.Run();
