root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.GA
    trackPage:(url) ->
        if(_gaq?)
            _gaq.push(['_trackPageview', url])
        else
            console.log 'Track PageView: %s', url

