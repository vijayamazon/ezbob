root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter || {};

class EzBob.Underwriter.RecentCustomersModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/Customers/GetRecentCustomers"

class EzBob.Underwriter.goToCustomerId extends Backbone.Marionette.ItemView
    initialize: ->
        Mousetrap.bind "ctrl+g"
        , =>
            @render()
            false
        @on "NotFound", @notFound

    template: ->
        recentCustomers = JSON.parse(localStorage.getItem('RecentCustomers'))
        allOptions = ''
        for customer in recentCustomers
            allOptions += '<option value="' + customer.Item1 + '">' + customer.Item2 + '</option>'

        el = $("<div id='go-to-template'/>").html("
            <input type='text' class='goto-customerId form-control' autocomplete='off'/>
            <br/>
            <label>Recent customers:</label>
            <select id='recentCustomers'class='selectheight form-control'>" + allOptions + "</select>
            <br/>
            <div class='error-place' style='color:red'></div>")
        $('body').append el
        return el

    ui:
        "input"      : ".goto-customerId"
        "select"     : "#recentCustomers"
        "template"   : "#go-to-template"
        "errorPlace" : ".error-place"
        
    onRender: ->            
        @dialog = EzBob.ShowMessage( @ui.template, "Customer ID?",( => @okTrigger()), "OK", null, "Cancel" )
        @ui.input.on "keydown", (e)=>@keydowned(e)
        @okBtn = $(".ok-button")
        @ui.input.autocomplete
            source: "#{gRootPath}Underwriter/Customers/FindCustomer"
            autoFocus: false
            minLength: 3
            delay: 500

    okTrigger: ->
        val = @ui.input.val()
        unless IsInt(val, true)
            val = val.substring(0, val.indexOf(','))
        unless IsInt(val, true)
            selectVal = @ui.select.val()
            unless IsInt(selectVal, true)
                @addError "Incorrect input"
                return false
            val = selectVal
        @checkCustomer(val)
        return false

    keydowned: (e)->
        @addError ""
        return if @okBtn.attr("disabled") is "disabled"
        return if $('.ui-autocomplete:visible').length != 0
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
                    @trigger "ok", id
                    @dialog.dialog "close"
                    break;
                when "Ok"
                    @trigger "ok", id
                    @dialog.dialog "close"
                    break;
        xhr.complete => @okBtn.removeAttr("disabled")
