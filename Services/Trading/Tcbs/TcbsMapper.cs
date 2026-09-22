using System.Globalization;
using System.Text.Json;
using BlazorWasmPortfolioGhAction.Models.Trading.Tcbs;

namespace BlazorWasmPortfolioGhAction.Services.Trading.Tcbs;

public static class TcbsMapper
{
    public static decimal DeskPrice(decimal raw)
    {
        if (raw <= 0) return 0;
        return raw >= 1000 ? raw / 1000m : raw;
    }

    public static TcbsStatus ReadStatus(string json)
    {
        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        var root = doc.RootElement;
        return new TcbsStatus
        {
            Connected = Bool(root, "connected"),
            NeedsReauth = Bool(root, "needsReauth"),
            ReadOnly = !root.TryGetProperty("readOnly", out var ro) || ro.ValueKind != JsonValueKind.False,
            CustodyCode = Str(root, "custodyCode"),
            AccountNo = Str(root, "accountNo"),
        };
    }

    public static List<TcbsSubAccount> ReadAccounts(string json)
    {
        var list = new List<TcbsSubAccount>();
        Walk(Parse(json), el =>
        {
            var no = Str(el, "accountNo", "acctno");
            if (string.IsNullOrWhiteSpace(no) || list.Any(x => x.AccountNo == no)) return;
            var type = Str(el, "accountType", "accountTypeName", "aftype");
            list.Add(new TcbsSubAccount
            {
                AccountNo = no,
                Name = Str(el, "accountName", "bankName"),
                Type = type,
                Status = Str(el, "status", "accountStatus"),
                IsDefault = Str(el, "isDefault").Equals("Y", StringComparison.OrdinalIgnoreCase),
                IsDerivative = type.Contains("DERIV", StringComparison.OrdinalIgnoreCase),
            });
        });
        return list.Where(a => !a.IsDerivative).ToList();
    }

    public static List<TcbsAsset> ReadAssets(string json)
    {
        var list = new List<TcbsAsset>();
        Walk(Parse(json), el =>
        {
            var symbol = Str(el, "symbol", "ticker").ToUpperInvariant();
            if (symbol.Length < 3 || list.Any(x => x.Symbol == symbol)) return;
            if (!el.TryGetProperty("quantity", out _) && !el.TryGetProperty("qty", out _)) return;
            list.Add(new TcbsAsset
            {
                Symbol = symbol,
                Quantity = Num(el, "quantity", "qty"),
                AvgPrice = DeskPrice(Num(el, "avgPrice", "averagePrice", "costPrice")),
                MarketValue = Num(el, "marketValue", "marketVal"),
            });
        });
        return list.Where(a => a.Quantity > 0).ToList();
    }

    public static TcbsCash? ReadCash(string json)
    {
        TcbsCash? cash = null;
        Walk(Parse(json), el =>
        {
            if (cash is not null) return;
            if (!Has(el, "cashBalance") && !Has(el, "bodBalance")) return;
            cash = new TcbsCash
            {
                AccountNo = Str(el, "accountNo"),
                CashBalance = Num(el, "cashBalance"),
                BodBalance = Num(el, "bodBalance"),
            };
        });
        return cash;
    }

    public static List<TcbsStatementRow> ReadStatement(string json)
    {
        var list = new List<TcbsStatementRow>();
        Walk(Parse(json), el =>
        {
            if (!Has(el, "debitAmount") && !Has(el, "creditAmount") && !Has(el, "transactionCode")) return;
            list.Add(new TcbsStatementRow
            {
                Date = When(el, "transactionDate", "businessDate") ?? DateTime.MinValue,
                Code = Str(el, "transactionCode"),
                Name = Str(el, "transactionName"),
                Debit = Num(el, "debitAmount"),
                Credit = Num(el, "creditAmount"),
                Description = Str(el, "descriptions", "description"),
            });
        });
        return list.Where(r => r.Date != DateTime.MinValue || r.Debit != 0 || r.Credit != 0).ToList();
    }

    public static List<TcbsOrder> ReadOrders(string json)
    {
        var list = new List<TcbsOrder>();
        Walk(Parse(json), el =>
        {
            var id = Str(el, "orderId", "orderID");
            var symbol = Str(el, "symbol").ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(symbol)) return;
            if (list.Any(x => x.OrderId == id)) return;
            list.Add(new TcbsOrder
            {
                OrderId = id,
                Symbol = symbol,
                ExecType = Str(el, "execType"),
                Price = DeskPrice(Num(el, "price", "limitPrice")),
                Quantity = Num(el, "quantity", "orderQtty"),
                ExecQuantity = Num(el, "execQtty", "execQuantity"),
                Status = Str(el, "status", "orStatus"),
                PriceType = Str(el, "priceType"),
                Time = When(el, "txdate", "txtime"),
            });
        });
        return list;
    }

    public static List<TcbsMatch> ReadMatches(string json)
    {
        var list = new List<TcbsMatch>();
        Walk(Parse(json), el =>
        {
            var symbol = Str(el, "symbol").ToUpperInvariant();
            if (symbol.Length < 3) return;
            if (!Has(el, "qtty") && !Has(el, "quantity") && !Has(el, "price") && !Has(el, "p")) return;
            var side = Str(el, "side", "execType", "a");
            list.Add(new TcbsMatch
            {
                OrderId = Str(el, "orderId", "orderID"),
                TradeId = Str(el, "tradeId"),
                Symbol = symbol,
                Side = side,
                Quantity = Num(el, "qtty", "quantity", "v"),
                Price = DeskPrice(Num(el, "price", "p")),
                Time = When(el, "timeExec", "t", "time"),
            });
        });
        return list.Where(m => m.Quantity > 0 && m.Price > 0).ToList();
    }

    public static List<TcbsQuote> ReadQuotes(string json)
    {
        var list = new List<TcbsQuote>();
        Walk(Parse(json), el =>
        {
            var symbol = Str(el, "symbol", "ticker").ToUpperInvariant();
            if (symbol.Length < 3 || !Has(el, "matchPrice") && !Has(el, "refPrice")) return;
            if (list.Any(x => x.Symbol == symbol)) return;
            list.Add(new TcbsQuote
            {
                Symbol = symbol,
                MatchPrice = DeskPrice(Num(el, "matchPrice")),
                CeilPrice = DeskPrice(Num(el, "ceilPrice")),
                FloorPrice = DeskPrice(Num(el, "floorPrice")),
                RefPrice = DeskPrice(Num(el, "refPrice")),
                ChangePercent = Num(el, "changePercent"),
                TotalVolume = Num(el, "totalVol", "totalVolume"),
                BuyForeign = Num(el, "buyForeignQtty"),
                SellForeign = Num(el, "sellForeignQtty"),
                Room = Str(el, "room"),
            });
        });
        return list;
    }

    public static TcbsBuyingPower ReadBuyingPower(string json)
    {
        var power = new TcbsBuyingPower();
        Walk(Parse(json), el =>
        {
            if (power.MaxQuantity > 0 || power.PurchasingPower > 0) return;
            power.PurchasingPower = Num(el, "purchasingPower", "pp0");
            power.MaxQuantity = Num(el, "maxQuantity", "maxQtty");
        });
        return power;
    }

    public static TcbsOrderPreview? ReadPreview(string json)
    {
        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        var root = doc.RootElement;
        var token = Str(root, "confirmToken");
        if (string.IsNullOrWhiteSpace(token)) return null;
        var order = root.TryGetProperty("order", out var o) ? o : default;
        var cost = root.TryGetProperty("cost", out var c) ? c : default;
        return new TcbsOrderPreview
        {
            ConfirmToken = token,
            ReadOnly = Bool(root, "readOnly"),
            Symbol = order.ValueKind == JsonValueKind.Object ? Str(order, "symbol") : "",
            ExecType = order.ValueKind == JsonValueKind.Object ? Str(order, "execType") : "",
            Quantity = order.ValueKind == JsonValueKind.Object ? (int)Num(order, "quantity") : 0,
            PriceVnd = order.ValueKind == JsonValueKind.Object ? Num(order, "priceVnd") : 0,
            Exchange = order.ValueKind == JsonValueKind.Object ? Str(order, "exchange") : "",
            Notional = cost.ValueKind == JsonValueKind.Object ? Num(cost, "notional") : 0,
            Fee = cost.ValueKind == JsonValueKind.Object ? Num(cost, "fee") : 0,
            Tax = cost.ValueKind == JsonValueKind.Object ? Num(cost, "tax") : 0,
            Total = cost.ValueKind == JsonValueKind.Object ? Num(cost, "total") : 0,
        };
    }

    public static List<TcbsSupplyPoint> ReadSupply(string json)
    {
        var list = new List<TcbsSupplyPoint>();
        Walk(Parse(json), el =>
        {
            if (!Has(el, "bsr") && !Has(el, "bu") && !Has(el, "sd")) return;
            list.Add(new TcbsSupplyPoint
            {
                Time = Str(el, "t"),
                Buy = Num(el, "bu", "bup"),
                Sell = Num(el, "sd", "sdp"),
                Ratio = Num(el, "bsr"),
            });
        });
        return list;
    }

    public static List<TcbsPutThrough> ReadPutThrough(string json)
    {
        var list = new List<TcbsPutThrough>();
        void Take(JsonElement parent, string prop, string kind, string side)
        {
            if (!parent.TryGetProperty(prop, out var arr) || arr.ValueKind != JsonValueKind.Array) return;
            foreach (var el in arr.EnumerateArray())
            {
                list.Add(new TcbsPutThrough
                {
                    Symbol = Str(el, "symbol"),
                    Side = string.IsNullOrWhiteSpace(Str(el, "side")) ? side : Str(el, "side"),
                    Price = DeskPrice(Num(el, "price")),
                    Volume = Num(el, "vol"),
                    Time = Str(el, "time"),
                    Kind = kind,
                });
            }
        }

        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        var root = doc.RootElement;
        Take(root, "buyAdv", "Quảng cáo mua", "B");
        Take(root, "sellAdv", "Quảng cáo bán", "S");
        Take(root, "match", "Khớp", "");
        return list;
    }

    public static TcbsWsTicket? ReadTicket(string json)
    {
        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        var root = doc.RootElement;
        var ticket = Str(root, "ticket");
        if (string.IsNullOrWhiteSpace(ticket)) return null;
        return new TcbsWsTicket
        {
            Ticket = ticket,
            Stream = Str(root, "stream"),
            Symbol = Str(root, "symbol"),
            RelayUrl = Str(root, "relayUrl"),
            ExpiresInSec = (int)Num(root, "expiresInSec"),
        };
    }

    public static string ReadError(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return "";
        try
        {
            using var doc = JsonDocument.Parse(json);
            return Str(doc.RootElement, "message", "error");
        }
        catch
        {
            return json.Length > 240 ? json[..240] : json;
        }
    }

    private static JsonElement Parse(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
            return doc.RootElement.Clone();
        }
        catch
        {
            return default;
        }
    }

    private static void Walk(JsonElement el, Action<JsonElement> visit)
    {
        if (el.ValueKind == JsonValueKind.Object)
        {
            visit(el);
            foreach (var prop in el.EnumerateObject())
                Walk(prop.Value, visit);
        }
        else if (el.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in el.EnumerateArray())
                Walk(item, visit);
        }
    }

    private static bool Has(JsonElement el, string name) =>
        el.ValueKind == JsonValueKind.Object && el.TryGetProperty(name, out var p) && p.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined;

    private static string Str(JsonElement el, params string[] names)
    {
        if (el.ValueKind != JsonValueKind.Object) return "";
        foreach (var name in names)
        {
            if (!el.TryGetProperty(name, out var p)) continue;
            if (p.ValueKind == JsonValueKind.String) return p.GetString() ?? "";
            if (p.ValueKind == JsonValueKind.Number) return p.ToString();
        }
        return "";
    }

    private static decimal Num(JsonElement el, params string[] names)
    {
        var text = Str(el, names);
        return decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var n) ? n : 0;
    }

    private static bool Bool(JsonElement el, string name)
    {
        if (el.ValueKind != JsonValueKind.Object || !el.TryGetProperty(name, out var p)) return false;
        return p.ValueKind == JsonValueKind.True;
    }

    private static DateTime? When(JsonElement el, params string[] names)
    {
        var text = Str(el, names);
        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt)) return dt;
        if (long.TryParse(text, out var unix))
        {
            try { return unix > 10_000_000_000 ? DateTimeOffset.FromUnixTimeMilliseconds(unix).LocalDateTime : DateTimeOffset.FromUnixTimeSeconds(unix).LocalDateTime; }
            catch { return null; }
        }
        return null;
    }
}
