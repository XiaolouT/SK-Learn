using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Hosting;
using SK.Learn.ConsoleChat.config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SK.Learn.ConsoleChat.plugins;

namespace SK.Learn.ConsoleChat
{
    internal class Program
    {
        static async Task Main(string[] args)
        {


            // Load the kernel settings
            var kernelSettings = KernelSettings.LoadSettings();

            // Create the host builder with logging configured from the kernel settings.
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(kernelSettings.LogLevel ?? LogLevel.Warning);
                });

            // Configure the services for the host
            builder.ConfigureServices((context, services) =>
            {

                // Add kernel settings to the host builder
                services
                    .AddSingleton<KernelSettings>(kernelSettings)
                    .AddTransient<Kernel>(serviceProvider => {
                        KernelBuilder builder = new();
                        builder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Information));
                        builder.Services.AddChatCompletionService(kernelSettings);
                        builder.Plugins.AddFromType<LightPlugin>();

                        return builder.Build();
                    })
                    .AddHostedService<ConsoleChat>();
            });

            // Build and run the host. This keeps the app running using the HostedService.
            var host = builder.Build();
            await host.RunAsync();
        }
    }
}