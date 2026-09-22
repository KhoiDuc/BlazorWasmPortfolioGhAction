window.tcbsStream = {
  socket: null,
  connect: function (url, dotnetRef) {
    try {
      if (this.socket) {
        this.socket.close();
        this.socket = null;
      }
      const ws = new WebSocket(url);
      this.socket = ws;
      const beat = setInterval(function () {
        if (ws.readyState === WebSocket.OPEN) ws.send("d|p|||");
      }, 2000);
      ws.onmessage = function (ev) {
        dotnetRef.invokeMethodAsync("OnTick", String(ev.data || ""));
      };
      ws.onclose = function () { clearInterval(beat); };
      ws.onerror = function () { clearInterval(beat); };
      return true;
    } catch {
      return false;
    }
  },
  close: function () {
    if (this.socket) {
      this.socket.close();
      this.socket = null;
    }
  }
};
