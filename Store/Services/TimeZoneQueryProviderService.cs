using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorWasmPortfolioGhAction.Store.Services
{
    public class TimeZoneQueryProviderService : ITimeZoneQueryProviderService
    {
        public NavigationManager NavManager;

        public TimeZoneQueryProviderService(NavigationManager navManager)
        {
            NavManager = navManager;
        }

        public IEnumerable<String> GetTimeZones()
        {
            var uri = NavManager.ToAbsoluteUri(NavManager.Uri);

            var queryValues = QueryHelpers.ParseQuery(uri.Query);

            if (queryValues.TryGetValue("timeZone", out var timeZoneStringValues))
            {
                return (ICollection<string>)timeZoneStringValues.AsEnumerable();
            }

            return [];
        }
    }
}
