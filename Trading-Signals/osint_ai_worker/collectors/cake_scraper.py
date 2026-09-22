import urllib.request
import re
import logging

logger = logging.getLogger(__name__)

def html_table_to_markdown(html_table: str) -> str:
    """
    Converts an HTML table string into a clean Markdown table string.
    Works by using regular expressions to parse <tr> and <td>/<th> elements.
    """
    # Remove metadata tags we don't need
    table_content = re.sub(r'</?(table|tbody|thead|colgroup|col).*?>', '', html_table, flags=re.IGNORECASE | re.DOTALL)
    
    # Extract all table rows
    rows = re.findall(r'<tr.*?>.*?</tr>', table_content, flags=re.IGNORECASE | re.DOTALL)
    markdown_lines = []
    
    for i, row in enumerate(rows):
        # Extract headers or data cells
        cells = re.findall(r'<t[dh].*?>(.*?)</t[dh]>', row, flags=re.IGNORECASE | re.DOTALL)
        
        # Clean inline HTML tags (like <strong>, <span>, <u>, etc.) from cells
        clean_cells = []
        for cell in cells:
            text = re.sub(r'<.*?>', '', cell).strip()
            # Normalize whitespace/tabs/newlines inside cells
            text = re.sub(r'\s+', ' ', text)
            clean_cells.append(text)
            
        if not clean_cells:
            continue
            
        markdown_lines.append("| " + " | ".join(clean_cells) + " |")
        
        # Add separator line after the header row (row index 0)
        if i == 0:
            separators = []
            for cell in clean_cells:
                # If cell represents a number, align right, otherwise align left
                clean_val = cell.replace('%', '').replace('.', '').replace('-', '').strip()
                if clean_val.isdigit():
                    separators.append(" :---: ")
                else:
                    separators.append(" :--- ")
            markdown_lines.append("|" + "|".join(separators) + "|")
            
    return "\n".join(markdown_lines)

def fetch_cake_interest_rates() -> str:
    """
    Fetches the Cake.vn interest rate comparison article and returns the extracted
    monthly interest rate tables in Markdown format.
    
    Returns:
        str: Cleaned markdown content containing the interest rate tables,
             or None if the request/parsing fails.
    """
    url = "https://cake.vn/tin-tuc/tai-chinh/lai-suat-gui-tiet-kiem-ngan-hang-nao-cao-nhat"
    logger.info(f"Scraping Vietnam bank interest rates from Cake.vn: {url}")
    
    try:
        # Create request with a browser-like User-Agent
        req = urllib.request.Request(
            url,
            headers={
                'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
                'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
                'Accept-Language': 'vi,en-US;q=0.7,en;q=0.3'
            }
        )
        
        # Fetch the webpage
        with urllib.request.urlopen(req, timeout=15) as response:
            html = response.read().decode('utf-8', errors='ignore')
            
        # Extract title and meta description
        title_match = re.search(r'<title>(.*?)</title>', html, flags=re.IGNORECASE)
        title = title_match.group(1).strip() if title_match else "Lãi suất gửi tiết kiệm ngân hàng cao hiện nay"
        
        desc_match = re.search(r'<meta\s+name="description"\s+content="(.*?)"', html, flags=re.IGNORECASE)
        if not desc_match:
            desc_match = re.search(r'<meta\s+content="(.*?)"\s+name="description"', html, flags=re.IGNORECASE)
        desc = desc_match.group(1).strip() if desc_match else ""
        
        # Find all table structures in the page
        tables = re.findall(r'<table.*?>.*?</table>', html, flags=re.IGNORECASE | re.DOTALL)
        
        markdown_content = f"### {title}\n"
        if desc:
            markdown_content += f"*{desc}*\n\n"
            
        if tables:
            logger.info(f"Found {len(tables)} table(s) on the Cake.vn page. Converting to Markdown...")
            for idx, table in enumerate(tables, 1):
                markdown_table = html_table_to_markdown(table)
                if markdown_table.strip():
                    markdown_content += f"#### Bảng {idx}:\n{markdown_table}\n\n"
        else:
            logger.warning("No HTML tables found on the Cake.vn page. Extracting raw article content as fallback.")
            # Clean styles and scripts
            body_text = re.sub(r'<script.*?>.*?</script>', '', html, flags=re.IGNORECASE | re.DOTALL)
            body_text = re.sub(r'<style.*?>.*?</style>', '', body_text, flags=re.IGNORECASE | re.DOTALL)
            body_text = re.sub(r'<.*?>', ' ', body_text, flags=re.DOTALL)
            body_text = re.sub(r'\s+', ' ', body_text)
            markdown_content += f"\n{body_text[:10000]}..."
            
        return markdown_content
        
    except Exception as e:
        logger.error(f"Failed to fetch or parse Cake.vn interest rates: {e}")
        return None

if __name__ == "__main__":
    # Test script locally when run directly
    logging.basicConfig(level=logging.INFO)
    res = fetch_cake_interest_rates()
    if res:
        print("SUCCESS! Scraped rates:")
        print(res[:1500])
    else:
        print("FAILED to fetch rates.")
