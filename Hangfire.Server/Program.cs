using Fclp;
using Hangfire.PostgreSql;
using Hangfire.SQLite;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace Hangfire.Server
{
    internal class ApplicationArguments
    {
        public string Storage { get; set; }
        public string ConnectionString { get; set; }
        public int Workers { get; internal set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var argsObj = GetServerConfig(args);

            if (argsObj.Storage == "sqlite")
            {
                GlobalConfiguration.Configuration.UseSQLiteStorage(argsObj.ConnectionString);
            }

            if (argsObj.Storage == "sqlserver")
            {
                GlobalConfiguration.Configuration.UseSqlServerStorage(argsObj.ConnectionString);
            }

            if (argsObj.Storage == "postgresql")
            {
                GlobalConfiguration.Configuration.UsePostgreSqlStorage(argsObj.ConnectionString);
            }

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

            Console.WriteLine("Hanfire server started");
            await hostBuilder.RunConsoleAsync();
        }

        private static ApplicationArguments GetServerConfig(string[] args)
        {
            var p = new FluentCommandLineParser<ApplicationArguments>();

            p.Setup(arg => arg.Storage)
             .As('s', "instorage")
             .SetDefault("sqlite");

            p.Setup(args => args.Workers)
            .As('w', "workers")
            .SetDefault(Environment.ProcessorCount * 5);

            p.Setup(args => args.ConnectionString)
           .As('c', "connectionstring")
           .SetDefault("Data Source=c:\\hangfire_sqlite.db;");

            var result = p.Parse(args);

            if (result.HasErrors)
            {
                Console.WriteLine("Error parsing Arguments: {0}", result.ErrorText);
            }

            var argsObj = p.Object;

            Console.WriteLine("Server Configuration:\r\n\r\n" +
                    ":================================================================:\r\n" +
                    "::Storage type: {0}\r\n" +
                    "::ConnectionString: {1}\r\n" +
                    "::Workers: {2}\r\n" +
                    ":================================================================:\r\n" +
                    "", argsObj.Storage, argsObj.ConnectionString, argsObj.Workers);
            return argsObj;
        }
    }
}
