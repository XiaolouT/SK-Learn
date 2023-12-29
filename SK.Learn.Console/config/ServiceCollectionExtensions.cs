using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Sqlite;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Core;
using SK.Learn.ConsoleChat.plugins;

namespace SK.Learn.ConsoleChat.config
{
    internal static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a chat completion service to the list. It can be either an OpenAI or Azure OpenAI backend service.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        internal static IServiceCollection AddChatCompletionService(this IServiceCollection serviceCollection, KernelSettings kernelSettings, ISemanticTextMemory semanticTextMemory)
        {
            var kernel = serviceCollection.AddKernel();
            switch (kernelSettings.ServiceType.ToUpperInvariant())
            {
                case ServiceTypes.AzureOpenAI:
                    kernel.Services.AddAzureOpenAIChatCompletion(kernelSettings.ChatDeploymentId, endpoint: kernelSettings.Endpoint, apiKey: kernelSettings.ApiKey);
                    kernel.Services.AddAzureOpenAITextEmbeddingGeneration(kernelSettings.EmbeddingDeploymentId, endpoint: kernelSettings.Endpoint, apiKey: kernelSettings.ApiKey);
                    break;

                case ServiceTypes.OpenAI:
                    kernel.Services.AddOpenAIChatCompletion(modelId: kernelSettings.ChatModelId, apiKey: kernelSettings.ApiKey);
                    kernel.Services.AddOpenAITextEmbeddingGeneration(modelId: kernelSettings.EmbeddingModelId, apiKey: kernelSettings.ApiKey);
                    break;

                default:
                    throw new ArgumentException($"Invalid service type value: {kernelSettings.ServiceType}");
            }

            kernel.Plugins.AddFromType<LightPlugin>();
            kernel.Plugins.AddFromType<TimePlugin>();
            WeatherPlugin weatherPlugin = new(new HttpClient(), kernelSettings);
            kernel.Plugins.AddFromObject(weatherPlugin);

            CNCityPlugin cityPlugin = new(semanticTextMemory);
            kernel.Plugins.AddFromObject(cityPlugin);

            return serviceCollection;
        }

        internal static ISemanticTextMemory AddSemanticTextMemory(this IServiceCollection serviceCollection, KernelSettings kernelSettings)
        {
            var memoryBuilder = new MemoryBuilder();
            var semanticTextMemory = memoryBuilder.WithMemoryStore(SqliteMemoryStore.ConnectAsync("memories.sqlite").GetAwaiter().GetResult())
                .WithAzureOpenAITextEmbeddingGeneration(kernelSettings.EmbeddingDeploymentId, endpoint: kernelSettings.Endpoint, apiKey: kernelSettings.ApiKey)
                .Build();
            serviceCollection.AddSingleton(semanticTextMemory);

            return semanticTextMemory;
        }
    }

}
