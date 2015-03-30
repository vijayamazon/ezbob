var EzBob = EzBob || {};

EzBob.Iovation = (function () {
    function Iovation() { }

    Iovation.prototype.io_sent = false;

    Iovation.io_send_bb = function (blackbox) {
        console.log('io_send_bb');
        // function to process the blackbox
        if (Iovation.io_sent)
            return;
        console.log('blackbox', blackbox);
        $.post(window.gRootPath + "Account/Iovation", { 'blackbox': blackbox });
        io_sent = true;
    };

    Iovation.prototype.callIovation = function () {
        console.log('callIovation');
        setTimeout(function () {
            try {
                var bb_info = ioGetBlackbox();
                console.log('bb_info', bb_info);
                Iovation.io_send_bb(bb_info.blackbox);
                return;
            } catch (e) {
            }
            Iovation.io_send_bb("");
            // if we are done but got an error,
            // send an empty blackbox
        }, 5000);
    };

    return Iovation;
})();



