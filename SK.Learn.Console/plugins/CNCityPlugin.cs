using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using System.ComponentModel;

namespace SK.Learn.ConsoleChat.plugins
{
    public sealed class CNCityPlugin
    {
        const string CitiesCollection = "cities";
        private readonly ISemanticTextMemory _semanticTextMemory;
        public CNCityPlugin(ISemanticTextMemory semanticTextMemory)
        {
            this._semanticTextMemory = semanticTextMemory;
            // LoadCityMappings();
        }

        [KernelFunction("GetChinaCityCodeByName")]
        [Description("Get 6 digital city code for given City Name")]
        public async Task<string> GetCityCodeAsync(string cityName)
        {
            var results =  this._semanticTextMemory.SearchAsync(CitiesCollection, cityName, 1);
            await foreach (var result in results)
            {
                return result.Metadata.Id;
            }
            return string.Empty;
        }

        private void LoadCityMappings()
        {
            var mappingFile = "plugins/cn_adcode_citycode.csv";
            var lines = File.ReadAllLines(mappingFile);

            string currentCityCode = null;
            string currentCityName = null;
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length == 3)
                {
                    string name = parts[0];
                    string acCode = parts[1];
                    string cityCode = parts[2];

                    if(currentCityCode != cityCode)
                    {
                        currentCityCode = cityCode;
                        currentCityName = name;
                    }
                    else
                    {
                        name = currentCityName + name;
                    }
                    this._semanticTextMemory.SaveInformationAsync(CitiesCollection, name, acCode).GetAwaiter().GetResult();
                }
            }
        }
    }
}
