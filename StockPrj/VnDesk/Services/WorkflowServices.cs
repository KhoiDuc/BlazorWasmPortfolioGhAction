using VnDesk.Models;

namespace VnDesk.Services;

public sealed class ChecklistService
{
    public PreTradeChecklist Last { get; private set; } = new();

    public void Set(PreTradeChecklist c) => Last = c;
}

public sealed class TradePlanService
{
    private readonly string _path;
    public PlanStore Store { get; }

    public TradePlanService(AppConfig cfg)
    {
        _path = cfg.DataPath("plans.json");
        Store = JsonStore.LoadOr(_path, new PlanStore());
    }

    public void Add(TradePlan plan)
    {
        Store.Plans.Insert(0, plan);
        JsonStore.Save(_path, Store);
    }
}

public sealed class JournalService
{
    private readonly string _path;
    public JournalStore Store { get; }

    public JournalService(AppConfig cfg)
    {
        _path = cfg.DataPath("journal.json");
        Store = JsonStore.LoadOr(_path, new JournalStore());
    }

    public void AddSession(SessionLog log)
    {
        Store.Sessions.Insert(0, log);
        JsonStore.Save(_path, Store);
    }

    public void AddReview(TradeReview review)
    {
        Store.Reviews.Insert(0, review);
        JsonStore.Save(_path, Store);
    }

    public void AddAlert(AlertLog alert)
    {
        Store.Sessions.Insert(0, new SessionLog
        {
            At = alert.At,
            Notes = $"ALERT {alert.Symbol} confirm={alert.Confirmed} action={alert.Action} signal={alert.Signal} | {alert.ChecklistNote}"
        });
        JsonStore.Save(_path, Store);
    }

    public List<SessionLog> LastDays(int days = 7) =>
        Store.Sessions.Where(s => s.At >= DateTime.Now.AddDays(-days)).ToList();
}

public static class PromptLibrary
{
    public static string Wrap(string role, string task, string context, string outputFormat, string verification) =>
        $"""
        Context: {context}
        Role: {role}
        Task: {task}
        Output format: {outputFormat}
        Verification: {verification}
        Dùng ngôn ngữ trung lập. Không dự đoán chắc chắn. Không khuyến mua/bán. Người dùng tự quyết định.
        """;

    public static string News(string raw) => Wrap(
        "Trợ lý nghiên cứu thị trường VN, không ra tín hiệu giao dịch",
        "Tách văn bản tin thành Facts / Sources / Implications",
        raw,
        "Ba mục: FACTS, SOURCES, IMPLICATIONS. Ngắn gọn.",
        "Gắn 'chưa verify nguồn'. Không thêm tin không có trong input.");

    public static string Thesis(string symbol, string numbers, string notes) => Wrap(
        "Người soạn thesis, không thay thế phán đoán",
        $"Viết nhập thesis cho {symbol}: assumptions, evidence, invalidation",
        $"Số liệu app:\n{numbers}\nGhi chú:\n{notes}",
        "ASSUMPTIONS / EVIDENCE / INVALIDATION",
        "Mỗi claim phải gắn số liệu nguồn. Không thêm tín hiệu mua/bán.");

    public static string NeutralChecklist(string symbol, string numbers, string notes) => Wrap(
        "Người mô tả setup kỹ thuật trung lập",
        $"Mô tả observation vs interpretation vs invalidation cho {symbol}",
        $"{numbers}\n{notes}",
        "CONTEXT / CONFIRM-INVALIDATE / RISK / VERIFY",
        "Không hype, không chắc chắn, không ra lệnh.");

    public static string StructurePlan(string symbol, string draft) => Wrap(
        "Biên tập kế hoạch giao dịch",
        $"Cấu trúc lại chữ cho {symbol} thành Entry / Exit / Risk / Thesis / Invalidation. Không thêm tín hiệu mới.",
        draft,
        "ENTRY LOGIC / EXIT CRITERIA / RISK PARAMETERS / THESIS SUMMARY / INVALIDATION CONDITIONS",
        "Chỉ sắp xếp lại. Không thêm mục tiêu giá không có trong input.");

    public static string Journal(string symbol, string notes) => Wrap(
        "Trợ lý journal",
        $"Soạn nhập post-trade review cho {symbol}",
        notes,
        "FACTS / JUDGMENT / NEXT PROCESS",
        "Người dùng sẽ sửa trước khi lưu. Không khen/chê cảm xúc.");
}