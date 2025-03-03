using System;

namespace BlazorWasmPortfolioGhAction.Contexts
{
    public class TemperatureStore
    {

        public event EventHandler OnTemperatureUpdated;

        public string Temperature { get; set; }

        public TemperatureStore()
        {
            Temperature = "30 degree";
        }

        public void UpdateTemperature()
        {
            Random r1 = new Random();
            var currentTemperature = r1.Next(20, 45);
            Temperature = $"{currentTemperature} degree";
            OnTemperatureUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}
