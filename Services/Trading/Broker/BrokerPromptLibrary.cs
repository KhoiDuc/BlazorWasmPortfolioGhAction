using System.Globalization;
using BlazorWasmPortfolioGhAction.Models.Trading.Broker;

namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

public static class BrokerPromptLibrary
{
    public static string ExplainNote(
        string symbol,
        string sector,
        string status,
        string? avgBuy,
        string? stop,
        string? target,
        string newNote,
        string priorNotes)
    {
        return $"""
            Role: Tro ly giai thich ngon ngu broker chung khoan Viet Nam. Khong khuyen mua/ban. Nguoi dung tu quyet dinh.
            Task: Giai thich note broker ve ma {symbol} bang tieng Viet de hieu.

            Context:
            - Ma: {symbol}
            - Nganh: {sector}
            - Trang thai dang ghi: {status}
            - Gia TB: {avgBuy}
            - Cat lo: {stop}
            - Muc tieu: {target}
            - Note can giai thich:
            {newNote}
            - Note cu cung ma (de doi chieu):
            {priorNotes}

            Output format (tieng Viet, ngan):
            1) JARGON — giai thich tu/cum tu kho (tich luy, breakout, PE, canh mua, ...)
            2) Y NGHIA — broker dang noi gi, khong suy dien tin khong co trong note
            3) CAU HOI — 2-4 cau nen hoi lai broker neu con mo ho
            4) MAU THUAN — neu note moi mau thuan note cu thi ghi ro; neu khong thi "Khong thay mau thuan ro"

            Verification: Khong them tin hieu mua/ban. Khong du doan chac chan. Neu note qua ngan thi noi thieu thong tin.
            """;
    }

    public static string ExplainRecommendation(string rawText)
    {
        return $"""
            Role: Tro ly phan tich khuyen nghi broker chung khoan Viet Nam. KHONG khuyen mua/ban. Chi phan tich y broker.
            Task: Nguoi dung paste nguyen van khuyen nghi cua broker. Giai thich bang tieng Viet de hieu.

            Khuyen nghi (raw):
            {rawText}

            Output format (tieng Viet, ro rang, ngan gon):
            ## Ma CP
            Ma broker de cap (neu co). Neu nhieu ma thi liet ke.

            ## Jargon
            Giai thich cac tu/cum tu chuyen nganh: tich luy, breakout, hap thu, cung cau, MA, volume, ...
            Dinh dang: <tu> — <giai thich ngan 1-2 dong>

            ## Y nghia
            Broker muon noi gi? Phan tich logic: vi sao mua, vi sao cat lo o muc do, muc tieu nao.
            KHONG suy dien tin khong co trong text. Neu broker khong noi ro thi noi "Broker khong de cap".

            ## Muc gia
            Vung mua, cat lo, muc tieu (neu co). Trinh bay bang bullet, gia tri so ro rang.

            ## Thoi diem mua
            Dua tren chu noi cua broker, nen:
            - MUA LIEN — neu broker noi "mua ngay", "mua nhuong gia hien tai", "mua o kha nang ky" hoac tuong tu.
            - DOI — neu broker noi "mua quanh", "mua vung", "canh mua o", "cho lui ve" hoac co vung gia muc tieu.
            Neu khong ro: "Broker khong noi ro thoi diem — can hoi lai."
            Chi trich dan tu goc, KHONG them y kien cua ban than.

            ## Rui ro
            Nhung rui ro broker de cap hoac ngam hieu. Neu khong co thi "Khong de cap rui ro ro".

            ## Cau hoi
            2-4 cau nguoi dung nen hoi lai broker truoc khi quyet dinh.

            Verification: KHONG them loi khuyen mua/ban cua ban than. KHONG du doan gia tuong lai. Neu text qua ngan thi noi thieu thong tin.
            """;
    }

    /// <summary>
    /// Builds a compact textual summary of the current recommendation list
    /// (active positions only) to inject as context for AI questions about the portfolio.
    /// </summary>
    public static string BuildPortfolioContext(BrokerPortfolio? portfolio)
    {
        if (portfolio?.Positions is null || portfolio.Positions.Count == 0)
            return "(Danh sach khuyen nghi hien dang trong.)";

        var lines = new List<string>();
        var i = 0;
        foreach (var p in portfolio.Positions)
        {
            i++;
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(p.Symbol)) parts.Add($"Ma: {p.Symbol}");
            if (!string.IsNullOrWhiteSpace(p.Sector)) parts.Add($"Nganh: {p.Sector}");
            parts.Add($"Trang thai: {BrokerStatusLabels.Vi(p.Status)}");
            if (p.AvgBuy is { } avg && avg > 0) parts.Add($"Gia TB: {avg.ToString("N2", CultureInfo.InvariantCulture)}");
            if (p.RemainingQuantity is { } rem && rem > 0) parts.Add($"KL con: {rem.ToString("N0", CultureInfo.InvariantCulture)}");
            if (p.StopLoss is { } sl && sl > 0) parts.Add($"Cat lo: {sl.ToString("N2", CultureInfo.InvariantCulture)}");
            if (p.TargetPrice is { } tp && tp > 0) parts.Add($"Muc tieu: {tp.ToString("N2", CultureInfo.InvariantCulture)}");
            if (p.WeightPct is { } w && w > 0) parts.Add($"Trong so: {w.ToString("N1", CultureInfo.InvariantCulture)}%");
            if (p.Tags is { } tags && tags.Count > 0) parts.Add($"Tag: {string.Join(", ", tags)}");
            lines.Add($"  {i}. {string.Join(" | ", parts)}");
        }
        return string.Join('\n', lines);
    }

    /// <summary>
    /// Prompt for answering free-form questions about the current recommendation list.
    /// Injects the full portfolio context so the user can ask things like
    /// "I have 3 bank stocks, should I merge into 1?".
    /// </summary>
    public static string ExplainWithPortfolioContext(string userQuestion, string portfolioContext)
    {
        return $"""
            Role: Tro ly phan tich danh muc khuyen nghj broker chung khoan Viet Nam. KHONG khuyen mua/ban. Chi phan tich va tra loi dua tren danh sach hien co.
            Task: Nguoi dung hoi cau hoi ve danh muc khuyen nghi hien tai. Tra loi bang tieng Viet, ro rang, dua tren duoi day.

            Danh sach khuyen nghi hien tai:
            {portfolioContext}

            Cau hoi cua nguoi dung:
            {userQuestion}

            Huong dan tra loi:
            - Tra loi dua tren danh sach tren, KHONG suy dien ma khong co.
            - Neu cau hoi ve gop/xen/giam position: phan tich trung lap nganh, trong so, rui ro tap trung, muc tieu/cat lo trung nhau.
            - Neu cau hoi ve 1 ma cu the: dung thong tin cua ma do trong danh sach.
            - Neu cau hoi ve nganh/phan bo: nhom theo nganh, tinh tong trong so, nhan xet tap trung.
            - Neu thieu thong tin de tra loi: noi ro dieu gi thieu, KHONG bao loi khuyen.
            - Neu danh sach trong: bao "Danh sach hien dang trong, them khuyen nghi truoc khi hoi."

            Output format (tieng Viet, ngan gon, dung markdown):
            ## Tra loi
            Tra loi truc tiep cau hoi.

            ## Phan tich
            Phan tich chi tiet hon: logic, so sanh, rui ro.

            ## De xuat theo doi
            1-3 de xuat "nen theo doi/giam theo doi/them thong tin" (KHONG phai lenh mua/ban).

            ## Cau hoi phu
            1-2 cau hoi nguoi dung nen tu hoi lai.

            Verification: KHONG them loi khuyen mua/ban. KHONG du doan gia tuong lai. Chi dung du lieu trong danh sach.
            """;
    }
}
