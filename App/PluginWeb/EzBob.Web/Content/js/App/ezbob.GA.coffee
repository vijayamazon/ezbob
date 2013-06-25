root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.GA
    trackPage:(url) ->
        _gaq.push(['_trackPageview', url]);
    trackEvent:(category, action, opt_label, opt_value, opt_noninteraction) ->
        _gaq.push(['_trackPageview', category, action, opt_label, opt_value, opt_noninteraction]);

class EzBob.GATest
    trackPage:(url) ->
        console.log 'Track PageView: %s', url
    trackEvent:(category, action, opt_label, opt_value, opt_noninteraction) ->
        console.log "Track Event: #{category}:#{action}:#{opt_label}"