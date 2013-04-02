window.lpTag = {
    site: '13658382',
    lpSrv: 'server.iad.liveperson.net',
    _v: '1.0',
    protocol: location.protocol,
    events: {
        bind: function (app, ev, fn) {
            lpTag.defer(function () {
                lpTag.events.bind(app, ev, fn)
            })
        },
        trigger: function (app, ev, json) {
            lpTag.defer(function () {
                lpTag.events.trigger(app, ev, json)
            })
        }
    },
    defer: function (fn) {
        this._defL = this._defL || [];
        this._defL.push(fn)
    },
    load: function (src, chr, id) {
        if (!src) {
            src = this.protocol + '//' + ((this.ovr && this.ovr.domain) ? this.ovr.domain : 'lptag.liveperson.net') + '/tag/tag.js?site=' + this.site
        }
        var s = document.createElement('script');
        s.setAttribute('charset', chr ? chr : 'UTF-8');
        if (id) {
            s.setAttribute('id', id)
        }
        s.setAttribute('src', src);
        document.getElementsByTagName('head').item(0).appendChild(s)
    },
    init: function () {
        this._timing = this._timing || {};
        this._timing.start = (new Date()).getTime();
        var that = this;
        if (window.attachEvent) {
            window.attachEvent('onload', function () {
                that._domReady('domReady')
            })
        } else {
            window.addEventListener('DOMContentLoaded', function () {
                that._domReady('contReady')
            }, false);
            window.addEventListener('load', function () {
                that._domReady('domReady')
            }, false)
        }
        if (typeof (_lptStop) == 'undefined') {
            this.load()
        }
    },
    _domReady: function (n) {
        this.isDom = true;
        this._timing[n] = (new Date()).getTime();
        this.events.trigger('LPT', 'DOM_READY')
    }
};
lpTag.init();