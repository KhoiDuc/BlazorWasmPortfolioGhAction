namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public static class PromptLibrary
{
    public static string Wrap(string role, string task, string context, string outputFormat, string verification) =>
        $"""
        Context: {context}
        Role: {role}
        Task: {task}
        Output format: {outputFormat}
        Verification: {verification}
        Dung ngon ngu trung lap. Khong du doan chac chan. Khong khuyen mua/ban. Nguoi dung tu quyet dinh.
        """;

    public static string News(string raw) => Wrap(
        "Tro ly nghien cuu thi truong VN, khong ra tin hieu giao dich",
        "Tach van ban tin thanh Facts / Sources / Implications",
        raw,
        "Ba muc: FACTS, SOURCES, IMPLICATIONS. Ngan gon.",
        "Gan 'chua verify nguon'. Khong them tin khong co trong input.");

    public static string Thesis(string symbol, string numbers, string notes) => Wrap(
        "Nguoi soan thesis, khong thay the phan doan",
        $"Viet nhap thesis cho {symbol}: assumptions, evidence, invalidation",
        $"So lieu app:\n{numbers}\nGhi chu:\n{notes}",
        "ASSUMPTIONS / EVIDENCE / INVALIDATION",
        "Moi claim phai gan so lieu nguon. Khong them tin hieu mua/ban.");

    public static string NeutralChecklist(string symbol, string numbers, string notes) => Wrap(
        "Nguoi mo ta setup ky thuat trung lap",
        $"Mo ta observation vs interpretation vs invalidation cho {symbol}",
        $"{numbers}\n{notes}",
        "CONTEXT / CONFIRM-INVALIDATE / RISK / VERIFY",
        "Khong hype, khong chac chan, khong ra lenh.");

    public static string StructurePlan(string symbol, string draft) => Wrap(
        "Bien tap ke hoach giao dich",
        $"Cau truc lai chu cho {symbol} thanh Entry / Exit / Risk / Thesis / Invalidation. Khong them tin hieu moi.",
        draft,
        "ENTRY LOGIC / EXIT CRITERIA / RISK PARAMETERS / THESIS SUMMARY / INVALIDATION CONDITIONS",
        "Chi sap xep lai. Khong them muc tieu gia khong co trong input.");

    public static string Journal(string symbol, string notes) => Wrap(
        "Tro ly journal",
        $"Soan nhap post-trade review cho {symbol}",
        notes,
        "FACTS / JUDGMENT / NEXT PROCESS",
        "Nguoi dung se sua truoc khi luu. Khong khen/che cam xuc.");
}
