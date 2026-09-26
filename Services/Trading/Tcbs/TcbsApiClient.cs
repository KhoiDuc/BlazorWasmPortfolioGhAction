using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BlazorWasmPortfolioGhAction.Models.Trading.Tcbs;
using BlazorWasmPortfolioGhAction.Services.Trading.Broker;
using Microsoft.Extensions.Logging;

namespace BlazorWasmPortfolioGhAction.Services.Trading.Tcbs;

public interface ITcbsApiClient
{
    Task<TcbsStatus> GetStatusAsync(CancellationToken ct = default);
    Task<TcbsCall> ConnectAsync(string apiKey, string otp, string? custodyCode, CancellationToken ct = default);
    Task<TcbsCall> DisconnectAsync(CancellationToken ct = default);
    Task<TcbsCall> SetReadOnlyAsync(bool readOnly, CancellationToken ct = default);
    Task<TcbsCall> GetAsync(string pathAndQuery, CancellationToken ct = default);
    Task<TcbsCall> SendAsync(HttpMethod method, string pathAndQuery, object? body, string? idempotencyKey = null, CancellationToken ct = default);
    Task<List<TcbsQuote>> GetQuotesAsync(IReadOnlyList<string> symbols, CancellationToken ct = default);
    Task<TcbsOrderPreview?> PreviewOrderAsync(object body, CancellationToken ct = default);
    Task<TcbsCall> PlaceOrderAsync(object body, string idempotencyKey, CancellationToken ct = default);
    Task<TcbsWsTicket?> CreateStreamTicketAsync(string stream, string symbol, CancellationToken ct = default);
}

public sealed class TcbsApiClient : ITcbsApiClient
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly IBrokerAuthService _auth;
    private readonly ILogger<TcbsApiClient> _logger;
    private TcbsStatus? _status;
    private DateTime _statusAt;

    public TcbsApiClient(HttpClient http, IConfiguration config, IBrokerAuthService auth, ILogger<TcbsApiClient> logger)
    {
        _http = http;
        _config = config;
        _auth = auth;
        _logger = logger;
    }

    private string? BaseUrl => _config["BrokerApi:BaseUrl"]?.Trim().TrimEnd('/');

    public async Task<TcbsStatus> GetStatusAsync(CancellationToken ct = default)
    {
        if (_status is not null && DateTime.UtcNow - _statusAt < TimeSpan.FromSeconds(20))
            return _status;

        var call = await GetAsync("status", ct);
        _status = call.Ok ? TcbsMapper.ReadStatus(call.Json) : new TcbsStatus { NeedsReauth = call.NeedsReauth };
        _statusAt = DateTime.UtcNow;
        return _status;
    }

    public Task<TcbsCall> ConnectAsync(string apiKey, string otp, string? custodyCode, CancellationToken ct = default)
    {
        _status = null;
        return SendAsync(HttpMethod.Post, "connect", new { apiKey, otp, custodyCode }, ct: ct);
    }

    public Task<TcbsCall> DisconnectAsync(CancellationToken ct = default)
    {
        _status = null;
        return SendAsync(HttpMethod.Post, "disconnect", new { }, ct: ct);
    }

    public Task<TcbsCall> SetReadOnlyAsync(bool readOnly, CancellationToken ct = default)
    {
        _status = null;
        return SendAsync(HttpMethod.Post, "trading-mode", new { readOnly }, ct: ct);
    }

    public Task<TcbsCall> GetAsync(string pathAndQuery, CancellationToken ct = default) =>
        SendAsync(HttpMethod.Get, pathAndQuery, null, ct: ct);

    public async Task<List<TcbsQuote>> GetQuotesAsync(IReadOnlyList<string> symbols, CancellationToken ct = default)
    {
        var joined = string.Join(",", symbols.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim().ToUpperInvariant()).Distinct());
        if (string.IsNullOrWhiteSpace(joined)) return [];
        var call = await GetAsync($"market/quotes?tickers={Uri.EscapeDataString(joined)}", ct);
        return call.Ok ? TcbsMapper.ReadQuotes(call.Json) : [];
    }

    public async Task<TcbsOrderPreview?> PreviewOrderAsync(object body, CancellationToken ct = default)
    {
        var call = await SendAsync(HttpMethod.Post, "orders/preview", body, ct: ct);
        return call.Ok ? TcbsMapper.ReadPreview(call.Json) : null;
    }

    public Task<TcbsCall> PlaceOrderAsync(object body, string idempotencyKey, CancellationToken ct = default) =>
        SendAsync(HttpMethod.Post, "orders", body, idempotencyKey, ct);

    public async Task<TcbsWsTicket?> CreateStreamTicketAsync(string stream, string symbol, CancellationToken ct = default)
    {
        var call = await SendAsync(HttpMethod.Post, "ws-ticket", new { stream, symbol }, ct: ct);
        return call.Ok ? TcbsMapper.ReadTicket(call.Json) : null;
    }

    public async Task<TcbsCall> SendAsync(HttpMethod method, string pathAndQuery, object? body, string? idempotencyKey = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            return Fail(0, "BrokerApi:BaseUrl trống.");

        var token = await _auth.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            return Fail(401, "Chưa đăng nhập Broker.");

        try
        {
            using var request = new HttpRequestMessage(method, $"{BaseUrl}/api/tcbs/{pathAndQuery.TrimStart('/')}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
                request.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);
            if (body is not null)
                request.Content = JsonContent.Create(body);

            using var resp = await _http.SendAsync(request, ct);
            var text = await resp.Content.ReadAsStringAsync(ct);
            if ((int)resp.StatusCode == 409 && text.Contains("TCBS_REAUTH", StringComparison.OrdinalIgnoreCase))
            {
                _status = null;
                return new TcbsCall { NeedsReauth = true, Status = 409, Error = "Phiên TCBS hết hạn. Nhập lại iOTP.", Json = text };
            }

            if (!resp.IsSuccessStatusCode)
                return Fail((int)resp.StatusCode, TcbsMapper.ReadError(text));

            return new TcbsCall { Ok = true, Status = (int)resp.StatusCode, Json = text };
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "TCBS {Method} {Path} failed", method, pathAndQuery);
            return Fail(0, ex.Message);
        }
    }

    private static TcbsCall Fail(int status, string error) => new() { Status = status, Error = error };
}
