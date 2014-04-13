root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.GA
    trackPage:(url) ->
        _gaq.push(['_trackPageview', url])

class EzBob.GATest
    trackPage:(url) ->
        console.log 'Track PageView: %s', url
