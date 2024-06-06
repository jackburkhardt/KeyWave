var SocketPlugin = {
    disconnect: function() {
        window.dispatchEvent(new CustomEvent("disconnect"));
    },
    canYouHearMe: function() {
        window.dispatchEvent(new CustomEvent("canYouHearMe"));
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

