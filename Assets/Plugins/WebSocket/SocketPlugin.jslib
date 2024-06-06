var SocketPlugin = {
    disconnect: function() {
        window.dispatchEvent(new CustomEvent("disconnect"));
    },
    canYouHearMe: function() {
        console.log("Yes, I can hear you!");
    },
    sendSaveGame: function(slot, data){
        window.dispatchEvent(new CustomEvent("putSaveGame", {slot: slot, data: data}));
    },
    saveGameExists: function(slot){
        window.dispatchEvent(new CustomEvent("saveGameExists", {slot: slot}));
    },
    getSaveGame: function(slot){
        window.dispatchEvent(new CustomEvent("getSaveGame", {slot: slot}));
    },
    sendPlayerEvent: function(data){
        window.dispatchEvent(new CustomEvent("playerEvent", {data: data}));
    },
};

mergeInto(LibraryManager.library, SocketPlugin);

