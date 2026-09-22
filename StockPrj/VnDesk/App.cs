using System.Text;
using Spectre.Console;
using VnDesk.Clients;
using VnDesk.Models;
using VnDesk.Render;
using VnDesk.Services;

namespace VnDesk;

public sealed class App
{
    private readonly AppConfig _cfg;
    private readonly VnDirectClient _vnd;
    private readonly CafeFClient _cafef;
    private readonly IntradayClient _intraday;
    private readonly OllamaClient _ollama;
    private readonly IndicatorService _ind = new();
    private readonly SectorService _sectors;
    private readonly ScreenerService _screener;
    private readonly PositionService _position = new();
    private readonly WatchlistService _watch;
    private readonly ChecklistService _check = new();
    private readonly TradePlanService _plans;
    private readonly JournalService _journal;

    private MarketSnapshot? _market;
    private Dictionary<string, SectorAnalysis>? _sectorCache;
    private TechnicalIndicators? _lastTa;
    private List<PotentialStock> _lastScreen = [];
    private SizeResult? _lastSize;
    private TradePlan? _lastPlan;

    public App(AppConfig cfg)
    {
        _cfg = cfg;
        _vnd = new VnDirectClient(cfg);
        _cafef = new CafeFClient(cfg);
        _intraday = new IntradayClient(cfg);
        _ollama = new OllamaClient(cfg);
        _sectors = new SectorService(cfg);
        _screener = new ScreenerService(cfg);
        _watch = new WatchlistService(cfg);
        _plans = new TradePlanService(cfg);
        _journal = new JournalService(cfg);
    }

    public async Task RunAsync()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        while (true)
        {
            AnsiConsole.Clear();
            Ui.Header("VnDesk — thi truong VN");
            Ui.Info("Gold / quoc te: sau. AI soan thao, nguoi quyet dinh. Khong tu dat lenh.");
            Ui.MenuLines(
                ("A", "Thi truong     m, market"),
                ("B", "Phan tich      t, ta"),
                ("C", "Loc            s, screen"),
                ("D", "Quyet dinh     d, plan"),
                ("E", "Nhat ky        j, journal"),
                ("F", "AI             ai"),
                ("0", "Chuoi ngay (A -> watchlist -> risk -> plan)"),
                ("Q", "Thoat          exit, quit"));

            var c = ConsoleInput.ReadKeyChoice("Chon:").ToLowerInvariant();
            if (ConsoleInput.IsQuit(c) || c is "q" or "exit" or "quit")
                break;

            try
            {
                switch (c)
                {
                    case "a" or "m" or "market": await MarketMenuAsync(); break;
                    case "b" or "t" or "ta": await AnalysisMenuAsync(); break;
                    case "c" or "s" or "screen": await ScreenMenuAsync(); break;
                    case "d" or "plan": await DecisionMenuAsync(); break;
                    case "e" or "j" or "journal": await JournalMenuAsync(); break;
                    case "f" or "ai": await AiMenuAsync(); break;
                    case "0": await DailySequenceAsync(); break;
                    default: Ui.Warn("Khong hieu. A-F, 0, Q."); ConsoleInput.Pause(); break;
                }
            }
            catch (Exception ex)
            {
                Ui.Error(ex.Message);
                ConsoleInput.Pause();
            }
        }
    }

    private async Task MarketMenuAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();
            Ui.Header("A — Thi truong");
            Ui.MenuLines(("1", "Tong quan thi truong"), ("2", "Heatmap nganh"), ("Q", "Back"));
            var c = ConsoleInput.ReadKeyChoice("Chon:").ToLowerInvariant();
            if (ConsoleInput.IsBack(c)) return;
            if (c is "1") await ToolMarketOverviewAsync();
            else if (c is "2") await ToolSectorAsync();
        }
    }

    private async Task AnalysisMenuAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();
            Ui.Header("B — Phan tich");
            Ui.MenuLines(("1", "Phan tich ma"), ("2", "Checklist ky thuat"), ("3", "P&L vi the"), ("Q", "Back"));
            var c = ConsoleInput.ReadKeyChoice("Chon:").ToLowerInvariant();
            if (ConsoleInput.IsBack(c)) return;
            if (c is "1") await ToolAnalyzeAsync();
            else if (c is "2") await ToolTechChecklistAsync();
            else if (c is "3") await ToolPnlAsync();
        }
    }

    private async Task ScreenMenuAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();
            Ui.Header("C — Loc");
            Ui.MenuLines(("1", "Screener tiem nang"), ("2", "Watchlist cham diem"), ("Q", "Back"));
            var c = ConsoleInput.ReadKeyChoice("Chon:").ToLowerInvariant();
            if (ConsoleInput.IsBack(c)) return;
            if (c is "1") await ToolScreenerAsync();
            else if (c is "2") await ToolWatchlistAsync();
        }
    }

    private async Task DecisionMenuAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();
            Ui.Header("D — Quyet dinh");
            Ui.MenuLines(
                ("1", "Pre-trade checklist"),
                ("2", "Trading plan"),
                ("3", "Position size"),
                ("4", "Alert workflow"),
                ("Q", "Back"));
            var c = ConsoleInput.ReadKeyChoice("Chon:").ToLowerInvariant();
            if (ConsoleInput.IsBack(c)) return;
            if (c is "1") await ToolPreTradeAsync();
            else if (c is "2") await ToolPlanAsync();
            else if (c is "3") ToolSize();
            else if (c is "4") await ToolAlertAsync();
        }
    }

    private async Task JournalMenuAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();
            Ui.Header("E — Nhat ky");
            Ui.MenuLines(("1", "Session log"), ("2", "Post-trade review"), ("Q", "Back"));
            var c = ConsoleInput.ReadKeyChoice("Chon:").ToLowerInvariant();
            if (ConsoleInput.IsBack(c)) return;
            if (c is "1") await ToolSessionAsync();
            else if (c is "2") await ToolReviewAsync();
        }
    }

    private async Task AiMenuAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();
            Ui.Header("F — AI (Ollama local, fail-soft)");
            Ui.MenuLines(("1", "Dan tin -> Facts/Sources/Implications"), ("2", "Draft thesis"), ("Q", "Back"));
            var c = ConsoleInput.ReadKeyChoice("Chon:").ToLowerInvariant();
            if (ConsoleInput.IsBack(c)) return;
            if (c is "1") await ToolNewsAsync();
            else if (c is "2") await ToolThesisAsync();
        }
    }

    private async Task DailySequenceAsync()
    {
        Ui.Header("Chuoi ngay");
        await ToolMarketOverviewAsync();
        await ToolWatchlistAsync();
        ToolSize();
        await ToolPlanAsync();
    }

    // --- A1 ---
    private async Task ToolMarketOverviewAsync()
    {
        Ui.Header("A1 Tong quan");
        Ui.Info("Enter = cache | r = reload | json = dump | q = back");
        var cmd = ConsoleInput.ReadLine("Lenh:").ToLowerInvariant();
        if (ConsoleInput.IsBack(cmd)) return;

        if (cmd is "r" or "reload" || _market is null)
        {
            await AnsiConsole.Status().StartAsync("Tai CafeF + VNDirect...", async _ =>
            {
                var indices = await _cafef.FetchIndicesAsync();
                var symbols = _sectors.AllSymbols.Count > 0 ? _sectors.AllSymbols : await _cafef.FetchSymbolsAsync();
                var take = symbols.Take(400).ToList();
                var stocks = await _vnd.GetLatestManyAsync(take);
                _market = new MarketSnapshot { Indices = indices, Stocks = stocks, LoadedAt = DateTime.Now };
            });
        }

        if (_market is null) { Ui.Warn("Khong tai duoc."); ConsoleInput.Pause(); return; }
        MarketRenderer.Indices(_market.Indices);
        MarketRenderer.Movers(_market.Stocks);
        Ui.Info($"Cache {_market.LoadedAt:HH:mm:ss}");
        if (cmd is "json") Ui.JsonPanel("snapshot", new { _market.LoadedAt, indices = _market.Indices.Count, stocks = _market.Stocks.Count });
        ConsoleInput.Pause();
    }

    // --- A2 ---
    private async Task ToolSectorAsync()
    {
        Ui.Header("A2 Heatmap nganh");
        var input = ConsoleInput.ReadLine("Enter = tat ca | ten nganh | ma CK | reload:");
        if (ConsoleInput.IsBack(input)) return;

        if (input.Equals("reload", StringComparison.OrdinalIgnoreCase) || _sectorCache is null)
        {
            _vnd.ClearCache();
            var symbols = _sectors.AllSymbols;
            List<StockData> stocks = [];
            await AnsiConsole.Progress().StartAsync(async ctx =>
            {
                var task = ctx.AddTask("Tai nganh", maxValue: Math.Max(1, symbols.Count));
                var prog = new Progress<int>(n => task.Value = n);
                stocks = await _vnd.GetLatestManyAsync(symbols, prog);
            });
            _sectorCache = _sectors.Analyze(stocks);
            if (_market is not null) _market.Stocks = stocks;
        }

        string? focus = null;
        if (!string.IsNullOrWhiteSpace(input) && !input.Equals("reload", StringComparison.OrdinalIgnoreCase))
        {
            var sector = _sectors.Map.Keys.FirstOrDefault(k => k.Contains(input, StringComparison.OrdinalIgnoreCase));
            if (sector is null)
            {
                var found = _sectors.FindSector(input);
                if (found.Length > 0) { focus = found; Ui.Ok($"{input.ToUpperInvariant()} thuoc {found}"); }
                else Ui.Warn("Khong tim thay nganh / ma.");
            }
            else focus = sector;
        }

        if (_sectorCache is not null)
            MarketRenderer.Sectors(_sectorCache, focus);
        ConsoleInput.Pause();
    }

    // --- B1 ---
    private async Task<TechnicalIndicators?> AnalyzeSymbolAsync(string symbol, bool wantIntraday)
    {
        symbol = symbol.ToUpperInvariant();
        var hist = await _vnd.GetHistoricalAsync(symbol, _cfg.HistorySessions);
        var ta = _ind.Calculate(symbol, hist);
        if (ta is null)
        {
            Ui.Warn($"Khong du du lieu TA cho {symbol} (can ~200 phien). Co {hist.Count}.");
            return null;
        }
        if (wantIntraday)
            _ind.ApplyIntraday(ta, _intraday.Fetch(symbol));
        _lastTa = ta;
        return ta;
    }

    private async Task ToolAnalyzeAsync()
    {
        Ui.Header("B1 Phan tich ma");
        var raw = ConsoleInput.ReadMultiline("Ma (1 dong hoac nhieu ma). Them dong 'intraday' / 'json' neu can.");
        if (string.IsNullOrWhiteSpace(raw)) return;

        var wantIntra = raw.Contains("intraday", StringComparison.OrdinalIgnoreCase);
        var wantJson = raw.Contains("json", StringComparison.OrdinalIgnoreCase);
        var symbols = ConsoleInput.ParseSymbols(raw.Replace("intraday", "", StringComparison.OrdinalIgnoreCase).Replace("json", "", StringComparison.OrdinalIgnoreCase));
        if (symbols.Count == 0)
        {
            var one = ConsoleInput.ReadSymbol();
            if (ConsoleInput.IsBack(one) || one.Length == 0) return;
            symbols = [one];
        }

        foreach (var sym in symbols.Take(8))
        {
            var ta = await AnalyzeSymbolAsync(sym, wantIntra);
            if (ta is not null)
            {
                AnalysisRenderer.Technical(ta);
                if (wantJson) Ui.JsonPanel(sym, CompactTa(ta));
            }
        }
        ConsoleInput.Pause();
    }

    // --- B2 ---
    private async Task ToolTechChecklistAsync()
    {
        Ui.Header("B2 Checklist ky thuat");
        var symbol = ConsoleInput.ReadSymbol();
        if (ConsoleInput.IsBack(symbol) || symbol.Length == 0) return;
        var notes = ConsoleInput.ReadMultiline("Ghi chu chart (optional):");
        var ta = _lastTa?.Symbol == symbol ? _lastTa : await AnalyzeSymbolAsync(symbol, false);
        if (ta is null) { ConsoleInput.Pause(); return; }
        var cl = _ind.BuildTechnicalChecklist(ta, notes);
        AnalysisRenderer.Checklist(cl);

        var f = ConsoleInput.ReadLine("[F] Ollama nhap trung lap | json | Enter:").ToLowerInvariant();
        if (f is "f")
        {
            var numbers = TaSummary(ta);
            var (ok, text) = await _ollama.GenerateAsync(PromptLibrary.NeutralChecklist(symbol, numbers, notes));
            if (!ok) Ui.Warn($"Ollama skip: {text}");
            else Ui.Panel("AI nhap (chua verify)", text);
        }
        if (f is "json") Ui.JsonPanel("checklist", cl);
        ConsoleInput.Pause();
    }

    // --- B3 ---
    private async Task ToolPnlAsync()
    {
        Ui.Header("B3 P&L vi the");
        Ui.Info("Don vi gia trung voi VNDirect (vd 27.35), khong nhan 1000.");
        var symbol = ConsoleInput.ReadSymbol();
        if (ConsoleInput.IsBack(symbol) || symbol.Length == 0) return;
        if (!decimal.TryParse(ConsoleInput.ReadLine("Gia mua:"), out var entry)) { Ui.Warn("Gia khong hop le."); ConsoleInput.Pause(); return; }
        if (!int.TryParse(ConsoleInput.ReadLine("So CP:"), out var shares)) { Ui.Warn("KL khong hop le."); ConsoleInput.Pause(); return; }
        Ui.MenuLines(("A", "VPS 0.15%"), ("B", "SSI 0.15%"), ("C", "TCBS 0.10%"), ("D", "MBS 0.12%"));
        var broker = ConsoleInput.ReadLine("Cong ty CK A/B/C/D:");
        if (ConsoleInput.IsBack(broker)) return;

        decimal current = _lastTa?.Symbol == symbol ? _lastTa.LatestClose : 0;
        if (current == 0)
        {
            var ta = await AnalyzeSymbolAsync(symbol, false);
            current = ta?.LatestClose ?? 0;
        }
        if (current == 0 && !decimal.TryParse(ConsoleInput.ReadLine("Gia hien tai:"), out current))
        {
            Ui.Warn("Khong co gia.");
            ConsoleInput.Pause();
            return;
        }

        var result = _position.Calculate(symbol, entry, shares, current, broker);
        AnalysisRenderer.Position(result);
        if (ConsoleInput.ReadLine("json?").Equals("json", StringComparison.OrdinalIgnoreCase))
            Ui.JsonPanel("pnl", result);
        ConsoleInput.Pause();
    }

    // --- C1 ---
    private async Task ToolScreenerAsync()
    {
        Ui.Header("C1 Screener");
        Ui.MenuLines(("1", "VN30"), ("2", "HOSE"), ("3", "HNX30"), ("4", "UPCOM"), ("5", "Ngan hang"), ("6", "Dan / clip"));
        var pick = ConsoleInput.ReadLine("List:");
        if (ConsoleInput.IsBack(pick)) return;

        List<string> symbols = pick switch
        {
            "1" => _screener.Lists.GetValueOrDefault("VN30") ?? [],
            "2" => _screener.Lists.GetValueOrDefault("HOSE") ?? [],
            "3" => _screener.Lists.GetValueOrDefault("HNX30") ?? [],
            "4" => _screener.Lists.GetValueOrDefault("UPCOM") ?? [],
            "5" => _screener.Lists.GetValueOrDefault("NganHang") ?? [],
            "6" => ConsoleInput.ParseSymbols(ConsoleInput.ReadMultiline("Dan ma:")),
            _ => []
        };
        if (symbols.Count == 0) { Ui.Warn("List rong."); ConsoleInput.Pause(); return; }

        var histAll = new List<StockData>();
        await AnsiConsole.Progress().StartAsync(async ctx =>
        {
            var task = ctx.AddTask("Scan", maxValue: symbols.Count);
            foreach (var s in symbols)
            {
                var h = await _vnd.GetHistoricalAsync(s, _cfg.ScreenerDays);
                histAll.AddRange(h);
                task.Increment(1);
            }
        });

        _lastScreen = _screener.Scan(histAll);
        ScreenerRenderer.Results(_lastScreen);

        var extra = ConsoleInput.ReadLine("[d] drill | json | Enter:").ToLowerInvariant();
        if (extra is "json") Ui.JsonPanel("screener", _lastScreen.Take(20));
        if (extra is "d")
        {
            var sym = ConsoleInput.ReadSymbol("Ma drill");
            if (!ConsoleInput.IsBack(sym) && sym.Length > 0)
            {
                var ta = await AnalyzeSymbolAsync(sym, false);
                if (ta is not null) AnalysisRenderer.Technical(ta);
            }
        }
        ConsoleInput.Pause();
    }

    // --- C2 ---
    private async Task ToolWatchlistAsync()
    {
        Ui.Header("C2 Watchlist");
        var edit = ConsoleInput.ReadLine("Sua tieu chi? y / Enter giu mac dinh:");
        if (ConsoleInput.IsBack(edit)) return;
        if (edit.Equals("y", StringComparison.OrdinalIgnoreCase))
        {
            var text = ConsoleInput.ReadMultiline("Moi tieu chi 1 dong, toi da 5:");
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Take(5).ToList();
            if (lines.Count > 0) _watch.State.Criteria = lines;
        }

        var raw = ConsoleInput.ReadMultiline("Dan ma (toi da ~10):");
        if (ConsoleInput.IsBack(raw)) return;
        var symbols = ConsoleInput.ParseSymbols(raw).Take(10).ToList();
        if (symbols.Count == 0)
        {
            symbols = _screener.Lists.GetValueOrDefault("VN30")?.Take(10).ToList() ?? [];
            Ui.Info("Dung 10 ma VN30.");
        }

        var scores = new List<WatchlistScore>();
        foreach (var s in symbols)
        {
            var ta = await AnalyzeSymbolAsync(s, false);
            if (ta is not null) scores.Add(_watch.Score(ta));
        }
        _watch.State.LastScores = scores;
        _watch.State.UpdatedAt = DateTime.Now;
        _watch.Save();
        ScreenerRenderer.Watchlist(scores, _watch.State.Criteria);
        ConsoleInput.Pause();
    }

    // --- D1 ---
    private async Task ToolPreTradeAsync()
    {
        Ui.Header("D1 Pre-trade checklist");
        var symbol = ConsoleInput.ReadSymbol();
        if (ConsoleInput.IsBack(symbol) || symbol.Length == 0) return;
        var c = new PreTradeChecklist { Symbol = symbol };
        c.Thesis = ConsoleInput.ReadMultiline("1 Thesis 1 cau:");
        c.SourceData = ConsoleInput.ReadMultiline("2 Nguon du lieu:");
        c.Invalidation = ConsoleInput.ReadMultiline("3 Dieu gi invalidate:");
        c.MaxLoss = ConsoleInput.ReadMultiline("4 Max loss:");
        c.PositionSizeLogic = ConsoleInput.ReadMultiline("5 Position size logic:");
        if (_lastSize is not null && string.IsNullOrWhiteSpace(c.PositionSizeLogic))
            c.PositionSizeLogic = $"Size goi y {_lastSize.Shares} CP, R:R {_lastSize.RiskReward:N2}";
        c.StopConditions = ConsoleInput.ReadMultiline("6 Stop:");
        c.ScenarioChange = ConsoleInput.ReadMultiline("7 Scenario doi plan:");
        c.WithoutPressure = ConsoleInput.ReadMultiline("8 Van lam neu khong ap luc?:");
        _check.Set(c);
        WorkflowRenderer.Checklist(c);

        var cr = ConsoleInput.ConfirmCr();
        if (cr is true)
        {
            c.Confirmed = true;
            _journal.AddAlert(new AlertLog { Symbol = symbol, Signal = "pre-trade", Confirmed = true, Action = "logged", ChecklistNote = c.Thesis });
            Ui.Ok("Da ghi journal. Khong gui lenh.");
        }
        else if (cr is false)
        {
            _journal.AddAlert(new AlertLog { Symbol = symbol, Signal = "pre-trade", Confirmed = false, Action = "reject", ChecklistNote = c.Thesis });
            Ui.Warn("Reject. Khong vao lenh.");
        }
        await Task.CompletedTask;
        ConsoleInput.Pause();
    }

    // --- D2 ---
    private async Task ToolPlanAsync()
    {
        Ui.Header("D2 Trading plan");
        var symbol = ConsoleInput.ReadSymbol();
        if (ConsoleInput.IsBack(symbol) || symbol.Length == 0) return;
        var p = new TradePlan { Symbol = symbol };
        p.EntryLogic = ConsoleInput.ReadMultiline("ENTRY LOGIC:");
        p.ExitCriteria = ConsoleInput.ReadMultiline("EXIT CRITERIA:");
        p.RiskParameters = ConsoleInput.ReadMultiline("RISK PARAMETERS:");
        if (_lastSize is not null && string.IsNullOrWhiteSpace(p.RiskParameters))
            p.RiskParameters = $"Max loss {_lastSize.RiskAmount:N0}; size {_lastSize.Shares}; stop {_lastSize.Stop:N2}";
        p.ThesisSummary = ConsoleInput.ReadMultiline("THESIS SUMMARY:");
        p.InvalidationConditions = ConsoleInput.ReadMultiline("INVALIDATION:");
        _lastPlan = p;
        WorkflowRenderer.Plan(p);
        Ui.JsonPanel("plan", p);

        var extra = ConsoleInput.ReadLine("Luu? y | [F] Ollama cau truc lai | Enter:").ToLowerInvariant();
        if (extra is "f")
        {
            var draft = $"{p.EntryLogic}\n{p.ExitCriteria}\n{p.RiskParameters}\n{p.ThesisSummary}\n{p.InvalidationConditions}";
            var (ok, text) = await _ollama.GenerateAsync(PromptLibrary.StructurePlan(symbol, draft));
            if (!ok) Ui.Warn($"Ollama skip: {text}");
            else Ui.Panel("AI cau truc (chua luu)", text);
        }
        if (extra is "y" or "f")
        {
            if (extra is "y") { _plans.Add(p); Ui.Ok("Da luu plans.json"); }
        }
        ConsoleInput.Pause();
    }

    // --- D3 ---
    private void ToolSize()
    {
        Ui.Header("D3 Position size");
        if (!decimal.TryParse(ConsoleInput.ReadLine("Von (VND):"), out var cap)) { Ui.Warn("So khong hop le."); ConsoleInput.Pause(); return; }
        if (!decimal.TryParse(ConsoleInput.ReadLine("% risk:"), out var risk)) { Ui.Warn("So khong hop le."); ConsoleInput.Pause(); return; }
        if (!decimal.TryParse(ConsoleInput.ReadLine("Gia:"), out var price)) { Ui.Warn("So khong hop le."); ConsoleInput.Pause(); return; }
        if (!decimal.TryParse(ConsoleInput.ReadLine("Stop:"), out var stop)) { Ui.Warn("So khong hop le."); ConsoleInput.Pause(); return; }
        decimal? tp = decimal.TryParse(ConsoleInput.ReadLine("Target (Enter bo qua):"), out var t) ? t : null;
        _lastSize = _position.Size(cap, risk, price, stop, tp);
        AnalysisRenderer.Size(_lastSize);
        ConsoleInput.Pause();
    }

    // --- D4 ---
    private async Task ToolAlertAsync()
    {
        Ui.Header("D4 Alert workflow");
        var signal = _lastTa is not null
            ? $"{_lastTa.Symbol} {_lastTa.TradingSignal.Action} @ {_lastTa.TradingSignal.EntryPrice:N2}"
            : (_lastScreen.FirstOrDefault() is { } p ? $"{p.Symbol} score {p.PotentialScore:N1}" : "");
        if (string.IsNullOrEmpty(signal))
            signal = ConsoleInput.ReadMultiline("Mo ta signal:");
        if (ConsoleInput.IsBack(signal)) return;

        WorkflowRenderer.AlertSteps(signal);
        if (_check.Last.FilledCount > 0)
            WorkflowRenderer.Checklist(_check.Last);

        var cr = ConsoleInput.ConfirmCr();
        if (cr is null) { ConsoleInput.Pause(); return; }
        _journal.AddAlert(new AlertLog
        {
            Symbol = _lastTa?.Symbol ?? _check.Last.Symbol,
            Signal = signal,
            Confirmed = cr.Value,
            Action = cr.Value ? "logged-no-order" : "reject",
            ChecklistNote = _check.Last.Thesis
        });
        Ui.Ok(cr.Value ? "Logged. Khong khop lenh." : "Reject. Logged.");
        await Task.CompletedTask;
        ConsoleInput.Pause();
    }

    // --- E1 ---
    private async Task ToolSessionAsync()
    {
        Ui.Header("E1 Session log");
        var log = new SessionLog
        {
            Calendar = ConsoleInput.ReadMultiline("Market calendar:"),
            WatchlistFlags = ConsoleInput.ReadMultiline("Watchlist flags:"),
            Risks = ConsoleInput.ReadMultiline("Map risks:"),
            SessionPlan = ConsoleInput.ReadMultiline("Plan phien:"),
            StopTime = ConsoleInput.ReadLine("Gio dung:"),
            Notes = ConsoleInput.ReadMultiline("Notes them:")
        };
        if (ConsoleInput.IsBack(log.Calendar) && string.IsNullOrWhiteSpace(log.SessionPlan)) return;
        _journal.AddSession(log);
        WorkflowRenderer.Session(log);
        Ui.JsonPanel("session", log);
        Ui.SubHeader("7 ngay gan nhat");
        foreach (var s in _journal.LastDays())
            AnsiConsole.MarkupLine($"[grey]{s.At:dd/MM HH:mm}[/] {Ui.E(s.SessionPlan.Length > 60 ? s.SessionPlan[..60] : s.SessionPlan)}");
        await Task.CompletedTask;
        ConsoleInput.Pause();
    }

    // --- E2 ---
    private async Task ToolReviewAsync()
    {
        Ui.Header("E2 Post-trade review");
        var symbol = ConsoleInput.ReadSymbol();
        if (ConsoleInput.IsBack(symbol) || symbol.Length == 0) return;
        var happened = ConsoleInput.ReadMultiline("Dieu gi xay ra:");
        var learned = ConsoleInput.ReadMultiline("Hoc duoc:");
        var process = ConsoleInput.ReadMultiline("Sua process:");

        var extra = ConsoleInput.ReadLine("[F] Ollama soan nhap | Enter luu:").ToLowerInvariant();
        string? ai = null;
        if (extra is "f")
        {
            var (ok, text) = await _ollama.GenerateAsync(PromptLibrary.Journal(symbol, $"{happened}\n{learned}\n{process}"));
            if (!ok) Ui.Warn($"Ollama skip: {text}");
            else
            {
                ai = text;
                Ui.Panel("AI nhap — sua truoc khi luu", text);
                var edited = ConsoleInput.ReadMultiline("Sua (Enter giu AI, hoac dan lai):");
                if (!string.IsNullOrWhiteSpace(edited)) process = edited;
            }
        }

        var r = new TradeReview { Symbol = symbol, WhatHappened = happened, Learned = learned, ProcessChange = process, AiDraft = ai };
        _journal.AddReview(r);
        WorkflowRenderer.Review(r);
        ConsoleInput.Pause();
    }

    // --- F1 ---
    private async Task ToolNewsAsync()
    {
        Ui.Header("F1 News brief");
        var raw = ConsoleInput.ReadMultiline("Dan tin (khong fetch URL):");
        if (string.IsNullOrWhiteSpace(raw)) return;
        var (ok, text) = await _ollama.GenerateAsync(PromptLibrary.News(raw));
        var brief = new NewsBrief { Raw = raw };
        if (!ok)
        {
            Ui.Warn($"Ollama skip: {text}");
            brief.Facts = raw;
            brief.Sources = "(chua tach — Ollama down)";
            brief.Implications = "";
        }
        else ParseNews(text, brief);
        WorkflowRenderer.News(brief);
        var v = ConsoleInput.ReadLine("[v] da doi chieu nguon:").ToLowerInvariant();
        if (v is "v") { brief.Verified = true; WorkflowRenderer.News(brief); }
        ConsoleInput.Pause();
    }

    // --- F2 ---
    private async Task ToolThesisAsync()
    {
        Ui.Header("F2 Draft thesis");
        var symbol = ConsoleInput.ReadSymbol();
        if (ConsoleInput.IsBack(symbol) || symbol.Length == 0) return;
        var notes = ConsoleInput.ReadMultiline("Ghi chu:");
        var ta = _lastTa?.Symbol == symbol ? _lastTa : await AnalyzeSymbolAsync(symbol, false);
        var numbers = ta is null ? "(khong co chi bao)" : TaSummary(ta);
        if (ta is not null)
        {
            var t = new Table().Border(TableBorder.Simple);
            t.AddColumn("Nguon");
            t.AddColumn("Gia tri");
            t.AddRow("Close", $"{ta.LatestClose:N2}");
            t.AddRow("Trend", Ui.E(ta.Trend));
            t.AddRow("RSI", $"{ta.RSI:N1}");
            t.AddRow("Signal", Ui.E(ta.TradingSignal.Action));
            AnsiConsole.Write(t);
        }
        var (ok, text) = await _ollama.GenerateAsync(PromptLibrary.Thesis(symbol, numbers, notes));
        if (!ok) Ui.Warn($"Ollama skip: {text}");
        else Ui.Panel($"Thesis nhap {symbol}", text);

        var copy = ConsoleInput.ReadLine("Copy sang D2? y:").ToLowerInvariant();
        if (copy is "y" && ok)
        {
            _lastPlan = new TradePlan { Symbol = symbol, ThesisSummary = text };
            Ui.Ok("Da gan thesis vao plan tam. Mo D2 de dien tiep.");
        }
        ConsoleInput.Pause();
    }

    private static void ParseNews(string text, NewsBrief brief)
    {
        brief.Facts = Slice(text, "FACTS", "SOURCES");
        brief.Sources = Slice(text, "SOURCES", "IMPLICATIONS");
        brief.Implications = Slice(text, "IMPLICATIONS", null);
        if (string.IsNullOrWhiteSpace(brief.Facts)) brief.Facts = text;
    }

    private static string Slice(string text, string from, string? until)
    {
        var i = text.IndexOf(from, StringComparison.OrdinalIgnoreCase);
        if (i < 0) return "";
        i += from.Length;
        var j = until is null ? text.Length : text.IndexOf(until, i, StringComparison.OrdinalIgnoreCase);
        if (j < 0) j = text.Length;
        return text[i..j].Trim().TrimStart(':', '-', '\n', '\r', ' ');
    }

    private static string TaSummary(TechnicalIndicators ta) =>
        $"{ta.Symbol} close={ta.LatestClose:N2} trend={ta.Trend} RSI={ta.RSI:N1} SMA20={ta.SMA20:N2} SMA50={ta.SMA50:N2} SMA200={ta.SMA200:N2} MACD={ta.MACD:N3} ATR%={ta.ATR:N2} volx={ta.VolumeRatio:N2} {ta.Divergence} signal={ta.TradingSignal.Action}";

    private static object CompactTa(TechnicalIndicators ta) => new
    {
        ta.Symbol,
        ta.LatestClose,
        ta.Trend,
        ta.RSI,
        ta.SMA20,
        ta.SMA50,
        ta.SMA200,
        ta.MACD,
        ta.ATR,
        ta.VolumeRatio,
        ta.Divergence,
        signal = ta.TradingSignal.Action,
        sl = ta.TradingSignal.StopLoss,
        tp = ta.TradingSignal.TakeProfit
    };
}
