// Cloudflare Worker relay for TCBS equity websockets (5.2 / 5.9 / 5.10).
// Browser connects with a one-minute ticket. The worker redeems it at broker-api
// and never returns the TCBS JWT to the browser.
//
// Deploy from this folder, separate from the Yahoo proxy:
//   npx wrangler deploy --config wrangler.tcbs-relay.toml
//   npx wrangler secret put TCBS_RELAY_SECRET --config wrangler.tcbs-relay.toml
// BROKER_API is a plain var in wrangler.tcbs-relay.toml.
// TCBS_RELAY_SECRET must match the Vercel broker-api env of the same name.

export default {
  async fetch(request, env) {
    const upgrade = request.headers.get("Upgrade") || "";
    if (upgrade.toLowerCase() !== "websocket") {
      return new Response("TCBS websocket relay", { status: 200 });
    }
    const ticket = new URL(request.url).searchParams.get("ticket");
    if (!ticket) return new Response("Missing ticket", { status: 400 });

    const broker = String(env?.BROKER_API || "").replace(/\/$/, "");
    const secret = String(env?.TCBS_RELAY_SECRET || "");
    if (!broker || !secret) return new Response("Relay is not configured", { status: 500 });

    const redeemed = await fetch(`${broker}/api/tcbs/ws-session/${encodeURIComponent(ticket)}`, {
      headers: { "X-Relay-Secret": secret },
    });
    if (!redeemed.ok) return new Response("Ticket rejected", { status: 401 });
    const session = await redeemed.json();
    if (!session.url || !session.token) return new Response("Upstream missing", { status: 502 });

    const pair = new WebSocketPair();
    const client = pair[0];
    const server = pair[1];
    server.accept();

    let upstream;
    try {
      upstream = new WebSocket(session.url, { headers: { Authorization: `Bearer ${session.token}` } });
    } catch (err) {
      server.close(1011, "upstream");
      return new Response("Upstream failed", { status: 502 });
    }

    const pending = [];
    upstream.addEventListener("open", () => {
      while (pending.length) upstream.send(pending.shift());
    });
    upstream.addEventListener("message", (ev) => {
      if (server.readyState === WebSocket.OPEN) server.send(ev.data);
    });
    upstream.addEventListener("close", () => {
      try { server.close(); } catch { /* ignore */ }
    });
    server.addEventListener("message", (ev) => {
      if (upstream.readyState === WebSocket.OPEN) upstream.send(ev.data);
      else pending.push(ev.data);
    });
    server.addEventListener("close", () => {
      try { upstream.close(); } catch { /* ignore */ }
    });

    return new Response(null, { status: 101, webSocket: client });
  },
};
