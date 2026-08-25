using System.Net.Http.Json;
using System.Text.Json;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public sealed class VnDeskDataService
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private Dictionary<string, List<string>>? _lists;
    private Dictionary<string, List<string>>? _sectors;

    public VnDeskDataService(HttpClient http) => _http = http;

    public async Task<Dictionary<string, List<string>>> GetListsAsync()
    {
        if (_lists is not null) return _lists;
        _lists = await _http.GetFromJsonAsync<Dictionary<string, List<string>>>("trading/vndesk/lists.json", JsonOpts)
                 ?? new Dictionary<string, List<string>>();
        return _lists;
    }

    public async Task<Dictionary<string, List<string>>> GetSectorsAsync()
    {
        if (_sectors is not null) return _sectors;
        _sectors = await _http.GetFromJsonAsync<Dictionary<string, List<string>>>("trading/vndesk/sectors.json", JsonOpts)
                   ?? new Dictionary<string, List<string>>();
        foreach (var key in _sectors.Keys.ToList())
            _sectors[key] = _sectors[key].Select(s => s.ToUpperInvariant()).Distinct().ToList();
        return _sectors;
    }
}
