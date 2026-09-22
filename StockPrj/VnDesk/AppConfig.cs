using System.Text.Json;

namespace VnDesk;

public sealed class AppConfig
{
    public string VndirectBaseUrl { get; set; } = "https://api-finfo.vndirect.com.vn";
    public string CafefIndexUrl { get; set; } = "https://banggia.cafef.vn/stockhandler.ashx?index=true";
    public string CafefAllStocksUrl { get; set; } = "https://banggia.cafef.vn/stockhandler.ashx?allstocks=true";
    public string PythonExe { get; set; } = "python";
    public string IntradayScript { get; set; } = "scripts/fetch_stock.py";
    public string OllamaUrl { get; set; } = "http://127.0.0.1:11434";
    public string OllamaModel { get; set; } = "llama3.1";
    public int HistorySessions { get; set; } = 250;
    public int ScreenerDays { get; set; } = 60;

    public static AppConfig Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
            return new AppConfig();

        var json = File.ReadAllText(path);
        var cfg = JsonSerializer.Deserialize<AppConfig>(json, JsonStore.Read);
        return cfg ?? new AppConfig();
    }

    public string DataPath(string fileName) => Path.Combine(AppContext.BaseDirectory, "data", fileName);

    public string IntradayScriptFullPath =>
        Path.Combine(AppContext.BaseDirectory, IntradayScript.Replace('/', Path.DirectorySeparatorChar));
}
