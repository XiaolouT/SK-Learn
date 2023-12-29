
using Microsoft.Extensions.Hosting;
using SK.Learn.ConsoleChat.config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace SK.Learn.ConsoleChat
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;
            // Load the kernel settings
            var kernelSettings = KernelSettings.LoadSettings();

            // Create the host builder with logging configured from the kernel settings.
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel( LogLevel.Warning);
                });

            // Configure the services for the host
            builder.ConfigureServices((context, services) =>
            {
                services.AddSingleton(kernelSettings);

                var semanticTextMemory = services.AddSemanticTextMemory(kernelSettings);

                services.AddChatCompletionService(kernelSettings, semanticTextMemory);
                services.AddHostedService<ConsoleChat>();
            });

            // Build and run the host. This keeps the app running using the HostedService.
            var host = builder.Build();

            await host.RunAsync().ConfigureAwait(false);
        }
    }
}
