root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.SettingsChargesModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/SettingsCharges"

class EzBob.Underwriter.SettingsChargesView extends Backbone.Marionette.ItemView
    template: "#charges-settings-template"

    initialize: (options) ->
        @modelBinder = new Backbone.ModelBinder()
        @model.on "reset", @render, @
        @update()
        @

    bindings:
        LatePaymentCharge:      "input[name='latePaymentCharge']"
        RolloverCharge:         "input[name='rolloverCharge']"
        PartialPaymentCharge:   "input[name='partialPaymentCharge']"
        AdministrationCharge:   "input[name='administrationCharge']"
        OtherCharge:            "input[name='otherCharge']"

    events:
        "click button[name='SaveChargesSettings']":     "saveSettings"
        "click button[name='CancelChargesSettings']":   "cancelSettings"

    saveSettings: ->
        return unless @validator.form()
        BlockUi "on"
        @model.save().done ->  EzBob.ShowMessage  "Saved successfully", "Successful"
        @model.save().complete -> BlockUi "off"
        false

    cancelSettings: ->
        @update()
        false
    
    update: ->
        xhr = @model.fetch()
        xhr.done => @render()

    onRender: -> 
        @modelBinder.bind @model, @el, @bindings
        if !$("body").hasClass("role-manager") 
            @$el.find("input[name='latePaymentCharge'], input[name='rolloverCharge'], input[name='partialPaymentCharge'], input[name='administrationCharge'], input[name='otherCharge']").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"});
            @$el.find("button[name='SaveChargesSettings'], button[name='CancelChargesSettings']").hide();
        @setValidator()

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

    onClose: ->
        @modelBinder.unbind()

    setValidator: ->
        @validator = @$el.find('form').validate
            rules:
                latePaymentCharge:
                    required: true
                    min: 0
                rolloverCharge:
                    required: true
                    min: 0
                partialPaymentCharge:
                    required: true
                    min: 0
                administrationCharge:
                    required: true
                    min: 0
                otherCharge:
                    required: true
                    min: 0