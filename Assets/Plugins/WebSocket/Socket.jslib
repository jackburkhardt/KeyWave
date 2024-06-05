var SocketPlugin = {
    getSocketLibrarySource: function() {
       var script = document.createElement("script");
       script.src = "https://cdn.socket.io/4.7.5/socket.io.min.js";
       document.head.appendChild(script);
    },
    socket: null,
    userID: null,
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
    },
    sendSaveGame: function(slot, data){
        this.emit("putSaveGame", {user: this.userID, slot: slot, data: data});
    },
    saveGameExists: function(slot){
        this.emit("saveGameExists", {user: this.userID, slot: slot}, (response) =>{
            return response;
        });
    },
    getSaveGame: function(slot){
        this.emit("getSaveGame", {user: this.userID, slot: slot}, (response) =>{
            return response;
        });
    },
    sendPlayerEvent: function(data){
        this.emit("playerEvent", {user: this.userID, data: data});
    },
};

mergeInto(LibraryManager.library, SocketPlugin);

