using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Santolibre.Map.Elevation.Lib.Services;

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
            services.AddSingleton<ICacheService, CacheService>();
            services.AddSingleton(AutoMapper.CreateMapper());

            services.AddCors();
            services
                .AddMvc(options =>
                {
                    options.Filters.Add(typeof(ValidateModelStateAttribute));
                })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ssZ";
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
            app.UseMvc();
        }
    }
}
