var SocketPlugin = {
    $helpers: {
        stringify: function (arg) { return (typeof UTF8ToString !== 'undefined' ? UTF8ToString : Pointer_stringify)(arg); },
    },
    disconnect: function() {
        window.dispatchEvent(new CustomEvent("disconnect"));
    },
    canYouHearMe: function() {
        window.dispatchEvent(new CustomEvent("canYouHearMe"));
    },
    sendSaveGame: function(data){
        var data_str = helpers.stringify(data);
        var data_json = JSON.parse(data_str);
        window.dispatchEvent(new CustomEvent("putSaveGame", {detail:{data: data_json}}));
    },
    sendPlayerEvent: function(data){
        var data_str = helpers.stringify(data);
        var data_json = JSON.parse(data_str);
        window.dispatchEvent(new CustomEvent("playerEvent", {detail:{data: data_json}}));
    },
    unityReadyForData: function(){
        window.dispatchEvent(new CustomEvent("unityReadyForData"));
    }
};

autoAddDeps(SocketPlugin, '$helpers');
mergeInto(LibraryManager.library, SocketPlugin);

