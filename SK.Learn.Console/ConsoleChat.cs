
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SK.Learn.ConsoleChat.config;

namespace SK.Learn.ConsoleChat
{
    internal class ConsoleChat : IHostedService
    {
        private readonly Kernel _kernel;
        private readonly IHostApplicationLifetime _lifeTime;
        private readonly KernelSettings _kernelSettings;
        public ConsoleChat(Kernel kernel, IHostApplicationLifetime lifeTime, KernelSettings kernelSettings)
        {
            this._kernel = kernel;
            this._lifeTime = lifeTime;
            this._kernelSettings = kernelSettings;
        }

        /// <summary>
        /// Start the service.
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() => this.ExecuteAsync(cancellationToken), cancellationToken);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stop a running service.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// The main execution loop. It will use any of the available plugins to perform actions
        /// </summary>
        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            ChatHistory chatMessages = new ChatHistory(this._kernelSettings.SystemPrompt);
            IChatCompletionService chatCompletionService = this._kernel.GetRequiredService<IChatCompletionService>();

            // Loop till we are cancelled
            while (!cancellationToken.IsCancellationRequested)
            {
                // Get user input
                System.Console.Write("User > ");
                chatMessages.AddUserMessage(Console.ReadLine()!);

                // Get the chat completions
                OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
                {
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                };
                /* get response */
                //var contents = await chatCompletionService.GetChatMessageContentsAsync(chatMessages,
                //    executionSettings: openAIPromptExecutionSettings,
                //    kernel: this._kernel,
                //    cancellationToken: cancellationToken).ConfigureAwait(false);
                //if (contents != null)
                //{
                //    foreach (var content in contents)
                //    {
                //        if (content.Role == AuthorRole.Assistant)
                //        {
                //            System.Console.Write("Assistant > ");
                //            System.Console.Write(content.Content);
                //            System.Console.WriteLine();

                //            chatMessages.AddAssistantMessage(content.Content);
                //        }
                //    }
                //}

                /* get streaming response */
                IAsyncEnumerable<StreamingChatMessageContent> result =
                    chatCompletionService.GetStreamingChatMessageContentsAsync(
                        chatMessages,
                        executionSettings: openAIPromptExecutionSettings,
                        kernel: this._kernel,
                        cancellationToken: cancellationToken);

                //Print the chat completions
                ChatMessageContent? chatMessageContent = null;
                await foreach (var content in result)
                {
                    if (content.Role == AuthorRole.Assistant && chatMessageContent == null)
                    {
                        System.Console.Write("Assistant > ");
                        chatMessageContent = new(
                            AuthorRole.Assistant,
                            content.ModelId,
                            content.Content,
                            content.InnerContent,
                            content.Encoding,
                            content.Metadata
                        );
                    }
                    System.Console.Write(content.Content);
                    if (chatMessageContent != null)
                    {
                        chatMessageContent.Content += content.Content;
                    }


                }
                System.Console.WriteLine();
                if (chatMessageContent != null)
                {
                    chatMessages.AddAssistantMessage(chatMessageContent.Content);
                }
            }
        }
    }
}
