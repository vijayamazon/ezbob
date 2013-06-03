root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter || {};

class EzBob.Underwriter.goToCustomerId extends Backbone.Marionette.ItemView
    initialize: ->
        Mousetrap.bind "ctrl+g"
        , =>
            @render()
            false
        @on "NotFound", @notFound

    template: ->
        $("<div id='go-to-template'/>").html("
        <input type='text' class='goto-customerId'/>
        <br/>
        <div class='error-place' style='color:red'></div>")

    ui:
        "input"      : ".goto-customerId"
        "template"   : "#go-to-template"
        "errorPlace" : ".error-place"
        
    onRender: ->
        @dialog = EzBob.ShowMessage(
            @ui.template
            , "Customer ID?"
            , => 
                @okTrigger()
            , "OK", null, "Cancel")
        @ui.input.numericOnly()
        @ui.input.on "keydown", (e)=>@keydowned(e)
        @okBtn = $(".ok-button")

    okTrigger: ->
        val = @ui.input.val()
        unless IsInt(val, true)
            @addError "Incorrect input"
            return false
        @checkCustomer(val)
        return false

    keydowned: (e)->
        return if @okBtn.attr("disabled") is "disabled"
        if e.keyCode == 13
            @okTrigger()

    addError: (val)->
        @ui.errorPlace.text(val)

    checkCustomer: (id)->
        @okBtn.attr("disabled","disabled")
        xhr = $.get "#{window.gRootPath}Underwriter/Customers/CheckCustomer?customerId=#{id}"
        xhr.done (res)=>
            switch res.State
                when "NotFound" 
                    @addError "Customer id. ##{id} was not found"
                    break;
                when "NotSuccesfullyRegistred" 
                    @addError "Customer id ##{id} not successfully registered"
                    break;
                when "Ok"
                    @trigger "ok", id
                    @dialog.dialog "close"
                    break;
        xhr.complete => @okBtn.removeAttr("disabled")
