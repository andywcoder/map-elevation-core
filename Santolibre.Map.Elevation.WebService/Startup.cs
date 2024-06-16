using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Santolibre.Map.Elevation.Lib;
using System.Text.Json.Serialization;

namespace Santolibre.Map.Elevation.WebService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IMetadataService, MetadataService>();
            services.AddScoped<IElevationService, ElevationService>();
            services.AddScoped<IDistanceService, DistanceService>();
            services.AddSingleton(AutoMapper.CreateMapper());
            
            services.AddMemoryCache(options =>
            {
                options.SizeLimit = 1024L * 1024L * 256L;
            });

            services.AddCors();
            services
                .AddControllers(options =>
                {
                    options.Filters.Add(typeof(ValidateModelStateAttribute));
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(
                options => options
                .WithOrigins("http://local.map.santolibre.net", "https://map.santolibre.net")
                .AllowAnyMethod()
                .AllowAnyHeader()
            );

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
