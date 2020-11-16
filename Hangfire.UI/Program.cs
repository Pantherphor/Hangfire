using System;
using System.Linq;
using Fclp;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Hangfire.UI
{
    internal class ApplicationArguments
    {
        public string Storage { get; set; }
        public string TimeZone { get; set; }
        public string Dashboard { get; set; }
        public string Connection { get; set; }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var p = new FluentCommandLineParser<ApplicationArguments>();

                    p.Setup(arg => arg.Storage)
                     .As('s', "instorage")
                     .SetDefault("slqserver");

                    p.Setup(args => args.TimeZone)
                    .As('t', "timezone")
                    .SetDefault(TimeZoneInfo.Local.StandardName);

                    p.Setup(args => args.Dashboard)
                    .As('d', "dashboardname")
                    .SetDefault("hangfire");

                    p.Setup(args => args.Connection)
                   .As('c', "connection")
                   .SetDefault(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

                    var result = p.Parse(args);

                    if (result.UnMatchedOptions.Any())
                    {
                        Console.WriteLine("unmatched options count: " +result.UnMatchedOptions.Count());
                    }

                    var argsObj = p.Object;

                    webBuilder.ConfigureServices(services =>
                    {

                        if (argsObj.Storage is "inmemory")
                        {
                            services.AddHangfire(config =>
                                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                                .UseSimpleAssemblyNameTypeSerializer()
                                .UseDefaultTypeSerializer()
                                .UseInMemoryStorage());
                        }
                        else //if(argsObj.Storage is "sqlserverstorage")
                        {
                            services.AddHangfire(config =>
                               config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                               .UseSimpleAssemblyNameTypeSerializer()
                               .UseDefaultTypeSerializer()
                               .UseSqlServerStorage(argsObj.Connection));
                        }

                        services.AddControllers();

                        services.AddSwaggerGen(c =>
                        {
                            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hangfire UI", Version = "v1" });
                        });
                    });


                    webBuilder.Configure(app =>
                    {
                        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

                        if (env.IsDevelopment())
                        {
                            app.UseDeveloperExceptionPage();
                            app.UseSwagger();
                            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hangfire.UI v1"));
                        }

                        app.UseRouting();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                        });

                        app.UseHangfireDashboard("/" + argsObj.Dashboard, new DashboardOptions()
                        {
                            Authorization = new[] { new AllowAllConnectionsFilter() },
                            IgnoreAntiforgeryToken = true
                        });

                    });
                });
    }

    public class AllowAllConnectionsFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // Allow outside. You need an authentication scenario for this part.
            // DON'T GO PRODUCTION WITH THIS LINES.
            return true;
        }
    }
}
