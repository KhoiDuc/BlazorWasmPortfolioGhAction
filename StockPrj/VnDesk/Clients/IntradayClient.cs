using System.Diagnostics;
using System.Text.Json;
using VnDesk.Models;

namespace VnDesk.Clients;

public sealed class IntradayClient
{
    private readonly AppConfig _cfg;

    public IntradayClient(AppConfig cfg) => _cfg = cfg;

    public List<IntradayData> Fetch(string symbol)
    {
        var script = _cfg.IntradayScriptFullPath;
        if (!File.Exists(script))
            return [];

        var outFile = Path.Combine(AppContext.BaseDirectory, "data", $"intraday-{symbol}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(outFile)!);

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = _cfg.PythonExe,
                Arguments = $"\"{script}\" {symbol} \"{outFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8
            };

            using var process = Process.Start(psi);
            if (process is null)
                return [];
            var stdout = process.StandardOutput.ReadToEnd();
            process.WaitForExit(45_000);

            var raw = File.Exists(outFile) ? File.ReadAllText(outFile) : stdout;
            return Parse(raw);
        }
        catch
        {
            return [];
        }
    }

    private static List<IntradayData> Parse(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return [];

        var startArr = output.IndexOf('[');
        var startObj = output.IndexOf('{');
        var start = startArr >= 0 ? startArr : startObj;
        if (start < 0)
            return [];

        try
        {
            using var doc = JsonDocument.Parse(output[start..]);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("error", out _))
                return [];
            if (root.ValueKind != JsonValueKind.Array)
                return [];

            var list = new List<IntradayData>();
            foreach (var el in root.EnumerateArray())
            {
                if (!el.TryGetProperty("time", out var timeEl) ||
                    !el.TryGetProperty("price", out var priceEl) ||
                    !el.TryGetProperty("volume", out var volEl))
                    continue;

                var match = el.TryGetProperty("match_type", out var mt) ? mt.GetString() ?? "" : "";
                list.Add(new IntradayData
                {
                    Time = DateTime.Parse(timeEl.GetString() ?? DateTime.Now.ToString("s")),
                    Price = Convert.ToDecimal(priceEl.GetDouble() / 1000.0),
                    Volume = volEl.ValueKind == JsonValueKind.Number ? Convert.ToDecimal(volEl.GetDouble()) : 0,
                    MatchType = match
                });
            }
            return list;
        }
        catch
        {
            return [];
        }
    }
}
