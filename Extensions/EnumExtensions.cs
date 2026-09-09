namespace BlazorWasmPortfolioGhAction.Extensions;

/// <summary>
/// Enum helpers. Consolidated from <c>Shared/Model/Extensions.cs</c> (IsNullOrWhiteSpace wrapper dropped — use <see cref="string.IsNullOrWhiteSpace(string?)"/> directly).
/// </summary>
public static class EnumExtensions
{
    public static string GetName<T>(this T enumObj) where T : Enum =>
        Enum.GetName(typeof(T), enumObj) ?? "";

    public static int GetValue<T>(this T enumObj) where T : Enum =>
        Convert.ToInt32(enumObj);

    public static T ToEnum<T>(this string enumName) where T : Enum =>
        (T)Enum.Parse(typeof(T), enumName, true);
}