﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace SK.Learn.ConsoleChat.config
{
    public class KernelSettings
    {
        public const string DefaultConfigFile = "config/appsettings.json";

        [JsonPropertyName("serviceType")]
        public string ServiceType { get; set; } = string.Empty;

        [JsonPropertyName("chatDeploymentId")]
        public string ChatDeploymentId { get; set; } = string.Empty;

        [JsonPropertyName("chatModelId")]
        public string ChatModelId { get; set; } = string.Empty;

        [JsonPropertyName("embeddingDeploymentId")]
        public string EmbeddingDeploymentId { get; set; } = string.Empty;

        [JsonPropertyName("embeddingModelId")]
        public string EmbeddingModelId { get; set; } = string.Empty;

        [JsonPropertyName("endpoint")]
        public string Endpoint { get; set; } = string.Empty;

        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; } = string.Empty;

        [JsonPropertyName("weatherApiKey")]
        public string WeatherApiKey { get; set; } = string.Empty;

        [JsonPropertyName("orgId")]
        public string OrgId { get; set; } = string.Empty;

        [JsonPropertyName("logLevel")]
        public LogLevel? LogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Warning;

        [JsonPropertyName("systemPrompt")]
        public string SystemPrompt { get; set; } = "You are a friendly, intelligent, and curious assistant who is good at conversation, always response with Chinese.";

        /// <summary>
        /// Load the kernel settings from settings.json if the file exists and if not attempt to use user secrets.
        /// </summary>
        internal static KernelSettings LoadSettings()
        {
            try
            {
                if (File.Exists(DefaultConfigFile))
                {
                    return FromFile(DefaultConfigFile);
                }

                Console.WriteLine($"Semantic kernel settings '{DefaultConfigFile}' not found, attempting to load configuration from user secrets.");

                return FromUserSecrets();
            }
            catch (InvalidDataException ide)
            {
                Console.Error.WriteLine(
                    "Unable to load semantic kernel settings, please provide configuration settings using instructions in the README.\n" +
                    "Please refer to: https://github.com/microsoft/semantic-kernel-starters/blob/main/sk-csharp-hello-world/README.md#configuring-the-starter"
                );
                throw new InvalidOperationException(ide.Message);
            }
        }

        /// <summary>
        /// Load the kernel settings from the specified configuration file if it exists.
        /// </summary>
        internal static KernelSettings FromFile(string configFile = DefaultConfigFile)
        {
            if (!File.Exists(configFile))
            {
                throw new FileNotFoundException($"Configuration not found: {configFile}");
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile(configFile, optional: true, reloadOnChange: true)
                .Build();

            return configuration.Get<KernelSettings>()
                   ?? throw new InvalidDataException($"Invalid semantic kernel settings in '{configFile}', please provide configuration settings using instructions in the README.");
        }

        /// <summary>
        /// Load the kernel settings from user secrets.
        /// </summary>
        internal static KernelSettings FromUserSecrets()
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<KernelSettings>()
                .Build();

            return configuration.Get<KernelSettings>()
                   ?? throw new InvalidDataException("Invalid semantic kernel settings in user secrets, please provide configuration settings using instructions in the README.");
        }
    }
}
