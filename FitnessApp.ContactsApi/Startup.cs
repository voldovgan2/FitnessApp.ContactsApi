using System;
using System.IO;
using System.Reflection;
using FitnessApp.Abstractions.Db.Configuration;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Data.Entities;
using FitnessApp.ContactsApi.Models.Input;
using FitnessApp.ContactsApi.Models.Output;
using FitnessApp.ContactsApi.Services.Contacts;
using FitnessApp.ContactsApi.Services.MessageBus;
using FitnessApp.NatsServiceBus;
using FitnessApp.Serializer.JsonMapper;
using FitnessApp.Serializer.JsonSerializer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization;
using Swashbuckle.AspNetCore.Filters;

namespace FitnessApp.ContactsApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
            });

            services.AddTransient<IJsonSerializer, JsonSerializer>();

            services.AddTransient<IJsonMapper, JsonMapper>();

            services.Configure<MongoDbSettings>(Configuration.GetSection("MongoConnection"));

            services.Configure<NatsBusSettings>(Configuration.GetSection("Nats"));

            BsonClassMap.RegisterClassMap<UserContactsEntity>(cm =>
            {
                cm.MapMember(c => c.UserId);
                cm.MapMember(c => c.Collection);
            });
            BsonClassMap.RegisterClassMap<ContactItemEntity>(cm =>
            {
                cm.MapMember(c => c.Id);
            });

            services.AddTransient<IContactsRepository<UserContactsEntity, ContactItemEntity, UserContactsModel, ContactItemModel, CreateUserContactsModel, UpdateUserContactModel>, ContactsRepository<UserContactsEntity, ContactItemEntity, UserContactsModel, ContactItemModel, CreateUserContactsModel, UpdateUserContactModel>>();

            services.AddTransient<IContactsService<UserContactsEntity, ContactItemEntity, UserContactsModel, ContactItemModel, CreateUserContactsModel, UpdateUserContactModel>, ContactsService<UserContactsEntity, ContactItemEntity, UserContactsModel, ContactItemModel, CreateUserContactsModel, UpdateUserContactModel>>();
            
            services.AddSingleton<IServiceBus, ServiceBus>();

            services.AddHostedService<ContactsMessageBusService>();

            services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.Authority = Configuration["JWT:Issuer"];
                    cfg.Audience = Configuration["JWT:Audience"];
                });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "FitnessApp.ContactsApi",
                    Version = "v1",
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.XML";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.OperationFilter<SecurityRequirementsOperationFilter>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            loggerFactory.AddFile("Logs/ContactsApi-{Date}.txt");

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseMvc();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Swagger XML Api Demo v1");
            });
        }
    }
}