using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace BlazorWasmPortfolioGhAction.Utils.OnlineTools;

public static class NavigationManagerExtensions
{
    public static void SetParametersFromQueryString(this NavigationManager navigationManager, IComponent component)
    {
        var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);

        foreach (var property in GetQueryProperties(component.GetType()))
        {
            var parameterName = GetQueryStringParameterName(property);
            if (parameterName is null)
                continue;

            if (query.TryGetValue(parameterName, out var value))
                property.SetValue(component, ConvertValue(value.ToString(), property.PropertyType));
        }
    }

    public static void UpdateUrlUsingParameters(this NavigationManager navigationManager, IComponent component, bool replaceHistory = false)
    {
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var property in GetQueryProperties(component.GetType()))
        {
            var parameterName = GetQueryStringParameterName(property);
            if (parameterName is null)
                continue;

            parameters[parameterName] = property.GetValue(component);
        }

        var newUri = navigationManager.GetUriWithQueryParameters(parameters);
        if (!string.Equals(newUri, navigationManager.Uri, StringComparison.Ordinal))
            navigationManager.NavigateTo(newUri, replaceHistory);
    }

    private static PropertyInfo[] GetQueryProperties(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

    private static string? GetQueryStringParameterName(PropertyInfo property)
    {
        if (property.GetCustomAttribute<ParameterAttribute>() is null)
            return null;

        if (property.GetCustomAttribute<SupplyParameterFromQueryAttribute>() is not { } attribute)
            return null;

        return attribute.Name ?? property.Name;
    }

    private static object? ConvertValue(string? value, Type type)
    {
        if (type == typeof(string))
            return value;

        if (string.IsNullOrEmpty(value))
            return type.IsValueType ? Activator.CreateInstance(type) : null;

        if (type == typeof(bool) || type == typeof(bool?))
            return bool.TryParse(value, out var boolValue) && boolValue;

        if (type.IsEnum)
            return Enum.Parse(type, value, ignoreCase: true);

        try
        {
            return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }
        catch
        {
            return JsonSerializer.Deserialize(value, type);
        }
    }
}
