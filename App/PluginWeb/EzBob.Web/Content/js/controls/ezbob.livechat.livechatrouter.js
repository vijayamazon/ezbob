var EzBob = EzBob || {};
EzBob.LiveChat = EzBob.LiveChat || {};


EzBob.LiveChat.LiveChatRouter = Backbone.Router.extend({
    routes: {
        "LiveChat": "liveChat"
    },
    liveChat: function () {
        tm();
        function tm() {
            if ($('.lpchat-label').length > 0) {
                console.log('clicking');
                $('.lpchat-label').click();
            } else {
                window.setTimeout(tm, 1000);
            }
        }
    }
});