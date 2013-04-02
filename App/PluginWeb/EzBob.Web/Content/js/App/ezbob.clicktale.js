var EzBob = EzBob || {};
EzBob.CT = EzBob.CT || {};

EzBob.CT.exec = function (body) {
    if (typeof ClickTaleExec == 'function') {
        ClickTaleExec(body);
    }
};

EzBob.CT.recordEvent = function (ev) {
    var args = _.filter(Array.prototype.slice.call(arguments, 1), function (x) { return !_.isUndefined(x);}),
        stringArgs = _.map(args, function (a) {
            if (typeof a == "number") return a;
            return "'" + a.toString().replace("'", "\\'") + "'";
        }).join(',');
    EzBob.CT.exec('EzBob.App.trigger("' + ev + (stringArgs ? ('", ' + stringArgs) : ('"')) + ');');
};