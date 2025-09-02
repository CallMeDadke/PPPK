using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using MedicalSystemApp.Data;
using MedicalSystemApp.Data.Repositories;
using MedicalSystemApp.Json;          
using System.Globalization;

namespace MedicalSystemApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

         
            builder.Services.AddControllers()
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.Converters.Add(new HrDateTimeConverter());
                    o.JsonSerializerOptions.Converters.Add(new HrNullableDateTimeConverter());
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<MedicalContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"))
                       .UseLazyLoadingProxies()
            );

            
            builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            
            builder.Services.AddCors(opt =>
            {
                opt.AddPolicy("Default", p => p
                    .WithOrigins("http://localhost:5173", "http://localhost:4200", "http://localhost:8080")
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            });

           
            var culture = new CultureInfo("hr-HR");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            
            var supportedCultures = new[] { new CultureInfo("hr-HR") };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("hr-HR"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();   
            app.UseCors("Default");
            app.MapControllers();
            app.Run();
        }
    }
}
