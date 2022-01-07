using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using WeatherForecast.Services;

namespace WeatherForecast
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

            services.AddControllers();
            // Registering DbContext using Sql Server
            services.AddDbContext<IDemoDBContext, DemoDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                        x =>
                        {
                            x.MigrationsHistoryTable("__Net6DemoMigrationHistory");
                            x.EnableRetryOnFailure(maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorNumbersToAdd: null);
                        })
            );
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WeatherForecast", Version = "v1" });
            });
            services.AddTransient<IWeatherForecastService, WeatherForecastService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherForecast v1"));
                
            }
            else
            {
                app.UseExceptionHandler(a => a.Run(async context =>
                {
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var exception = exceptionHandlerPathFeature.Error;
                    ValidationProblemDetails problemDetails;
                    if (exception is HttpStatusException httpStatusException)
                    {
                        context.Response.StatusCode = (int)httpStatusException.Status;
                        problemDetails = new ValidationProblemDetails()
                        {
                            Title = "Invalid Request",
                            Status = (int)httpStatusException.Status,
                            Detail = httpStatusException.Message
                        };
                    }
                    else
                    {
                        problemDetails = new ValidationProblemDetails()
                        {
                            Title = "An internal error occured while processing the request.",
                            Status = context.Response.StatusCode,
                            Detail = exception.InnerException?.ToString() ?? exception.Message
                        };
                    }

                    var result = JsonConvert.SerializeObject(problemDetails);
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(result);
                }));
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
