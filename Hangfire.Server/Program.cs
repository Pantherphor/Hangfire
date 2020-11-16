using Fclp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hangfire.Server
{
    internal class ApplicationArguments
    {
        public string Storage { get; set; }
        public string Connection { get; set; }
        public int Workers { get; internal set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var p = new FluentCommandLineParser<ApplicationArguments>();

            p.Setup(args => args.Workers)
            .As('w', "workers")
            .SetDefault(Environment.ProcessorCount * 5);

            p.Setup(args => args.Connection)
           .As('c', "connection")
           .SetDefault(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

            var result = p.Parse(args);

            if (result.UnMatchedOptions.Any())
            {
                Console.WriteLine("unmatched options count: " + result.UnMatchedOptions.Count());
            }

            var argsObj = p.Object;



            GlobalConfiguration.Configuration.UseSqlServerStorage(argsObj.Connection);
            //GlobalConfiguration.Configuration.UsePostgreSqlStorage(argsObj.Connection);

            var hostBuilder = new HostBuilder()
                // Add configuration, logging, ...
                .ConfigureServices((hostContext, services) =>
                {
                    //services.Configure(()=> )
                    // Add your services with depedency injection.
                });

            using var server = new BackgroundJobServer(new BackgroundJobServerOptions()
            {
                WorkerCount = argsObj.Workers
            });
            await hostBuilder.RunConsoleAsync();
        }

    }
}
