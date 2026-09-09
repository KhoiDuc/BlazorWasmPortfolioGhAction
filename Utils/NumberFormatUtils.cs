using System.Globalization;

namespace BlazorWasmPortfolioGhAction.Utils;

/// <summary>
/// Number/value formatting helpers. Consolidates:
/// - CryptoCard.razor + CryptoDetails.razor <c>FormatPrice</c> (duplicate)
/// - EmailComposer.razor <c>FormatSize</c>
/// - CommoditySpreadCards.razor <c>FormatVnValue</c>/<c>FormatMillions</c>
/// </summary>
public static class NumberFormatUtils
{
    /// <summary>Format crypto price: N2 for ≥1, N6 for sub-unit prices.</summary>
    public static string FormatCryptoPrice(decimal price) =>
        price >= 1 ? price.ToString("N2") : price.ToString("N6");

    /// <summary>Human-readable byte size (B/KB/MB).</summary>
    public static string FormatByteSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        _ => $"{bytes / (1024.0 * 1024):F1} MB"
    };

    private static readonly CultureInfo ViCulture = CultureInfo.GetCultureInfo("vi-VN");

    /// <summary>Format Vietnamese commodity value: đ for liters, "triệu" for millions otherwise.</summary>
    public static string FormatVnValue(double val, string unit) =>
        unit == "lít"
            ? val.ToString("N0", ViCulture) + " đ"
            : FormatMillions(val);

    /// <summary>Format millions in Vietnamese: "N,2 triệu".</summary>
    public static string FormatMillions(double val) =>
        (val / 1_000_000).ToString("N2", ViCulture) + " triệu";
}