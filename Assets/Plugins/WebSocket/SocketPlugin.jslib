var SocketPlugin = {
    disconnect: function() {
        window.dispatchEvent(new CustomEvent("disconnect"));
    },
    canYouHearMe: function() {
        window.dispatchEvent(new CustomEvent("canYouHearMe"));
    },
    sendSaveGame: function(slot, data){
        window.dispatchEvent(new CustomEvent("putSaveGame", {detail:{data: data}}));
    },
    sendPlayerEvent: function(data){
        window.dispatchEvent(new CustomEvent("playerEvent", {detail:{data: data}}));
    },
};

mergeInto(LibraryManager.library, SocketPlugin);

