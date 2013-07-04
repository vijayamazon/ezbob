root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.SettingsApprovalModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/AutomationApproval"

class EzBob.Underwriter.SettingsApprovalView extends Backbone.Marionette.ItemView
    template: "#approval-settings-template"

    initialize: (options) ->
        @modelBinder = new Backbone.ModelBinder()
        @model.on "reset", @render, @
        @update()
        @

    bindings:
        EnableAutomaticApproval:                "select[name='enableAutomaticApproval']"
        EnableAutomaticReRejection:             "select[name='enableAutomaticReRejection']"
        
        AutoRejectionException_CreditScore:     "input[name='autoRejectionException_CreditScore']"
        AutoRejectionException_AnualTurnover:   "input[name='autoRejectionException_AnualTurnover']"
        MaxCapHomeOwner:                        "input[name='maxCapHomeOwner']"
        MaxCapNotHomeOwner:                     "input[name='maxCapNotHomeOwner']"


    events:
        "click button[name='SaveApprovalSettings']":     "saveSettings"
        "click button[name='CancelApprovalSettings']":   "cancelSettings"

    saveSettings: ->
        return unless @validator.form()
        BlockUi "on"
        @model.save().done ->  EzBob.ShowMessage  "Saved successfully", "Successful"
        @model.save().complete -> BlockUi "off"
        false

    update: ->
        xhr = @model.fetch()
        xhr.done => @render()

    cancelSettings: ->
        @update()
        false

    onRender: ->
        console.log "Render is Set"
        @modelBinder.bind @model, @el, @bindings
        if !$("body").hasClass("role-manager") 
            @$el.find(" select[name='enableAutomaticApproval'], 
                        select[name='enableAutomaticReRejection'], 
                        input[name='autoRejectionException_CreditScore'], 
                        input[name='autoRejectionException_AnualTurnover'],
                        input[name='maxCapHomeOwner'],
                        input[name='maxCapNotHomeOwner']").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"});
            @$el.find("button[name='SaveApprovalSettings'], button[name='CancelApprovalSettings']").hide();
        @setValidator()
        console.log @validator.settings.rules


    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

    onClose: ->
        @modelBinder.unbind()

     setValidator: ->
        console.log "Validator is set"
        @validator = @$el.find('form').validate
            onfocusout: -> return true
            onkeyup: -> return false
            onclick: -> return false
            rules:
                autoRejectionException_CreditScore:
                    required: true
                    min: 0
                autoRejectionException_AnualTurnover:
                    required: true
                    min: 0
                maxCapHomeOwner:
                    required: true
                    min: 0
                maxCapNotHomeOwner:
                    required: true
                    min: 0