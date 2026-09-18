// Cloudflare Worker — CORS proxy for Yahoo Finance + Petrolimex
// Deploy: wrangler deploy proxy-worker.js
// Set WORKER_URL in app config after deploy (e.g. https://your-worker.your-subdomain.workers.dev)

const ALLOWED_TARGETS = [
  'query1.finance.yahoo.com',
  'query2.finance.yahoo.com',
  'portals.petrolimex.com.vn',
];

export default {
  async fetch(request) {
    const url = new URL(request.url);
    const target = url.searchParams.get('url');
    if (!target) {
      return new Response('Missing ?url=', { status: 400 });
    }

    let targetUrl;
    try {
      targetUrl = new URL(target);
    } catch {
      return new Response('Invalid url', { status: 400 });
    }

    if (!ALLOWED_TARGETS.includes(targetUrl.hostname)) {
      return new Response('Target not allowed', { status: 403 });
    }

    try {
      const resp = await fetch(targetUrl.toString(), {
        method: request.method,
        headers: { 'User-Agent': 'Mozilla/5.0' },
      });

      const body = await resp.arrayBuffer();
      return new Response(body, {
        status: resp.status,
        headers: {
          'Content-Type': resp.headers.get('Content-Type') || 'application/json',
          'Access-Control-Allow-Origin': '*',
          'Cache-Control': 'public, max-age=300',
        },
      });
    } catch (err) {
      return new Response('Proxy error: ' + err.message, { status: 502 });
    }
  },
};