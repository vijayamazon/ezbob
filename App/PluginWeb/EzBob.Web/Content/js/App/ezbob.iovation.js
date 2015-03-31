var EzBob = EzBob || {};

EzBob.Iovation = (function () {
    function Iovation() { }

    Iovation.prototype.io_sent = false;

    Iovation.io_send_bb = function (blackbox, origin) {
        // function to process the blackbox
        if (Iovation.io_sent)
            return;
        $.post(window.gRootPath + "Account/Iovation", { 'blackbox': blackbox, 'origin' : origin });
        Iovation.io_sent = true;
    };

    Iovation.prototype.callIovation = function (origin) {
        console.log('callIovation');
        setTimeout(function () {
            try {
                var bb_info = window.ioGetBlackbox();
                Iovation.io_send_bb(bb_info.blackbox, origin);
                return;
            } catch (e) {
            }
            Iovation.io_send_bb("", origin);
            // if we are done but got an error,
            // send an empty blackbox
        }, 5000);
    };

    return Iovation;
})();



