namespace BlazorWasmPortfolioGhAction.Store.Services
{
    public interface ITimeZoneQueryProviderService
    {
        public IEnumerable<String> GetTimeZones();
    }
}
