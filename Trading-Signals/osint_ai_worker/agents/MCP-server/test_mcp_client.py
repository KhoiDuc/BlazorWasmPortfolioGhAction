import asyncio
import os
import sys
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client

# Xác định đường dẫn tuyệt đối đến file mcp server
current_dir = os.path.dirname(os.path.abspath(__file__))
server_path = os.path.join(current_dir, "gemini_mcp_server.py")

# Cấu hình tham số để chạy MCP Server
server_params = StdioServerParameters(
    command=sys.executable,  # Sử dụng chính python environment hiện tại
    args=[server_path],
    env={
        **os.environ,
        "PYTHONPATH": os.path.dirname(current_dir) # Để server import được "agents.*"
    }
)

async def run_mcp_client():
    print("Đang khởi tạo kết nối stdio đến MCP Server...")
    
    # Thiết lập kết nối stdio transport
    async with stdio_client(server_params) as (read_stream, write_stream):
        # Tạo session Client MCP
        async with ClientSession(read_stream, write_stream) as session:
            # 1. Khởi tạo bắt tay (Handshake) với Server
            await session.initialize()
            print("-> Đã kết nối thành công!")

            # 2. Lấy danh sách các Tools hiện có trên Server
            response = await session.list_tools()
            print("\n=== CÁC TOOL CÓ TRÊN SERVER ===")
            for tool in response.tools:
                print(f"- {tool.name}: {tool.description.splitlines()[0]}")
            
            # 3. Thử gọi (call) tool "tool_extract_signals"
            news_test = "Tin vĩ mô: FED công bố giữ nguyên lãi suất điều hành ở mức 5.25%-5.5%, phát đi tín hiệu diều hâu (Hawkish) do lạm phát CPI tháng 5 tăng nhẹ lên mức 3.3%."
            print(f"\n-> Đang gọi tool 'tool_extract_signals' với tin tức test...")
            
            result = await session.call_tool(
                name="tool_extract_signals",
                arguments={"news_content": news_test}
            )
            
            # Kết quả trả về từ MCP Server thường là một list các text/image block
            print("\n=== KẾT QUẢ TỪ TOOL ===")
            for content_block in result.content:
                if content_block.type == "text":
                    print(content_block.text)

if __name__ == "__main__":
    # MCP Client chạy bất đồng bộ (asynchronous) bằng asyncio
    asyncio.run(run_mcp_client())
