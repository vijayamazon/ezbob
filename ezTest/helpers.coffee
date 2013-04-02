counter = 1

zeroPad = (num, places) ->
  zero = places - num.toString().length + 1
  Array(+(zero > 0 and zero)).join("0") + num

class Helpers
    constructor: (@casper, @url) ->
        this
    capture: (name) ->
        @casper.capture "img/#{zeroPad(counter++, 3)} #{name}"

    thenCapture: (name) ->
        @casper.then => @capture(name)

    clickAndCapture: (selector, filename) ->
        @casper.thenClick selector
        @casper.then =>
            @capture filename

    logOff: ->
        @casper.log "log off"
        @casper.open "#{@url}/Account/LogOff"

    setTestMode: ->
        @casper.then =>
            @casper.evaluate ->
                document.cookie = "istest=true;path=/"

    #waits untill ui is blocked
    waitUnblock: ->
        @casper.then =>
            @casper.waitWhileVisible ".blockOverlay"
	    
    log: (message)->
        @casper.log "\n\n#{message}\n", 'info'
	    
    info: (message)-> 
        @casper.log "\n\n#{message}\n", 'info'

    debug: (message)-> 
        @casper.log "\n\n#{message}\n", 'debug'

    warning: (message)-> 
        @casper.log "\n\n#{message}\n", 'warning'

    error: (message)-> 
        @casper.log "\n\n#{message}\n", 'error'
        
exports.create = (casper, url) ->
    new Helpers(casper, url)

