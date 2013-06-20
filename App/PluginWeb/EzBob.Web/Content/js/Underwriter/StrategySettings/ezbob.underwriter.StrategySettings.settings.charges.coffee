root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.SettingsChargesModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/SettingsCharges"

class EzBob.Underwriter.SettingsChargesView extends Backbone.Marionette.ItemView
    template: "#charges-settings-template"

    initialize: (options) ->
        @modelBinder = new Backbone.ModelBinder()
        @model.on "change reset", @render, @
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
        @model.save()

    cancelSettings: ->
        @update()
    
    update: ->
        xhr = @model.fetch()
        xhr.done => @render()

    onRender: -> 
        @modelBinder.bind @model, @el, @bindings
        if !$("body").hasClass("role-manager") 
            @$el.find("input[name='latePaymentCharge'], input[name='rolloverCharge'], input[name='partialPaymentCharge'], input[name='administrationCharge'], input[name='otherCharge']").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"});
            @$el.find("button[name='SaveChargesSettings'], button[name='CancelChargesSettings']").hide();

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

    onClose: ->
        @modelBinder.unbind()
