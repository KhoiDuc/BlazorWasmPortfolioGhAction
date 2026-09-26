using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public sealed class VnDeskDataService
{
    private readonly HttpClient _http;
    private readonly ILogger<VnDeskDataService> _logger;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private Dictionary<string, List<string>>? _lists;
    private Dictionary<string, List<string>>? _sectors;

    public VnDeskDataService(HttpClient http, ILogger<VnDeskDataService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<Dictionary<string, List<string>>> GetListsAsync()
    {
        if (_lists is not null) return _lists;
        try
        {
            _lists = await _http.GetFromJsonAsync<Dictionary<string, List<string>>>("trading/vndesk/lists.json", JsonOpts)
                     ?? new Dictionary<string, List<string>>();
            return _lists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load VN desk lists");
            throw;
        }
    }

    public async Task<Dictionary<string, List<string>>> GetSectorsAsync()
    {
        if (_sectors is not null) return _sectors;
        try
        {
            _sectors = await _http.GetFromJsonAsync<Dictionary<string, List<string>>>("trading/vndesk/sectors.json", JsonOpts)
                       ?? new Dictionary<string, List<string>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load VN desk sectors");
            throw;
        }
        foreach (var key in _sectors.Keys.ToList())
            _sectors[key] = _sectors[key].Select(s => s.ToUpperInvariant()).Distinct().ToList();
        return _sectors;
    }
}
