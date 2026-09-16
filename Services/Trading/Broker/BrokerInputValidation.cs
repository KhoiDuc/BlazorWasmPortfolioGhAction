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
            error = "Trading_BrokerVal_SymbolRequired";
            return false;
        }

        symbol = input.Trim().ToUpperInvariant();
        if (symbol.Length < SymbolMinLen || symbol.Length > SymbolMaxLen)
        {
            error = "Trading_BrokerVal_SymbolLength";
            return false;
        }

        if (!SymbolRegex().IsMatch(symbol))
        {
            error = "Trading_BrokerVal_SymbolFormat";
            return false;
        }

        error = null;
        return true;
    }

    public static bool TryValidateBuyPrice(decimal? price, out string? error)
    {
        if (price is not > 0)
        {
            error = "Trading_BrokerVal_PriceRequired";
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
            error = "Trading_BrokerVal_QtyPositive";
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
            error = "Trading_BrokerVal_NoteMax";
            return false;
        }

        error = null;
        return true;
    }
}
