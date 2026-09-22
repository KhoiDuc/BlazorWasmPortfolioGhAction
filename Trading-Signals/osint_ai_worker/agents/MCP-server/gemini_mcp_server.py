import os
import sys
import json
from mcp.server.fastmcp import FastMCP

# Ensure the parent directory is in sys.path to resolve relative imports
current_dir = os.path.dirname(os.path.abspath(__file__))
parent_dir = os.path.dirname(current_dir)  # osint_ai_worker path
if parent_dir not in sys.path:
    sys.path.insert(0, parent_dir)

# Now import the core functions from the original gemini_client
from agents.gemini_client import extract_signals, generate_thesis, propose_world_state_changes

# Initialize FastMCP Server named "Trading Signals Macro Analyzer"
# FastMCP automatically generates schema from function signatures, docstrings, and type hints.
mcp = FastMCP("Trading Signals Macro Analyzer")

@mcp.tool()
def tool_extract_signals(news_content: str) -> str:
    """
    Trích xuất các tín hiệu vĩ mô thô (category, signal, confidence, reason) từ văn bản tin tức.
    
    Args:
        news_content: Nội dung văn bản của tin tức kinh tế, bài báo vĩ mô.
    """
    try:
        result = extract_signals(news_content)
        return json.dumps(result, ensure_ascii=False, indent=2)
    except Exception as e:
        return json.dumps({"error": str(e)}, ensure_ascii=False)

@mcp.tool()
def tool_generate_thesis(extracted_signals_json: str) -> str:
    """
    Nhận định vĩ mô cốt lõi và lập kế hoạch phân bổ danh mục (bao gồm BĐS VN, RWA, Tiền mặt) từ các tín hiệu vĩ mô đã trích xuất.
    
    Args:
        extracted_signals_json: Chuỗi JSON chứa danh sách các tín hiệu vĩ mô thô.
    """
    try:
        extracted_signals = json.loads(extracted_signals_json)
        result = generate_thesis(extracted_signals)
        return json.dumps(result, ensure_ascii=False, indent=2)
    except Exception as e:
        return json.dumps({"error": f"Lỗi parse JSON hoặc xử lý: {str(e)}"}, ensure_ascii=False)

@mcp.tool()
def tool_propose_world_state_changes(current_state_json: str, signals_json: str, theses_json: str) -> str:
    """
    Đề xuất thay đổi trạng thái thế giới (World State) dựa trên tín hiệu mới và nhận định hiện tại.
    
    Args:
        current_state_json: Trạng thái thế giới hiện tại dưới dạng JSON string.
        signals_json: Các tín hiệu vĩ mô mới nhất dưới dạng JSON string.
        theses_json: Các nhận định vĩ mô đang hoạt động dưới dạng JSON string.
    """
    try:
        current_state = json.loads(current_state_json)
        signals = json.loads(signals_json)
        theses = json.loads(theses_json)
        result = propose_world_state_changes(current_state, signals, theses)
        return json.dumps(result, ensure_ascii=False, indent=2)
    except Exception as e:
        return json.dumps({"error": f"Lỗi parse JSON hoặc xử lý: {str(e)}"}, ensure_ascii=False)

if __name__ == "__main__":
    # Chạy server MCP qua giao thức stdio (tiêu chuẩn kết nối của MCP)
    mcp.run()
