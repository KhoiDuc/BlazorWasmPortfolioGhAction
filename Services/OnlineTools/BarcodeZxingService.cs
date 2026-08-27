using System.Text;
using ZXing;
using ZXing.Common;
using ZXing.PDF417.Internal;
using ZXing.QrCode.Internal;

namespace BlazorWasmPortfolioGhAction.Services.OnlineTools;

public sealed class BarcodeGenerateRequest
{
    public string Content { get; set; } = "";
    public BarcodeFormat Format { get; set; } = BarcodeFormat.QR_CODE;
    public int Width { get; set; } = 300;
    public int Height { get; set; } = 300;
    public int MarginTop { get; set; } = 10;
    public int MarginRight { get; set; } = 10;
    public int MarginBottom { get; set; } = 10;
    public int MarginLeft { get; set; } = 10;
    public bool ShowTextBelow { get; set; } = true;
    public int FontSize { get; set; } = 14;
    public bool EnableGS1 { get; set; }
    public string QrErrorCorrectionLevel { get; set; } = "M";
    public int Pdf417ErrorLevel { get; set; } = 2;
    public bool Pdf417Compact { get; set; }
    public int AztecErrorPercent { get; set; } = 33;
}

public sealed class BarcodeGenerateResult
{
    public bool Success => string.IsNullOrEmpty(ErrorMessage);
    public string? Svg { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed class BarcodeDecodeHit
{
    public string Text { get; init; } = "";
    public string Format { get; init; } = "";
    public string Dimension { get; init; } = "";
    public bool IsGs1 { get; init; }
    public string? RawHex { get; init; }
    public IReadOnlyList<(string Key, string Value)> Metadata { get; init; } = [];
}

public sealed class BarcodeZxingService
{
    private static readonly BarcodeFormat[] Formats =
    [
        BarcodeFormat.AZTEC,
        BarcodeFormat.CODABAR,
        BarcodeFormat.CODE_39,
        BarcodeFormat.CODE_93,
        BarcodeFormat.CODE_128,
        BarcodeFormat.DATA_MATRIX,
        BarcodeFormat.EAN_8,
        BarcodeFormat.EAN_13,
        BarcodeFormat.ITF,
        BarcodeFormat.MSI,
        BarcodeFormat.PDF_417,
        BarcodeFormat.PLESSEY,
        BarcodeFormat.QR_CODE,
        BarcodeFormat.UPC_A,
        BarcodeFormat.UPC_E,
    ];

    public IReadOnlyList<BarcodeFormat> SupportedFormats => Formats;

    public bool Is1D(BarcodeFormat format) => format switch
    {
        BarcodeFormat.QR_CODE or BarcodeFormat.DATA_MATRIX or BarcodeFormat.AZTEC
            or BarcodeFormat.PDF_417 or BarcodeFormat.MAXICODE => false,
        _ => true
    };

    public bool SupportsGS1(BarcodeFormat format) => format is
        BarcodeFormat.CODE_128 or BarcodeFormat.DATA_MATRIX or BarcodeFormat.QR_CODE;

    public BarcodeGenerateResult Generate(BarcodeGenerateRequest options)
    {
        if (string.IsNullOrWhiteSpace(options.Content))
            return new BarcodeGenerateResult { ErrorMessage = "Content cannot be empty." };

        try
        {
            var encoding = CreateEncodingOptions(options);
            var writer = new BarcodeWriterGeneric
            {
                Format = options.Format,
                Options = encoding
            };

            var matrix = writer.Encode(options.Content);
            var textHeight = Is1D(options.Format) && options.ShowTextBelow ? options.FontSize + 8 : 0;
            var svg = RenderSvg(matrix, options, textHeight);
            return new BarcodeGenerateResult { Svg = svg };
        }
        catch (Exception ex)
        {
            return new BarcodeGenerateResult
            {
                ErrorMessage = $"{ex.Message} Check that the content is valid for the selected format."
            };
        }
    }

    public IReadOnlyList<BarcodeDecodeHit> DecodeRgba(byte[] rgba, int width, int height)
    {
        if (rgba.Length < width * height * 4 || width <= 0 || height <= 0)
            return [];

        var source = new RGBLuminanceSource(rgba, width, height, RGBLuminanceSource.BitmapFormat.RGBA32);
        var reader = new BarcodeReaderGeneric
        {
            AutoRotate = true,
            Options = new DecodingOptions
            {
                TryHarder = true,
                TryInverted = true,
            }
        };

        var results = reader.DecodeMultiple(source);
        if (results is null || results.Length == 0)
            return [];

        return results.Select(ToHit).ToList();
    }

    private static BarcodeDecodeHit ToHit(Result r)
    {
        var meta = new List<(string, string)>();
        if (r.ResultMetadata is not null)
        {
            foreach (var kv in r.ResultMetadata)
                meta.Add((kv.Key.ToString()!, FormatMeta(kv.Value)));
        }

        return new BarcodeDecodeHit
        {
            Text = r.Text ?? "",
            Format = r.BarcodeFormat.ToString(),
            Dimension = r.BarcodeFormat is BarcodeFormat.QR_CODE or BarcodeFormat.DATA_MATRIX
                or BarcodeFormat.AZTEC or BarcodeFormat.PDF_417 or BarcodeFormat.MAXICODE
                ? "2D" : "1D",
            IsGs1 = IsGs1(r),
            RawHex = r.RawBytes is { Length: > 0 } bytes
                ? BitConverter.ToString(bytes).Replace("-", " ", StringComparison.Ordinal)
                : null,
            Metadata = meta
        };
    }

    private static string FormatMeta(object? value)
    {
        if (value is null) return "";
        if (value is byte[] bytes) return BitConverter.ToString(bytes).Replace("-", " ", StringComparison.Ordinal);
        if (value is Array arr) return string.Join(", ", arr.Cast<object?>());
        return value.ToString() ?? "";
    }

    private static bool IsGs1(Result result)
    {
        if (result.ResultMetadata is not null
            && result.ResultMetadata.TryGetValue(ResultMetadataType.SYMBOLOGY_IDENTIFIER, out var value)
            && value?.ToString() is { } id
            && id is "]C1" or "]e0" or "]d2" or "]Q3")
        {
            return true;
        }

        var text = result.Text;
        if (string.IsNullOrEmpty(text)) return false;
        if ((text.StartsWith("01", StringComparison.Ordinal) || text.StartsWith("(01)", StringComparison.Ordinal)
             || text.StartsWith("02", StringComparison.Ordinal) || text.StartsWith("(02)", StringComparison.Ordinal)
             || text.StartsWith("10", StringComparison.Ordinal) || text.StartsWith("(10)", StringComparison.Ordinal)
             || text.StartsWith("21", StringComparison.Ordinal) || text.StartsWith("(21)", StringComparison.Ordinal))
            && text.Length >= 14)
        {
            return true;
        }

        return false;
    }

    private static string GetDisplayText(string content, BarcodeFormat format) => format switch
    {
        BarcodeFormat.EAN_8 when content.Length == 7 => content + CalculateEanCheckDigit(content),
        BarcodeFormat.EAN_13 when content.Length == 12 => content + CalculateEanCheckDigit(content),
        BarcodeFormat.UPC_A when content.Length == 11 => content + CalculateEanCheckDigit(content),
        BarcodeFormat.UPC_E when content.Length == 7 => content + CalculateUpcECheckDigit(content),
        _ => content
    };

    private static char CalculateEanCheckDigit(string digits)
    {
        var sum = 0;
        var isOdd = true;
        for (var i = digits.Length - 1; i >= 0; i--)
        {
            var digit = digits[i] - '0';
            sum += isOdd ? digit * 3 : digit;
            isOdd = !isOdd;
        }

        return (char)('0' + (10 - sum % 10) % 10);
    }

    private static char CalculateUpcECheckDigit(string upcE)
    {
        if (upcE.Length != 7) return CalculateEanCheckDigit(upcE);

        var lastDigit = upcE[6];
        string manufacturer;
        string product;
        switch (lastDigit)
        {
            case '0':
            case '1':
            case '2':
                manufacturer = upcE.Substring(1, 2) + lastDigit + "00";
                product = string.Concat("00", upcE.AsSpan(3, 3));
                break;
            case '3':
                manufacturer = upcE.Substring(1, 3) + "00";
                product = string.Concat("000", upcE.AsSpan(4, 2));
                break;
            case '4':
                manufacturer = upcE.Substring(1, 4) + "0";
                product = "0000" + upcE[5];
                break;
            default:
                manufacturer = upcE.Substring(1, 5);
                product = "0000" + lastDigit;
                break;
        }

        return CalculateEanCheckDigit(upcE[0] + manufacturer + product);
    }

    private string RenderSvg(BitMatrix matrix, BarcodeGenerateRequest options, int textHeight)
    {
        // Clone via copy — Render mutates cells when coalescing rectangles.
        var clone = new BitMatrix(matrix.Width, matrix.Height);
        for (var y = 0; y < matrix.Height; y++)
        for (var x = 0; x < matrix.Width; x++)
            if (matrix[x, y]) clone[x, y] = true;

        var sb = new StringBuilder();
        var mw = clone.Width;
        var mh = clone.Height;
        var totalW = mw + options.MarginLeft + options.MarginRight;
        var totalH = mh + options.MarginTop + options.MarginBottom + textHeight;

        sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {totalW} {totalH}\" width=\"{totalW}\" height=\"{totalH}\" shape-rendering=\"crispEdges\">");
        sb.AppendLine($"<rect width=\"{totalW}\" height=\"{totalH}\" fill=\"white\"/>");

        for (var y = 0; y < mh; y++)
        {
            var x = 0;
            while (x < mw)
            {
                if (clone[x, y])
                {
                    var startX = x;
                    var startY = y;
                    while (x < mw && clone[x, y]) x++;
                    var rectWidth = x - startX;
                    var rectHeight = 1;
                    var canExtend = true;
                    while (canExtend && startY + rectHeight < mh)
                    {
                        for (var checkX = startX; checkX < startX + rectWidth; checkX++)
                        {
                            if (!clone[checkX, startY + rectHeight])
                            {
                                canExtend = false;
                                break;
                            }
                        }

                        if (!canExtend) continue;
                        for (var clearX = startX; clearX < startX + rectWidth; clearX++)
                            clone[clearX, startY + rectHeight] = false;
                        rectHeight++;
                    }

                    sb.AppendLine($"<rect x=\"{startX + options.MarginLeft}\" y=\"{startY + options.MarginTop}\" width=\"{rectWidth}\" height=\"{rectHeight}\" fill=\"black\"/>");
                }
                else
                {
                    x++;
                }
            }
        }

        if (Is1D(options.Format) && options.ShowTextBelow && textHeight > 0)
        {
            var textX = options.MarginLeft + mw / 2;
            var textY = options.MarginTop + mh + options.FontSize;
            var display = System.Security.SecurityElement.Escape(GetDisplayText(options.Content, options.Format));
            sb.AppendLine($"<text x=\"{textX}\" y=\"{textY}\" text-anchor=\"middle\" font-family=\"Consolas, monospace\" font-size=\"{options.FontSize}\" fill=\"black\">{display}</text>");
        }

        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    private static EncodingOptions CreateEncodingOptions(BarcodeGenerateRequest options)
    {
        switch (options.Format)
        {
            case BarcodeFormat.QR_CODE:
            {
                var qrLevel = options.QrErrorCorrectionLevel switch
                {
                    "L" => ErrorCorrectionLevel.L,
                    "Q" => ErrorCorrectionLevel.Q,
                    "H" => ErrorCorrectionLevel.H,
                    _ => ErrorCorrectionLevel.M
                };
                var qr = new ZXing.QrCode.QrCodeEncodingOptions
                {
                    ErrorCorrection = qrLevel,
                    Width = options.Width,
                    Height = options.Height,
                    Margin = 0,
                    CharacterSet = "UTF-8"
                };
                if (options.EnableGS1)
                    qr.Hints[EncodeHintType.GS1_FORMAT] = true;
                return qr;
            }
            case BarcodeFormat.DATA_MATRIX:
                return new ZXing.Datamatrix.DatamatrixEncodingOptions
                {
                    Width = options.Width,
                    Height = options.Height,
                    Margin = 0,
                    GS1Format = options.EnableGS1
                };
            case BarcodeFormat.CODE_128:
            {
                var opts = new EncodingOptions
                {
                    Width = options.Width,
                    Height = options.Height,
                    Margin = 0,
                    PureBarcode = true
                };
                if (options.EnableGS1)
                    opts.Hints[EncodeHintType.GS1_FORMAT] = true;
                return opts;
            }
            case BarcodeFormat.PDF_417:
                return new ZXing.PDF417.PDF417EncodingOptions
                {
                    Width = options.Width,
                    Height = options.Height,
                    Margin = 1,
                    ErrorCorrection = (PDF417ErrorCorrectionLevel)Math.Clamp(options.Pdf417ErrorLevel, 0, 8),
                    Compact = options.Pdf417Compact
                };
            case BarcodeFormat.AZTEC:
                return new ZXing.Aztec.AztecEncodingOptions
                {
                    Width = options.Width,
                    Height = options.Height,
                    Margin = 0,
                    ErrorCorrection = Math.Clamp(options.AztecErrorPercent, 0, 100)
                };
            case BarcodeFormat.MSI:
            case BarcodeFormat.PLESSEY:
                return new EncodingOptions
                {
                    Width = options.Width,
                    Height = options.Height,
                    Margin = 1
                };
            default:
                return new EncodingOptions
                {
                    Width = options.Width,
                    Height = options.Height,
                    Margin = 0,
                    PureBarcode = true
                };
        }
    }
}
