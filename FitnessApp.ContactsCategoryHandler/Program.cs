using FitnessApp.Common.Configuration;
using FitnessApp.Contacts.Common.Interfaces;
using FitnessApp.Contacts.Common.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureMongo(builder.Configuration);
builder.Services.AddTransient<IGlobalContainer, GlobalContainer>();
builder.Services.AddTransient<IFollowersContainer, FollowersContainer>();
builder.Services.AddTransient<IUserDbContext, UserDbContext>();
builder.Services.AddTransient<IFollowerDbContext, FollowerDbContext>();
builder.Services.AddTransient<IFollowingDbContext, FollowingDbContext>();
builder.Services.AddTransient<IFirstCharSearchUserDbContext, FirstCharSearchUserDbContext>();
builder.Services.AddTransient<IFirstCharDbContext, FirstCharDbContext>();
builder.Services.AddScoped<IDateTimeService, DateTimeService>();

var app = builder.Build();

app.Run();
