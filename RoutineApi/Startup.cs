using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using RoutineApi.Data;
using RoutineApi.Services;
using System;
using System.Linq;

namespace RoutineApi
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
            services.AddControllers(setup =>
            {
                setup.ReturnHttpNotAcceptable = true;
                //setUp.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                //setUp.OutputFormatters.Insert(0,new XmlDataContractSerializerOutputFormatter());
            })
                .AddNewtonsoftJson(setup=> {
                setup.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();
            })
                .AddXmlDataContractSerializerFormatters()
                .ConfigureApiBehaviorOptions(setup =>
                {
                    setup.InvalidModelStateResponseFactory = context =>
                    {
                        var problemDetails = new ValidationProblemDetails(context.ModelState)
                        {
                            Type = "http://www.baidu.com",
                            Title = "�д��󣡣�",
                            Status = StatusCodes.Status422UnprocessableEntity,
                            Detail = "�뿴��ϸ��Ϣ��",
                            Instance = context.HttpContext.Request.Path,
                        };
                        problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);
                        return new UnprocessableEntityObjectResult(problemDetails)
                        {
                            ContentTypes = { "application/problem+json" }
                        };
                    };
                });

            services.Configure<MvcOptions>(config =>
            {
                var newtonSoftJsonOutputFormatter =
                config.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();
                if (newtonSoftJsonOutputFormatter!=null)
                {
                    newtonSoftJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.company.hateoas+json");
                    newtonSoftJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.company.full.hateoas+json");
                    newtonSoftJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.company.full+json");
                }

            });
            services.AddScoped<ICompanyRepository,CompanyRepository>();
            services.AddDbContext<RoutineDbContext>(option=>
            {
                option.UseSqlite("Data Source=routine2.db");
            });
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddTransient<IPropertyMappingService,PropertyMappingService>();
            services.AddTransient<IPropertyCheckerService,PropertyCheckerService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(apoBuilder =>
                {
                    apoBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("Unexpected Error!");
                    });
                }
                
                    );
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
