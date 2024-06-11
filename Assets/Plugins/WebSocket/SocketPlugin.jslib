var SocketPlugin = {
    disconnect: function() {
        window.dispatchEvent(new CustomEvent("disconnect"));
    },
    canYouHearMe: function() {
        window.dispatchEvent(new CustomEvent("canYouHearMe"));
    },
    sendSaveGame: function(slot, data){
        window.dispatchEvent(new CustomEvent("putSaveGame", {detail: {slot: slot, data: data}}));
    },
    getOccupiedSaveSlots: function(){
        window.dispatchEvent(new CustomEvent("getOccupiedSaveSlots"));
    },
    getSaveGame: function(slot){
        window.dispatchEvent(new CustomEvent("getSaveGame", {detail:{slot: slot}}));
    },
    sendPlayerEvent: function(data){
        window.dispatchEvent(new CustomEvent("playerEvent", {detail:{data: data}}));
    },
};

mergeInto(LibraryManager.library, SocketPlugin);

