using System;
using System.Collections.Generic;

namespace BlazorWasmPortfolioGhAction.Store.Services
{
    public interface ITimeZoneQueryProviderService
    {
        public IEnumerable<String> GetTimeZones();
    }
}
