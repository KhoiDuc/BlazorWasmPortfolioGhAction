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
}
