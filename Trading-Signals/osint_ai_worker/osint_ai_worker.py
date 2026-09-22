import logging
from apscheduler.schedulers.background import BackgroundScheduler
import time
import os
import sys
import uuid
import json
import psycopg2
from dotenv import load_dotenv
from http.server import HTTPServer, BaseHTTPRequestHandler
import threading

load_dotenv()

# Set up logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

# Adaptive scheduling state
_last_extraction_time = 0
_last_extraction_news_count = 0
_last_thesis_time = 0
_last_world_state_time = 0

def get_unprocessed_news_count():
    """Get count of unprocessed news items"""
    db_url = os.getenv("DATABASE_URL")
    if not db_url:
        return 0
    try:
        conn = psycopg2.connect(db_url)
        cur = conn.cursor()
        cur.execute("""
            SELECT COUNT(*) FROM news_items 
            WHERE id NOT IN (SELECT source_news_id FROM osint_signals)
            AND status = 'active'
        """)
        count = cur.fetchone()[0]
        cur.close()
        conn.close()
        return count
    except Exception as e:
        logger.warning(f"Could not get unprocessed news count: {e}")
        return 0

def get_new_signals_since(timestamp):
    """Count signals created since a given timestamp"""
    db_url = os.getenv("DATABASE_URL")
    if not db_url:
        return 0
    try:
        conn = psycopg2.connect(db_url)
        cur = conn.cursor()
        cur.execute("SELECT COUNT(*) FROM osint_signals WHERE created_at > to_timestamp(%s)", (timestamp,))
        count = cur.fetchone()[0]
        cur.close()
        conn.close()
        return count
    except Exception as e:
        logger.warning(f"Could not count new signals: {e}")
        return 0

def is_ai_enabled():
    """Check if AI features are enabled via system_settings"""
    db_url = os.getenv("DATABASE_URL")
    if not db_url:
        return False
    try:
        conn = psycopg2.connect(db_url)
        cur = conn.cursor()
        cur.execute("SELECT value FROM system_settings WHERE key = 'ai_enabled'")
        row = cur.fetchone()
        cur.close()
        conn.close()
        return row and row[0].lower() == 'true'
    except Exception as e:
        logger.warning(f"Could not check ai_enabled setting: {e}. Assuming enabled.")
        return True

def get_ai_prompt_from_db():
    """Read AI prompt template from system_settings"""
    db_url = os.getenv("DATABASE_URL")
    if not db_url:
        return ""
    try:
        conn = psycopg2.connect(db_url)
        cur = conn.cursor()
        cur.execute("SELECT value FROM system_settings WHERE key = 'ai_prompt_template'")
        row = cur.fetchone()
        cur.close()
        conn.close()
        return row[0] if row else ""
    except Exception as e:
        logger.warning(f"Could not read ai_prompt_template: {e}")
        return ""

def run_signal_extraction():
    global _last_extraction_time, _last_extraction_news_count

    if not is_ai_enabled():
        logger.info("AI features disabled. Skipping signal extraction.")
        return

    # Check how many unprocessed news items exist
    unprocessed_count = get_unprocessed_news_count()

    if unprocessed_count == 0:
        logger.info("No unprocessed news. Skipping signal extraction (nothing new).")
        return

    # Adaptive batch size: more news -> process more at once
    # 1-5 news -> process all (max 5)
    # 6-15 news -> process 15
    # 16+ news -> process 30 (burst mode)
    if unprocessed_count <= 5:
        batch_limit = 5
    elif unprocessed_count <= 15:
        batch_limit = 15
    else:
        batch_limit = 30

    logger.info(f"Running signal extraction job... ({unprocessed_count} unprocessed news, batch limit: {batch_limit})")

    try:
        from agents.gemini_client import extract_signals
        db_url = os.getenv("DATABASE_URL")
        if not db_url: return
            
        conn = psycopg2.connect(db_url)
        cur = conn.cursor()
        
        # Get unprocessed news items
        cur.execute("""
            SELECT id, title, content 
            FROM news_items 
            WHERE id NOT IN (SELECT source_news_id FROM osint_signals)
            AND status = 'active'
            ORDER BY created_at DESC LIMIT %s
        """, (batch_limit,))
        rows = cur.fetchall()
        
        for r in rows:
            news_id, title, content = r
            text = f"Title: {title}\nContent: {content}"
            result = extract_signals(text)
            if result and "signals" in result:
                for s in result["signals"]:
                    s_id = str(uuid.uuid4())
                    cat = s.get("category", "General")
                    sig = s.get("signal", "")
                    conf = float(s.get("confidence", 0.5))
                    reason = s.get("reason", "")
                    
                    cur.execute(
                        "INSERT INTO osint_signals (id, source_news_id, category, signal, confidence, reason) VALUES (%s, %s, %s, %s, %s, %s)",
                        (s_id, news_id, cat, sig, conf, reason)
                    )
        conn.commit()
        cur.close()
        conn.close()
        
        _last_extraction_time = time.time()
        _last_extraction_news_count = len(rows)
        logger.info(f"Successfully extracted signals from {len(rows)} news items.")
    except Exception as e:
        logger.error(f"Error in run_signal_extraction: {e}")

def run_thesis_update():
    global _last_thesis_time

    if not is_ai_enabled():
        logger.info("AI features disabled. Skipping thesis update.")
        return

    # Check if there are new signals since last thesis run
    if _last_thesis_time > 0:
        new_signal_count = get_new_signals_since(_last_thesis_time)
        if new_signal_count == 0:
            logger.info("No new signals since last thesis update. Skipping.")
            return
        logger.info(f"{new_signal_count} new signals since last thesis update. Generating thesis...")
    else:
        logger.info("First thesis run. Generating thesis...")

    logger.info("Running thesis update job...")
    try:
        from agents.gemini_client import generate_thesis
        from collectors.cake_scraper import fetch_cake_interest_rates
        db_url = os.getenv("DATABASE_URL")
        if not db_url:
            logger.error("DATABASE_URL not found")
            return
            
        conn = psycopg2.connect(db_url)
        cur = conn.cursor()
        
        # Get latest 50 signals
        cur.execute("SELECT category, signal, confidence, reason FROM osint_signals ORDER BY created_at DESC LIMIT 50")
        sig_rows = cur.fetchall()
        
        signals_list = []
        for r in sig_rows:
            signals_list.append({
                "category": r[0],
                "signal": r[1],
                "confidence": r[2],
                "reason": r[3]
            })
            
        # If no signals in DB, try extracting from news first
        if not signals_list:
            logger.info("No signals found in DB. Attempting to extract signals from news first...")
            cur.close()
            conn.close()
            run_signal_extraction()
            conn = psycopg2.connect(db_url)
            cur = conn.cursor()
            cur.execute("SELECT category, signal, confidence, reason FROM osint_signals ORDER BY created_at DESC LIMIT 50")
            sig_rows = cur.fetchall()
            for r in sig_rows:
                signals_list.append({
                    "category": r[0],
                    "signal": r[1],
                    "confidence": r[2],
                    "reason": r[3]
                })

        if not signals_list:
            logger.warning("No signals available to generate thesis.")
            cur.close()
            conn.close()
            return
            
        extracted_signals = {"signals": signals_list}
        
        logger.info("Fetching interest rates from Cake.vn...")
        interest_rates = fetch_cake_interest_rates()
        
        # Fetch latest 20 triggered alerts to analyze macro/money flows
        logger.info("Fetching latest triggered alerts for thesis context...")
        cur.execute("SELECT asset_type, symbol, price, message, created_at FROM triggered_alerts ORDER BY created_at DESC LIMIT 20")
        alert_rows = cur.fetchall()
        alert_list = []
        for ar in alert_rows:
            alert_list.append(f"- [{ar[4].strftime('%Y-%m-%d %H:%M:%S')}] {ar[0].upper()} ({ar[1]}): {ar[3]} (Giá: {ar[2]})")
        triggered_alerts_context = "\n".join(alert_list) if alert_list else "Không có cảnh báo kích hoạt gần đây."
        
        logger.info("Generating thesis with AI...")
        result = generate_thesis(
            extracted_signals, 
            interest_rate_context=interest_rates, 
            triggered_alerts_context=triggered_alerts_context
        )
        
        if result and "theses" in result:
            # Mark old theses as expired
            cur.execute("UPDATE osint_theses SET status = 'expired' WHERE status = 'active'")
            
            for t in result["theses"]:
                t_id = str(uuid.uuid4())
                thesis_text = t.get("thesis", "")
                conf = float(t.get("confidence", 0.5))
                
                # Transform allocation_plan into supporting_evidence string
                allocation = t.get("allocation_plan", {})
                inc = allocation.get("increase_weight", []) if isinstance(allocation, dict) else []
                dec = allocation.get("decrease_weight", []) if isinstance(allocation, dict) else []
                rwa = allocation.get("rwa_strategy_details", []) if isinstance(allocation, dict) else []
                cash_alloc = allocation.get("cash_allocation", {}) if isinstance(allocation, dict) else {}
                re_vn = allocation.get("real_estate_vn", {}) if isinstance(allocation, dict) else {}
                forex = allocation.get("recommended_forex_pairs", []) if isinstance(allocation, dict) else []
                
                evidence_parts = []
                if inc:
                    evidence_parts.append(f"**Tăng tỷ trọng**: {', '.join(inc)}")
                if dec:
                    evidence_parts.append(f"**Giảm tỷ trọng**: {', '.join(dec)}")
                if forex:
                    evidence_parts.append(f"**Khuyến nghị giao dịch Forex**: {', '.join(forex)}")
                if rwa:
                    evidence_parts.append("\n**Chi tiết chiến lược RWA/Tài sản cụ thể**:")
                    for item in rwa:
                        if isinstance(item, dict):
                            cat = item.get("category", "")
                            tokens = item.get("assets_or_tokens", [])
                            reason = item.get("reason", "")
                            evidence_parts.append(f"- **{cat}** ({', '.join(tokens)}): {reason}")

                # Cash Allocation: VND vs USD vs Stablecoin
                if isinstance(cash_alloc, dict):
                    evidence_parts.append("\n---\n**PHÂN BỔ TIỀN MẶT (VND vs USD vs Stablecoin)**:")
                    curr_dist = cash_alloc.get("currency_distribution", {})
                    if curr_dist:
                        dist_parts = []
                        for curr, pct in curr_dist.items():
                            dist_parts.append(f"{curr}: {pct*100:.0f}%")
                        evidence_parts.append(f"- **Tỷ lệ phân bổ**: {', '.join(dist_parts)}")
                    vn_rate = cash_alloc.get("vn_bank_interest_rate", "")
                    if vn_rate:
                        evidence_parts.append(f"- **Lãi suất NH VN**: {vn_rate}")
                    stable_yields = cash_alloc.get("stablecoin_platform_yields", [])
                    if stable_yields:
                        evidence_parts.append("- **Lợi suất Stablecoin USD trên các sàn**:")
                        for y in stable_yields:
                            evidence_parts.append(f"  - {y}")
                    cash_rec = cash_alloc.get("recommendation", "")
                    if cash_rec:
                        evidence_parts.append(f"- **Khuyến nghị**: {cash_rec}")

                # Real Estate VN
                if isinstance(re_vn, dict):
                    evidence_parts.append("\n---\n**BẤT ĐỘNG SẢN VIỆT NAM**:")
                    outlook = re_vn.get("market_outlook", "")
                    if outlook:
                        evidence_parts.append(f"- **Tổng quan thị trường**: {outlook}")
                    segs = re_vn.get("attractive_segments", [])
                    if segs:
                        evidence_parts.append(f"- **Phân khúc hấp dẫn**: {', '.join(segs)}")
                    
                    # Chi tiet cac de xuat BDS cu the
                    recommended = re_vn.get("recommended_properties", [])
                    if recommended:
                        evidence_parts.append("- **Đề xuất cụ thể**:")
                        for i, prop in enumerate(recommended, 1):
                            if isinstance(prop, dict):
                                ptype = prop.get("property_type", "")
                                area = prop.get("area", "")
                                project = prop.get("project", "")
                                price = prop.get("price_range", "")
                                reason = prop.get("reason", "")
                                line = f"  {i}. **{ptype}** - {area}"
                                if project:
                                    line += f" ({project})"
                                if price:
                                    line += f" - Giá: {price}"
                                evidence_parts.append(line)
                                if reason:
                                    evidence_parts.append(f"     *Lý do: {reason}*")
                    
                    risks = re_vn.get("risks", [])
                    if risks:
                        evidence_parts.append(f"- **Rủi ro**: {', '.join(risks)}")
                    re_rec = re_vn.get("recommendation", "")
                    if re_rec:
                        evidence_parts.append(f"- **Khuyến nghị**: {re_rec}")
                
                evidence = "\n".join(evidence_parts)
                
                cur.execute(
                    "INSERT INTO osint_theses (id, thesis, confidence, supporting_evidence, status) VALUES (%s, %s, %s, %s, %s)",
                    (t_id, thesis_text, conf, evidence, 'active')
                )
            conn.commit()
            _last_thesis_time = time.time()
            logger.info("Successfully updated theses in DB!")
        else:
            logger.warning("Failed to generate theses from AI")
            
        cur.close()
        conn.close()
    except Exception as e:
        logger.error(f"Error in run_thesis_update: {e}")

def run_world_state_update():
    global _last_world_state_time

    if not is_ai_enabled():
        logger.info("AI features disabled. Skipping world state update.")
        return

    # Check if there are new signals since last world state update
    if _last_world_state_time > 0:
        new_signal_count = get_new_signals_since(_last_world_state_time)
        if new_signal_count == 0:
            logger.info("No new signals since last world state update. Skipping.")
            return
        logger.info(f"{new_signal_count} new signals since last world state update. Updating world state...")
    else:
        logger.info("First world state update run.")

    logger.info("Running world state update job...")
    try:
        from agents.gemini_client import propose_world_state_changes
        db_url = os.getenv("DATABASE_URL")
        if not db_url: return
            
        conn = psycopg2.connect(db_url)
        cur = conn.cursor()
        
        cur.execute("SELECT state_json FROM osint_world_state WHERE id = 1")
        state_row = cur.fetchone()
        state_raw = state_row[0] if state_row and state_row[0] else "{}"
        if isinstance(state_raw, dict):
            state_dict = state_raw
            state_str = json.dumps(state_dict)
        else:
            state_str = state_raw
            try:
                state_dict = json.loads(state_str)
            except json.JSONDecodeError:
                state_dict = {}
        
        cur.execute("SELECT category, signal, reason FROM osint_signals ORDER BY created_at DESC LIMIT 20")
        sig_rows = cur.fetchall()
        signals_text = "\n".join([f"[{r[0]}] {r[1]} - {r[2]}" for r in sig_rows])
        
        cur.execute("SELECT thesis, supporting_evidence FROM osint_theses WHERE status = 'active' LIMIT 5")
        thes_rows = cur.fetchall()
        theses_text = "\n".join([f"- {r[0]}: {r[1]}" for r in thes_rows])
        
        result = propose_world_state_changes(state_str, signals_text, theses_text)
        
        if result and "proposed_changes" in result:
            current_time = time.strftime('%Y-%m-%dT%H:%M:%S+07:00')
            for p in result["proposed_changes"]:
                p_id = str(uuid.uuid4())
                tgt = p.get("target_entity", "General")
                fld = p.get("field_name", "trend")
                val = str(p.get("new_value", ""))
                conf = float(p.get("confidence", 0.5))
                reason = p.get("reason", "")
                
                cur.execute(
                    "INSERT INTO osint_proposed_changes (id, target_entity, field_name, new_value, confidence, reason, status) VALUES (%s, %s, %s, %s, %s, %s, 'approved')",
                    (p_id, tgt, fld, val, conf, reason)
                )
                
                if tgt not in state_dict:
                    state_dict[tgt] = {}
                state_dict[tgt][fld] = val
                state_dict[tgt]['_updated_at'] = current_time
            
            # Ensure ALL entities have _updated_at
            for entity_name in state_dict:
                if isinstance(state_dict[entity_name], dict) and '_updated_at' not in state_dict[entity_name]:
                    state_dict[entity_name]['_updated_at'] = current_time
                
            new_state_json = json.dumps(state_dict)
            cur.execute("""
                INSERT INTO osint_world_state (id, state_json, updated_at)
                VALUES (1, %s, CURRENT_TIMESTAMP)
                ON CONFLICT (id) DO UPDATE
                SET state_json = EXCLUDED.state_json, updated_at = CURRENT_TIMESTAMP
            """, (new_state_json,))
                
            conn.commit()
            _last_world_state_time = time.time()
            logger.info("Successfully proposed and automatically approved world state changes!")
            
        cur.close()
        conn.close()
    except Exception as e:
        logger.error(f"Error in run_world_state_update: {e}")

def adaptive_extraction_scheduler(scheduler):
    """
    Dynamically adjust extraction interval based on unprocessed news count.
    - 0 unprocessed: no-op (skip)
    - 1-5 unprocessed: every 20 minutes
    - 6-15 unprocessed: every 10 minutes (busy)
    - 16+ unprocessed: every 5 minutes (burst mode)
    """
    unprocessed = get_unprocessed_news_count()
    
    if unprocessed == 0:
        # Remove any existing dynamic job and schedule relaxed interval
        try:
            scheduler.remove_job('adaptive_extraction')
        except:
            pass
        scheduler.add_job(run_signal_extraction, 'interval', minutes=20, id='adaptive_extraction', replace_existing=True)
        logger.info("Adaptive extraction: 0 unprocessed -> every 20 min")
    elif unprocessed <= 5:
        try:
            scheduler.remove_job('adaptive_extraction')
        except:
            pass
        scheduler.add_job(run_signal_extraction, 'interval', minutes=10, id='adaptive_extraction', replace_existing=True)
        logger.info(f"Adaptive extraction: {unprocessed} unprocessed -> every 10 min")
    elif unprocessed <= 15:
        try:
            scheduler.remove_job('adaptive_extraction')
        except:
            pass
        scheduler.add_job(run_signal_extraction, 'interval', minutes=5, id='adaptive_extraction', replace_existing=True)
        logger.info(f"Adaptive extraction: {unprocessed} unprocessed -> every 5 min")
    else:
        try:
            scheduler.remove_job('adaptive_extraction')
        except:
            pass
        scheduler.add_job(run_signal_extraction, 'interval', minutes=3, id='adaptive_extraction', replace_existing=True)
        logger.info(f"Adaptive extraction: {unprocessed} unprocessed (BURST) -> every 3 min")

def cleanup_old_news():
    logger.info("Running database cleanup job...")
    try:
        db_url = os.getenv("DATABASE_URL")
        if not db_url: return
            
        conn = psycopg2.connect(db_url)
        cur = conn.cursor()
        
        # Delete news_items older than 14 days
        cur.execute("DELETE FROM news_items WHERE created_at < NOW() - INTERVAL '14 days'")
        deleted_count = cur.rowcount
        
        conn.commit()
        cur.close()
        conn.close()
        logger.info(f"Successfully deleted {deleted_count} old news items.")
    except Exception as e:
        logger.error(f"Error in cleanup_old_news: {e}")

class TriggerHandler(BaseHTTPRequestHandler):
    def do_POST(self):
        if self.path == '/trigger-thesis-update':
            logger.info("Manual trigger received for thesis and world state update")
            try:
                run_signal_extraction()
                run_thesis_update()
                run_world_state_update()
                
                self.send_response(200)
                self.send_header('Content-type', 'application/json')
                self.send_header('Access-Control-Allow-Origin', '*')
                self.end_headers()
                self.wfile.write(json.dumps({"status": "success", "message": "Cập nhật nhận định vĩ mô và trạng thái vĩ mô thành công!"}).encode('utf-8'))
            except Exception as e:
                logger.error(f"Error in manual trigger: {e}")
                self.send_response(500)
                self.send_header('Content-type', 'application/json')
                self.send_header('Access-Control-Allow-Origin', '*')
                self.end_headers()
                self.wfile.write(json.dumps({"status": "error", "message": str(e)}).encode('utf-8'))
        else:
            self.send_response(404)
            self.end_headers()
            
    def do_OPTIONS(self):
        self.send_response(200)
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'POST, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type')
        self.end_headers()

def start_trigger_server():
    server = HTTPServer(('0.0.0.0', 8081), TriggerHandler)
    logger.info("Trigger HTTP server started on port 8081")
    server.serve_forever()

if __name__ == "__main__":
    logger.info("Starting OSINT AI Worker with Adaptive Scheduling...")
    
    scheduler = BackgroundScheduler()
    
    # Signal extraction: adaptive (starts at every 10 min, auto-adjusts)
    scheduler.add_job(run_signal_extraction, 'interval', minutes=10, id='signal_extraction')
    
    # Adaptive interval adjuster: runs every 5 minutes to re-evaluate news volume
    scheduler.add_job(lambda: adaptive_extraction_scheduler(scheduler), 'interval', minutes=5, id='adaptive_controller')
    
    # Thesis update every 4 hours (auto-skips if no new signals)
    scheduler.add_job(run_thesis_update, 'interval', hours=4, id='thesis_update')
    
    # World state update every 4 hours (auto-skips if no new signals)
    scheduler.add_job(run_world_state_update, 'interval', hours=4, id='world_state_update')
    
    # Cleanup daily at 2 AM
    scheduler.add_job(cleanup_old_news, 'cron', hour=2, minute=0)
    
    scheduler.start()
    
    # Start trigger HTTP server for manual updates
    trigger_thread = threading.Thread(target=start_trigger_server, daemon=True)
    trigger_thread.start()
    
    # Run once at startup to populate initial data
    logger.info("Running initial jobs at startup...")
    run_signal_extraction()
    run_thesis_update()
    run_world_state_update()
    
    # Normally we would start the Pyrogram client here as well, 
    # but Pyrogram has its own event loop. We can run Pyrogram in the main thread.
    current_dir = os.path.dirname(os.path.abspath(__file__))
    if current_dir not in sys.path:
        sys.path.insert(0, current_dir)

    try:
        from collectors.telegram_scraper import start_scraping
        start_scraping()
    except (KeyboardInterrupt, SystemExit):
        scheduler.shutdown()
        logger.info("Shutting down...")