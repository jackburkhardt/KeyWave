var SocketPlugin = {
    getSocketLibrarySource: function() {
       var script = document.createElement("script");
       script.src = "https://cdn.socket.io/4.7.5/socket.io.min.js";
       document.head.appendChild(script);
    },
    socket: null,
    connect: function(url) {
        this.socket = io(url);
    },
    disconnect: function() {
        this.socket.disconnect();
    },
    emit: function(event, data) {
        this.socket.emit(event, data);
    },
    on: function(event, callback) {
        this.socket.on(event, callback);
    },
    off: function(event, callback) {
        this.socket.off(event, callback);
    },
    canYouHearMe: function() {
        console.log("Yes, I can hear you!");
    }
};

mergeInto(LibraryManager.library, SocketPlugin);

