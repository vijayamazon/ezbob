root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter || {};

class EzBob.Underwriter.goToCustomerId extends Backbone.Marionette.ItemView
    initialize: ->
        Mousetrap.bind "ctrl+g"
        , =>
            @render()
            false

    template: ->
        $("<input type='text' class='goto-customerId'/>")

    ui:
        "input":".goto-customerId"

    onRender: ->
        @dialog = EzBob.ShowMessage(
            @ui.input
            , "Customer ID?"
            , => 
                @okTrigger()
            , "OK", null, "Cancel")
        @ui.input.numericOnly()
        @ui.input.on "keydown"
            , (e)=>
                @keydowned(e)

    okTrigger: ->
        val = @ui.input.val()
        unless IsInt(val, true)
            EzBob.ShowMessage "Incorrect input", "Warning"
            return false
        @trigger "ok", val

    keydowned: (e)->
        if e.keyCode == 13
            @okTrigger()
            @dialog.dialog "close"
