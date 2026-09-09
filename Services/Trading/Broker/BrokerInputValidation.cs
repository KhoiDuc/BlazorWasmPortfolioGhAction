using System.Text.RegularExpressions;

namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

public static partial class BrokerInputValidation
{
    public const int SymbolMinLen = 3;
    public const int SymbolMaxLen = 10;
    public const int NoteMaxLen = 2000;
    public const int LotSize = 100;

    [GeneratedRegex("^[A-Z0-9]+$", RegexOptions.CultureInvariant)]
    private static partial Regex SymbolRegex();

    public static bool TryNormalizeSymbol(string? input, out string symbol, out string? error)
    {
        symbol = "";
        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Mã CP bắt buộc.";
            return false;
        }

        symbol = input.Trim().ToUpperInvariant();
        if (symbol.Length < SymbolMinLen || symbol.Length > SymbolMaxLen)
        {
            error = $"Mã CP: {SymbolMinLen}–{SymbolMaxLen} ký tự.";
            return false;
        }

        if (!SymbolRegex().IsMatch(symbol))
        {
            error = "Mã CP chỉ gồm chữ và số (A–Z, 0–9).";
            return false;
        }

        error = null;
        return true;
    }

    public static bool TryValidateBuyPrice(decimal? price, out string? error)
    {
        if (price is not > 0)
        {
            error = "Giá mua bắt buộc và > 0.";
            return false;
        }

        error = null;
        return true;
    }

    public static bool TryValidateQuantity(decimal? qty, out string? error)
    {
        if (qty is null or 0)
        {
            error = null;
            return true;
        }

        if (qty <= 0)
        {
            error = "KL phải > 0 nếu nhập.";
            return false;
        }

        error = null;
        return true;
    }

    public static bool TryValidateNote(string? note, out string? error)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            error = null;
            return true;
        }

        if (note.Length > NoteMaxLen)
        {
            error = $"Note tối đa {NoteMaxLen} ký tự.";
            return false;
        }

        error = null;
        return true;
    }
}
