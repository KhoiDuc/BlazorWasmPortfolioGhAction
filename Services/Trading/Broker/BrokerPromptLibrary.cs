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
}
