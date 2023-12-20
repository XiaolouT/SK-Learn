using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace SK.Learn.ConsoleChat.plugins
{
    internal class LightPlugin
    {
        public bool IsOn { get; set; } = false;

        [KernelFunction("GetState")]
        [Description("Gets the state of the light.")]
        public string GetState() => this.IsOn ? "on" : "off";

        [KernelFunction("ChangeState")]
        [Description("Changes the state of the light.'")]
        public string ChangeState(bool newState)
        {
            this.IsOn = newState;
            var state = this.GetState();

            // Print the state to the console
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine($"[Light is now {state}]");
            Console.ResetColor();

            return state;
        }
    }
}
