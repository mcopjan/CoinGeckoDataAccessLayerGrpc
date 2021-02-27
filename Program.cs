using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GreeterGrpc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                    webBuilder.ConfigureKestrel((context, options) =>
                    {
                        // Support --port and --use_tls cmdline arguments normally supported
                        // by gRPC interop servers.
                        /*
                        var port = context.Configuration.GetValue<int>("port", 50052);
                        var useTls = context.Configuration.GetValue<bool>("use_tls", false);

                        options.Limits.MinRequestBodyDataRate = null;
                        options.ListenAnyIP(port, listenOptions =>
                        {
                            Console.WriteLine($"Enabling connection encryption: {useTls}");

                            if (useTls)
                            {
                                //var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                                var certPath = "/Users/martinco/server.crt";

                                listenOptions.UseHttps(certPath, "0646748094aA");
                            }
                            listenOptions.Protocols = HttpProtocols.Http2;
                        });*/
                        // Setup a HTTP/2 endpoint without TLS.
                        options.ListenLocalhost(5000, o => o.Protocols =
                            HttpProtocols.Http2);

                    });
                });
    }
}
