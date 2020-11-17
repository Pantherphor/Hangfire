using System;
using Fclp;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Hangfire.SQLite;
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
        public string ConnectionString { get; set; }
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
                    var argsObj = GetDashboardConfig(args);

                    webBuilder.ConfigureServices(services =>
                    {

                        if (argsObj.Storage is "sqlite")
                        {
                            services.AddHangfire(config =>
                                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                                .UseSimpleAssemblyNameTypeSerializer()
                                .UseDefaultTypeSerializer()
                                .UseSQLiteStorage(argsObj.ConnectionString));
                        }
                        if (argsObj.Storage is "sqlserver")
                        {
                            services.AddHangfire(config =>
                               config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                               .UseSimpleAssemblyNameTypeSerializer()
                               .UseDefaultTypeSerializer()
                               .UseSqlServerStorage(argsObj.ConnectionString));
                        }
                        if (argsObj.Storage is "postgresql")
                        {
                            services.AddHangfire(config =>
                               config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                               .UseSimpleAssemblyNameTypeSerializer()
                               .UseDefaultTypeSerializer()
                               .UsePostgreSqlStorage(argsObj.ConnectionString));
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

        private static ApplicationArguments GetDashboardConfig(string[] args)
        {
            var p = new FluentCommandLineParser<ApplicationArguments>();

            p.Setup(arg => arg.Storage)
             .As('s', "instorage")
             .SetDefault("sqlite");

            p.Setup(args => args.TimeZone)
            .As('t', "timezone")
            .SetDefault(TimeZoneInfo.Local.StandardName);

            p.Setup(args => args.Dashboard)
            .As('d', "dashboardname")
            .SetDefault("hangfire");

            p.Setup(args => args.ConnectionString)
           .As('c', "connectionstring")
           .SetDefault("Data Source=c:\\hangfire_sqlite.db;");

            var result = p.Parse(args);

            if (result.HasErrors)
            {
                Console.WriteLine("Error parsing Arguments: {0}", result.ErrorText);
            }

            var argsObj = p.Object;

            Console.WriteLine("Dashboard Configuration:\r\n\r\n" +
                    ":================================================================:\r\n" +
                    "::Storage type: {0}\r\n" +
                    "::ConnectionString: {1}\r\n" +
                    "::Dashboard Name: {2}\r\n" +
                    ":================================================================:\r\n" +
                    "", argsObj.Storage, argsObj.ConnectionString, argsObj.Dashboard);
            return argsObj;
        }
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
