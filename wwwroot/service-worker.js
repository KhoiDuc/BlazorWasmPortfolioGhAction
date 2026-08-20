// In development, always fetch from the network and do not enable offline support.
// Caching in development can prevent live code changes from reflecting immediately.
self.addEventListener('install', event => self.skipWaiting());
self.addEventListener('activate', event => event.waitUntil(self.clients.claim()));
self.addEventListener('fetch', event => { });
