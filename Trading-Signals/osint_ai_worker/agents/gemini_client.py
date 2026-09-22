import os
import json
import logging
from typing import List, Optional, Dict
from pydantic import BaseModel, Field
from openai import OpenAI
from google import genai
from google.genai import types
from dotenv import load_dotenv

load_dotenv()
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# ==========================================
# API CONFIGURATION
# ==========================================

# 9Router API (Default/Primary)
ROUTER_API_ENDPOINT = os.getenv("ROUTER_API_ENDPOINT") or os.getenv("NINE_ROUTER_ENDPOINT")
ROUTER_API_KEY = os.getenv("ROUTER_API_KEY") or os.getenv("NINE_ROUTER_API_KEY")
ROUTER_COMBO_NAME = os.getenv("ROUTER_COMBO_NAME") or os.getenv("NINE_ROUTER_MODEL")

# Gemini API (Fallback)
GEMINI_API_KEY = os.getenv("GEMINI_API_KEY")

# ==========================================
# CẤU TRÚC ĐẦU RA (PYDANTIC SCHEMAS)
# ==========================================

class SignalItem(BaseModel):
    category: str = Field(description="Categories: Policy, Liquidity, Inflation, Growth, Market Sentiment")
    signal: str = Field(description="Xu hướng tín hiệu rõ ràng (ví dụ: Hawkish, Dovish, Tightening, Easing...)")
    confidence: float = Field(ge=0.0, le=1.0, description="Độ tin cậy từ 0.0 đến 1.0")
    reason: str = Field(description="Lý do trích xuất tín hiệu từ văn bản")

class SignalOutput(BaseModel):
    signals: List[SignalItem]

class RwaTokenSuggestion(BaseModel):
    category: str = Field(description="Phân khúc tài sản/RWA: Trái phiếu Mỹ (Treasuries), Vàng (Physical Gold & Gold RWA), Tín dụng tư nhân, Bất động sản VN...")
    assets_or_tokens: List[str] = Field(description="Danh sách các mã token hoặc tên tài sản cụ thể. Ví dụ: ['ONDO', 'USDY'] hoặc ['Vàng vật chất', 'PAXG', 'XAUT'] hoặc ['BĐS HCM', 'BĐS Hà Nội', 'Đất nền vùng ven']")
    reason: str = Field(description="Lý do ngắn gọn tại sao chọn phân khúc này trong bối cảnh hiện tại.")

class CashAllocation(BaseModel):
    currency_distribution: Dict[str, float] = Field(description="Phân bổ tỷ lệ tiền mặt theo đồng tiền. Key: 'VND', 'USD', 'USDT', 'USDC'... Value: tỷ lệ từ 0.0 đến 1.0, tổng = 1.0")
    vn_bank_interest_rate: str = Field(description="Lãi suất ngân hàng VN hiện tại cho kỳ hạn phổ biến. Ví dụ: '5.0-5.5%/năm kỳ hạn 6 tháng tại Vietcombank/BIDV/VietinBank'")
    stablecoin_platform_yields: List[str] = Field(description="Danh sách lợi suất USD stable coin trên các nền tảng. Ví dụ: ['Binance Earn USDT Flexible: 5-8% APY', 'OKX Simple Earn USDT: 5-10% APY', 'AAVE USDC Lending: 3-5% APY']")
    recommendation: str = Field(description="Khuyến nghị chi tiết bằng Tiếng Việt: nên giữ VND hay USD/USDT, tỉ lệ bao nhiêu, gửi NH VN hay stake stablecoin trên sàn, cân nhắc rủi ro tỉ giá, rủi ro sàn, bảo hiểm tiền gửi...")

class RealEstateRecommendation(BaseModel):
    property_type: str = Field(description="Loại hình BĐS: 'Chung cư', 'Nhà phố', 'Đất nền', 'Biệt thự', 'Shophouse', 'BĐS công nghiệp', 'BĐS nghỉ dưỡng', 'Nhà ở xã hội', 'Đất nông nghiệp'...")
    area: str = Field(description="Khu vực/quận/huyện cụ thể. Ví dụ: 'Quận 2 (TP.Thủ Đức)', 'Huyện Bình Chánh', 'Quận Long Biên', 'TP. Dĩ An - Bình Dương', 'Huyện Nhà Bè', 'TP. Biên Hòa - Đồng Nai'")
    project: str = Field(description="Tên dự án cụ thể (nếu có) hoặc mô tả khu vực. Ví dụ: 'Vinhomes Grand Park', 'Eco Green Saigon', 'Đất nền khu dân cư Đại Phúc', 'KĐT Đông Tăng Long', 'KCN VSIP II', hoặc để trống nếu là khu vực tự do")
    price_range: str = Field(description="Khoảng giá tham khảo. Ví dụ: '2-3 tỷ/căn hộ 2PN', '15-25 triệu/m2 đất nền', '40-60 triệu/m2 chung cư cao cấp'")
    reason: str = Field(description="Lý do chọn khu vực/dự án này trong bối cảnh hiện tại: hạ tầng sắp hoàn thiện, gần metro/cao tốc, KCN thu hút FDI, giá còn hợp lý, pháp lý rõ ràng...")

class RealEstateVN(BaseModel):
    market_outlook: str = Field(description="Tổng quan thị trường bất động sản Việt Nam hiện tại. Viết bằng Tiếng Việt.")
    attractive_segments: List[str] = Field(description="Các phân khúc BĐS VN hấp dẫn trong bối cảnh hiện tại. Ví dụ: ['Căn hộ trung cấp TPHCM', 'Đất nền vùng ven Hà Nội', 'BĐS công nghiệp', 'BĐS nghỉ dưỡng']")
    recommended_properties: List[RealEstateRecommendation] = Field(description="Danh sách chi tiết các loại hình BĐS, khu vực, dự án cụ thể NÊN đầu tư. Liệt kê ít nhất 3-5 đề xuất cụ thể nhất có thể.")
    risks: List[str] = Field(description="Rủi ro khi đầu tư BĐS VN hiện tại. Ví dụ: ['Pháp lý chưa rõ ràng', 'Thanh khoản thấp', 'Giá đã tăng quá cao', 'Tín dụng BĐS bị siết']")
    recommendation: str = Field(description="Khuyến nghị hành động: NÊN hay KHÔNG NÊN đầu tư BĐS VN lúc này, phân khúc nào, thời điểm nào. Viết bằng Tiếng Việt.")

class AssetAllocation(BaseModel):
    increase_weight: List[str] = Field(description="Các loại tài sản cần TĂNG tỷ trọng. Ví dụ: ['USD/Tiền mặt', 'Vàng', 'Bất động sản VN', 'Cổ phiếu VN']")
    decrease_weight: List[str] = Field(description="Các loại tài sản cần GIẢM tỷ trọng. Ví dụ: ['Cổ phiếu', 'Crypto đầu cơ', 'Bất động sản VN']")
    rwa_strategy_details: List[RwaTokenSuggestion] = Field(description="Chi tiết các mã token RWA cụ thể được chọn lọc.")
    cash_allocation: CashAllocation = Field(description="Phân tích chi tiết phân bổ tiền mặt: nên giữ VND hay USD/USDT, so sánh lãi suất NH VN vs lợi suất stablecoin trên sàn (Binance, OKX...).")
    real_estate_vn: RealEstateVN = Field(description="Phân tích chuyên sâu về bất động sản Việt Nam: triển vọng, phân khúc hấp dẫn, rủi ro, khuyến nghị.")
    recommended_forex_pairs: List[str] = Field(description="Khuyến nghị giao dịch các cặp tiền Forex theo xu hướng vĩ mô hiện tại. Ví dụ: ['Mua EURUSD', 'Bán USDJPY', 'Bán USDCAD', 'Mua AUDUSD']")

class ThesisItem(BaseModel):
    thesis: str = Field(description="Tóm tắt ngắn gọn nhận định vĩ mô cốt lõi dựa trên TÍN HIỆU ĐÃ LỌC (2-3 câu).")
    confidence: float = Field(ge=0.0, le=1.0)
    allocation_plan: AssetAllocation = Field(description="Kế hoạch phân bổ danh mục được định dạng cấu trúc để hiển thị giao diện.")

class ThesisOutput(BaseModel):
    theses: List[ThesisItem]

class ProposedChangeItem(BaseModel):
    target_entity: str = Field(description="FED, ECB, BOE, BOJ, RBA, RBNZ, BoC, OPEC, OPEC+, SBV, VN_Economy, US_Economy, Global_Liquidity, Crypto_Market, Oil (Crude, WTI, Brent) v.v.")
    field_name: str = Field(description="trend, status, risk_level, production_policy, liquidity_status, v.v.")
    new_value: str = Field(description="BẮT BUỘC viết bằng Tiếng Việt. Dịch nghĩa hoàn toàn các từ như Thắt chặt (Diều hâu), Nới lỏng (Bồ câu), Trung lập, Tăng/Giảm sản lượng... Đưa ra thời gian dự kiến gần nhất các ngân hàng trung ương họp.")
    confidence: float = Field(ge=0.0, le=1.0)
    reason: str = Field(description="Lý do đề xuất thay đổi dựa trên tin tức vĩ mô mới, viết bằng Tiếng Việt.")

class WorldStateChangesOutput(BaseModel):
    proposed_changes: List[ProposedChangeItem]


# ==========================================
# LLM CLIENT (SINGLETON PATTERN)
# Primary: 9Router API | Fallback: Gemini API
# ==========================================

class LLMClient:
    def __init__(self):
        # 9Router OpenAI-compatible client (Primary)
        self.router_enabled = bool(ROUTER_API_KEY)
        if self.router_enabled:
            self.router_client = OpenAI(
                base_url=ROUTER_API_ENDPOINT,
                api_key=ROUTER_API_KEY,
            )
            self.router_combo = ROUTER_COMBO_NAME
        else:
            self.router_client = None
            self.router_combo = None
            logger.warning("ROUTER_API_KEY is not set. 9Router is disabled.")

        # Gemini client (Fallback)
        self.gemini_enabled = bool(GEMINI_API_KEY)
        if self.gemini_enabled:
            self.gemini_client = genai.Client(api_key=GEMINI_API_KEY)
            self.gemini_model = "gemini-2.0-flash"
        else:
            logger.warning("GEMINI_API_KEY is not set. Gemini fallback is disabled.")

    def _try_router(self, prompt: str, response_schema) -> dict:
        """Gọi 9Router API (OpenAI-compatible) với response_format json_object
        và nhúng JSON schema vào prompt (vì DeepSeek backend không hỗ trợ json_schema strict mode)."""
        if not self.router_enabled:
            raise RuntimeError("9Router API is not configured")
        schema_name = response_schema.__name__
        json_schema = response_schema.model_json_schema()

        # Nhúng schema vào prompt để model biết định dạng output mong đợi
        schema_prompt = f"""
{prompt}

---

**IMPORTANT: You MUST respond with a valid JSON object matching this exact JSON schema.**
Do NOT include any text outside the JSON object (no markdown code blocks, no explanations).

JSON Schema:
```json
{json.dumps(json_schema, ensure_ascii=False)}
```

Respond ONLY with the JSON object that conforms to the schema above.
"""

        logger.info(f"Calling 9Router API with combo={self.router_combo}, schema={schema_name}")
        response = self.router_client.chat.completions.create(
            model=self.router_combo,
            messages=[
                {"role": "user", "content": schema_prompt}
            ],
            temperature=0.1,
            response_format={"type": "json_object"},
        )
        content = response.choices[0].message.content
        return json.loads(content)

    def _try_gemini(self, prompt: str, response_schema) -> dict:
        """Gọi Gemini API (Fallback)."""
        if not self.gemini_enabled:
            raise RuntimeError("Gemini API is not configured")

        logger.info(f"Falling back to Gemini API with model={self.gemini_model}")
        response = self.gemini_client.models.generate_content(
            model=self.gemini_model,
            contents=prompt,
            config=types.GenerateContentConfig(
                response_mime_type="application/json",
                response_schema=response_schema,
                temperature=0.1,
            ),
        )
        return json.loads(response.text)

    def generate_structured_data(self, prompt: str, response_schema) -> dict:
        """Generate structured data: try 9Router first, fallback to Gemini on error."""
        # Try primary: 9Router API
        try:
            return self._try_router(prompt, response_schema)
        except Exception as e:
            logger.warning(f"9Router API failed: {e}. Trying Gemini fallback...")

        # Try fallback: Gemini API
        try:
            return self._try_gemini(prompt, response_schema)
        except Exception as e:
            logger.error(f"Gemini fallback also failed: {e}")
            return {}

# Khởi tạo một client dùng chung cho toàn bộ app
global_gemini_client = LLMClient()


# ==========================================
# CORE CORE FUNCTIONS
# ==========================================

def extract_signals(news_content: str) -> dict:
    """Bước 1: Trích xuất dữ liệu cứng/tín hiệu thô từ tin tức văn bản"""
    prompt = f"""
    You are a Quant Researcher and Macro-Economic Analyst.
    Analyze the following news text and extract key macroeconomic signals.
    Focus strictly on hard data, statements, and actual event outcomes.

    News Text: {news_content}
    """
    return global_gemini_client.generate_structured_data(prompt, SignalOutput)


def generate_thesis(extracted_signals: dict, interest_rate_context: str = None, triggered_alerts_context: str = None) -> dict:
    interest_section = ""
    if interest_rate_context:
        interest_section = f"""
    Danh sách Lãi suất gửi tiết kiệm thực tế tại các Ngân hàng Việt Nam (cập nhật từ Cake.vn):
    {interest_rate_context}
    
    Hãy ưu tiên tham chiếu và trích xuất số liệu từ bảng lãi suất thực tế này để so sánh, phân tích trong phần 'cash_allocation'. Khi đưa ra con số lãi suất gửi tiết kiệm VN thực tế, hãy chỉ rõ các ngân hàng thương mại đang có lãi suất cao nổi trội (ví dụ như HLBank, Cake by VPBank, OceanBank, LPBank... với lãi suất 6.5 - 7.4%/năm cho kỳ hạn 6-12 tháng) bên cạnh nhóm ngân hàng nhà nước.
    """

    alerts_section = ""
    if triggered_alerts_context:
        alerts_section = f"""
    Danh sách các Cảnh báo giá/lợi suất đã kích hoạt gần đây (Triggered Alerts):
    {triggered_alerts_context}
    
    Hãy phân tích chi tiết các cảnh báo kích hoạt này (đặc biệt là các tín hiệu vượt đỉnh 52 tuần của lợi suất trái phiếu chính phủ Mỹ US30Y/US10Y, giá vàng, giá dầu, hoặc các cảnh báo tiền mã hóa/cổ phiếu lớn) để đánh giá sự chuyển dịch của dòng tiền vĩ mô (Money Flows) và các khuyến nghị phân bổ vốn/giao dịch ngoại hối.
    """

    prompt = f"""
    Bạn là một nhà quản lý quỹ định lượng (Quant Fund Manager) và chuyên gia phân tích chu kỳ dòng tiền tài chính vĩ mô.
    Dựa trên danh sách các TÍN HIỆU CỨNG đã được trích xuất dưới đây:
    
    Extracted Signals (JSON):
    {json.dumps(extracted_signals, ensure_ascii=False)}
    
    {interest_section}
    {alerts_section}
    
    Nhiệm vụ của bạn là đưa ra nhận định vĩ mô và lập kế hoạch phân bổ danh mục chi tiết theo định dạng cấu trúc.

    YÊU CẦU ĐẶC BIỆT VỀ CHIẾN LƯỢC TÀI SẢN PHÒNG THỦ & RWA:
    Hãy phân tích bối cảnh và chỉ định chính xác các mã tài sản/token vào trường 'rwa_strategy_details' nếu có phân bổ:

    1. Nếu LÃI SUẤT FED CAO KÉO DÀI (Hawkish / Higher-for-longer) HOẶC THANH KHOẢN THẮT CHẶT:
       - Phân khúc: 'Trái phiếu Mỹ (Treasuries), Tiền mặt (Cash), Lãi suất ngân hàng Việt Nam'
       - Gợi ý chính xác: ['ONDO', 'USDY']
       - Lý do: Khai thác lợi suất phi rủi ro 4.5% - 5% từ tín phiếu kho bạc Mỹ ngay trên chuỗi.

    2. Nếu ĐỊA CHÍNH TRỊ LEO THANG (Chiến sự Mỹ-Iran, nghẽn mạch eo biển):
       - Phân khúc: 'Vàng (Physical Gold & Gold RWA)'
       - Gợi ý chính xác bao gồm cả tài sản vật chất và on-chain: ['Vàng vật chất', 'PAXG', 'XAUT']
       - Lý do: Kết hợp giữa việc nắm giữ vàng vật chất ngoài đời thực làm tài sản trú ẩn tối hậu (Sound Money) và các token vàng trên chuỗi để tối ưu hóa tính thanh khoản và khả năng giao dịch linh hoạt 24/7.

    3. Nếu VĨ MÔ ỔN ĐỊNH, LÃI SUẤT HẠ NHIỆT (Dovish / Easing):
       - Phân khúc: 'Tín dụng tư nhân (Private Credit)'
       - Gợi ý chính xác các token: ['CFG', 'MPL']
       - Lý do: Tìm kiếm lợi nhuận (yield) cao hơn từ dòng vốn tăng trưởng doanh nghiệp.

    ---

    YÊU CẦU PHÂN TÍCH BẤT ĐỘNG SẢN VIỆT NAM (real_estate_vn):
    Dựa trên tín hiệu vĩ mô (đặc biệt là lãi suất SBV, tăng trưởng tín dụng, CPI Việt Nam, FDI, chính sách nhà ở, đầu tư công, v.v.), hãy phân tích:
    
    - Tổng quan thị trường BĐS VN: Xu hướng giá, thanh khoản, tâm lý thị trường.
    - Phân khúc hấp dẫn nhất trong bối cảnh hiện tại:
      + Nếu lãi suất VN GIẢM & tín dụng BĐS NỚI: Căn hộ trung cấp, đất nền vùng ven, BĐS công nghiệp.
      + Nếu lãi suất VN TĂNG & tín dụng BĐS SIẾT: Hạn chế BĐS, ưu tiên giữ tiền mặt hoặc kênh khác.
      + Nếu đầu tư công & hạ tầng ĐẨY MẠNH (cao tốc, metro, sân bay): BĐS vùng ven hưởng lợi hạ tầng.
    - Rủi ro cần lưu ý: pháp lý (sổ đỏ, giải phóng mặt bằng), thanh khoản, định giá quá cao, chính sách thuế BĐS...
    - Khuyến nghị cụ thể: NÊN hay KHÔNG NÊN đầu tư BĐS VN lúc này? Nếu có, phân khúc nào, khu vực nào?

    **QUAN TRỌNG - LIỆT KÊ recommended_properties chi tiết nhất có thể (ít nhất 3-5 đề xuất):**
    Điền đầy đủ vào mảng 'recommended_properties' - mỗi phần tử gồm:
    - 'property_type': Loại hình BĐS cụ thể (Chung cư, Nhà phố, Đất nền, Biệt thự, Shophouse, BĐS công nghiệp, BĐS nghỉ dưỡng, Nhà ở xã hội, Đất nông nghiệp...)
    - 'area': Khu vực/quận/huyện cụ thể, KHÔNG nói chung chung. Ví dụ: 'Bán đảo Thanh Đa (Quận Bình Thạnh)', 'Huyện Hóc Môn', 'Quận 9 (TP.Thủ Đức)', 'Huyện Bình Chánh', 'Quận Long Biên - Hà Nội', 'TP. Dĩ An - Bình Dương', 'Huyện Nhà Bè'.
    - 'project': Tên dự án cụ thể nếu có. BAO GỒM CẢ CÁC DỰ ÁN SẮP MỞ BÁN hoặc ĐANG TRIỂN KHAI GIAI ĐOẠN ĐẦU. Bạn BẮT BUỘC phải ưu tiên đề xuất các dự án của những chủ đầu tư danh tiếng/uy tín như: 'Dự án Bán đảo Thanh Đa (Bình Quới - Thanh Đa)', 'Vinhomes Saigon Park (Hóc Môn) - sắp mở bán', 'Vinhomes Grand Park', 'The Global City', 'KĐT Đông Tăng Long', 'KĐT Sala', 'KĐT Vinhomes Ocean Park 2-3'. Với dự án sắp mở bán, ghi rõ trạng thái (vd: 'sắp mở bán', 'đang giải phóng mặt bằng', 'đang triển khai hạ tầng').
    - 'price_range': Khoảng giá tham khảo (ví dụ: '2-3 tỷ/căn hộ 2PN', '15-25 triệu/m2 đất nền', '40-60 triệu/m2 chung cư cao cấp', '800 triệu-1.5 tỷ/lô đất nền'). Với dự án sắp mở bán, ghi giá dự kiến nếu có thông tin (vd: 'Dự kiến 35-45 triệu/m2', 'Giá chưa công bố - tham khảo khu vực lân cận').
    - 'reason': Lý do chọn khu vực/dự án này trong bối cảnh hiện tại (ví dụ: 'Được phát triển bởi chủ đầu tư lớn có tiếng và uy tín như Vingroup/Bitexco, đảm bảo tiến độ và tính pháp lý', 'Hưởng lợi từ quy hoạch siêu đô thị sinh thái Bán đảo Thanh Đa', 'Đón đầu quy hoạch lên quận của Hóc Môn và hạ tầng Vinhomes Saigon Park', 'Gần metro Bến Thành - Suối Tiên', 'Hưởng lợi từ cao tốc Bến Lức - Long Thành', 'hưởng lợi từ vành đai 3',...).

    ---

    YÊU CẦU PHÂN TÍCH PHÂN BỔ TIỀN MẶT: VND vs USD (cash_allocation):
    Dựa trên bối cảnh vĩ mô, hãy phân tích chi tiết chiến lược giữ tiền mặt:
    
    1. TỶ GIÁ VND/USD:
       - Xu hướng tỷ giá: SBV đang bảo vệ VND hay để trượt giá? Dự trữ ngoại hối ra sao?
       - Nếu VND được dự báo MẤT GIÁ >3%/năm: Nên ưu tiên giữ USD.
       - Nếu VND ỔN ĐỊNH hoặc SBV đang thắt chặt để bảo vệ tỷ giá: Có thể giữ một phần VND.
    
    2. SO SÁNH LÃI SUẤT:
       - Lãi suất tiền gửi ngân hàng VN (VND): Ưu tiên sử dụng số liệu thực tế được cập nhật từ bảng Cake.vn ở trên (ví dụ: khoảng 6.0 - 7.4%/năm cho kỳ hạn 6-12 tháng tại các ngân hàng thương mại). Nếu không có bảng dữ liệu thực tế, sử dụng số liệu mặc định khoảng 4.5-5.5%/năm cho kỳ hạn 6-12 tháng. Có bảo hiểm tiền gửi (tối đa 75 triệu VND). An toàn cao, thanh khoản tốt.
       - Lợi suất stablecoin USD (USDT/USDC) trên các sàn:
         + Binance Earn Flexible: ~5-10% APY (thay đổi theo thị trường)
         + OKX Simple Earn: ~5-10% APY
         + Bybit Earn: ~4-8% APY
         + Lending trên AAVE/Compound (on-chain): ~3-6% APY (tùy utilization rate)
         + Rủi ro: Rủi ro sàn (exchange default, hack), rủi ro smart contract, rủi ro depeg stablecoin, không có bảo hiểm tiền gửi.
       - Lãi suất USD gửi ngân hàng VN: ~0% (gần như không có lãi suất cho USD gửi tại NH VN)
    
    3. PHÂN BỔ KHUYẾN NGHỊ:
       - Trong bối cảnh lãi suất FED CAO: USD mạnh -> nên giữ tỷ trọng USD/USDT cao (60-80%), VND thấp (20-40%).
       - Trong bối cảnh FED HẠ LÃI SUẤT: USD yếu đi -> có thể tăng tỷ trọng VND lên để hưởng lãi suất cao hơn.
       - Nếu chấp nhận rủi ro để tối ưu lợi suất: stake USDT/USDC trên Binance/OKX (lợi suất 5-10% APY, vượt trội so với gửi VND 4.5-5.5% sau khi trừ trượt giá ~2-3%/năm).
       - Nếu ƯU TIÊN AN TOÀN: gửi VND tại ngân hàng lớn (Vietcombank, BIDV, VietinBank) hưởng 4.5-5.5%, có bảo hiểm tiền gửi.
       - Kết hợp cả hai: một phần VND gửi NH (an toàn), một phần USDT stake trên sàn lớn (sinh lời cao hơn).
       - Thế chấp sổ tiết kiệm ngoại tệ (USD) để vay VND (80-100% giá trị sổ tiết kiệm tùy bank) rồi dùng chính số tiền vay đó gửi tiết kiệm ngược lại để ăn chênh lệch lãi suất. lãi vay USD ~5%/năm, lãi gửi VND ~6-7%/năm, ăn chênh lệch 1-2%/năm. Rủi ro: tỷ giá VND/USD biến động, lãi suất thay đổi, thanh khoản sổ tiết kiệm. Vừa được lời từ lãi suất chênh lệch và trượt giá VND/USD mà vẫn giữ được USD.

    YÊU CẦU PHÂN TÍCH KHUYẾN NGHỊ GIAO DỊCH FOREX (recommended_forex_pairs):
    Dựa trên xu hướng DXY, lợi suất trái phiếu Mỹ, giá dầu mỏ, và tâm lý thị trường (Risk-On / Risk-Off), hãy đề xuất các vị thế giao dịch Forex phù hợp (liệt kê tối thiểu 3 cặp tiền cụ thể kèm theo hướng đi Mua/Bán rõ ràng, ví dụ: 'Mua EURUSD', 'Bán USDJPY', 'Bán USDCAD', 'Mua AUDUSD'):
    - Nếu USD mạnh (DXY tăng, lợi suất Mỹ tăng): Mua USDJPY, Mua USDCAD, Bán EURUSD, Bán GBPUSD.
    - Nếu tâm lý Risk-On: Mua AUDUSD, Mua NZDUSD, Bán USDCHF.
    - Nếu tâm lý Risk-Off (lo sợ, chiến tranh): Mua USDCHF, Mua XAUUSD.
    - Nếu giá dầu tăng: Bán USDCAD (CAD mạnh lên).

    YÊU CẦU ĐỊNH DẠNG:
    - Điền chính xác các nhóm tài sản cần tăng/giảm vào 'increase_weight' và 'decrease_weight' (bao gồm 'Bất động sản VN' nếu phù hợp).
    - Điền đầy đủ thông tin vào 'cash_allocation' (currency_distribution, vn_bank_interest_rate, stablecoin_platform_yields, recommendation).
    - Điền đầy đủ thông tin vào 'real_estate_vn' (market_outlook, attractive_segments, recommended_properties, risks, recommendation). Mảng 'recommended_properties' là BẮT BUỘC, phải có ít nhất 3-5 đề xuất với property_type, area, project, price_range, reason cụ thể.
    - Điền đầy đủ thông tin các cặp tiền Forex khuyến nghị giao dịch vào 'recommended_forex_pairs' (tối thiểu 3 cặp cụ thể kèm hành động Mua/Bán).
    - Toàn bộ phần mô tả lý do (reason, thesis, recommendation, market_outlook) BẮT BUỘC viết bằng Tiếng Việt.
    """
    return global_gemini_client.generate_structured_data(prompt, ThesisOutput)


def propose_world_state_changes(current_state: dict, signals: dict, theses: dict) -> dict:
    """Bước 3: Đề xuất cập nhật trạng thái hệ thống bằng Tiếng Việt"""
    prompt = f"""
    Bạn là AI Quản lý World State cho một nền tảng Macro Intelligence.
    Nhiệm vụ của bạn là so sánh Trạng thái Thế giới hiện tại (Current World State) với các Tín hiệu (Signals) và Nhận định (Theses) mới nhận được, từ đó đề xuất cập nhật.
    
    YÊU CẦU NGÔN NGỮ & ĐỊNH DANH (MỘT CÁCH TUYỆT ĐỐI):
    - Các trường "new_value" và "reason" BẮT BUỘC phải viết bằng Tiếng Việt.
    - Dịch hoàn toàn các thuật ngữ kinh tế sang tiếng Việt tương đương:
      + "Hawkish" / "Tightening" -> "Thắt chặt (Diều hâu)" hoặc "Tăng lãi suất".
      + "Dovish" / "Easing" -> "Nới lỏng (Bồ câu)" hoặc "Giảm lãi suất".
      + "Neutral" -> "Trung lập".
      + Biến động cung ứng: "Cắt giảm sản lượng", "Tăng sản lượng", "Giữ nguyên sản lượng".
      + Ngân hàng Nhà nước VN (SBV): "Hạ lãi suất điều hành", "Bơm/Hút thanh khoản"...

    Current World State (JSON):
    {json.dumps(current_state, ensure_ascii=False)}
    
    Recent Signals (JSON):
    {json.dumps(signals, ensure_ascii=False)}
    
    Active Theses (JSON):
    {json.dumps(theses, ensure_ascii=False)}
    
    Nếu dữ liệu mới trùng khớp hoàn toàn với Current World State hoặc không đủ trọng số để thay đổi, trả về danh sách proposed_changes rỗng [].
    """
    return global_gemini_client.generate_structured_data(prompt, WorldStateChangesOutput)