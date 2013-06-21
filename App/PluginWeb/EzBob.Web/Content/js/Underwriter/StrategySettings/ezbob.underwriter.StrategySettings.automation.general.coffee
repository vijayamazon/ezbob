root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.SettingsAutomationModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/AutomationGeneral"

class EzBob.Underwriter.SettingsAutomationView extends Backbone.Marionette.ItemView
    template: "#automation-settings-template"

    initialize: (options) ->
        @modelBinder = new Backbone.ModelBinder()
        @model.on "change reset", @render, @
        @update()
        @

    events:
        "click button[name='SaveAutomationSettings']":     "saveSettings"
        "click button[name='CancelAutomationSettings']":   "cancelSettings"

    saveSettings: ->
        BlockUi "on"
        @model.save().done ->  EzBob.ShowMessage  "Saved successfully", "Successful"
        @model.save().complete -> BlockUi "off"
        @model.save()

    update: ->
        xhr = @model.fetch()
        xhr.done => @render()

    cancelSettings: ->
        @update()

    onRender: ->
        @modelBinder.bind @model, @el, @bindings
        if !$("body").hasClass("role-manager") 
            @$el.find("button[name='SaveAutomationSettings'], button[name='CancelAutomationSettings']").hide();

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

    onClose: ->
        @modelBinder.unbind()
