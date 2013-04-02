///<reference path="~/Content/js/./App/ezbob.clicktale.js" />

var EzBob = EzBob || {};
EzBob.CT = EzBob.CT || {};

EzBob.CT.bindShopToCT = function (shop, name) {
    shop.on('back', function () {
        EzBob.CT.recordEvent('ct:shop.back', name);
    });
    shop.on('completed', function () {
        EzBob.CT.recordEvent('ct:shop.completed', name);
    });

    EzBob.App.on("ct:shop.back", function (targetName) {
        if (name != targetName) {
            return;
        }
        shop.trigger('back');
    });

    EzBob.App.on("ct:shop.completed", function (targetName) {
        if (name != targetName) {
            return;
        }
        shop.trigger('completed');
    });
};