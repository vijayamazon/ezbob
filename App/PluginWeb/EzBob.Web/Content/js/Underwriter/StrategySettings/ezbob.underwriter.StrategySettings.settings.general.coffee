root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.SettingsGeneralModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/SettingsGeneral"

class EzBob.Underwriter.SettingsGeneralView extends Backbone.Marionette.ItemView
    template: "#general-settings-template"

    initialize: (options) ->
        @modelBinder = new Backbone.ModelBinder()
        @model.on "reset", @render, @
        @update()
        @

    bindings:
        BWABusinessCheck:   "select[name='bwaBusinessCheck']"
        #DisplayEarnedPoints:    "select[name='displayEarnedPoints']"

    events:
        "click button[name='SaveGeneralSettings']":     "saveSettings"
        "click button[name='CancelGeneralSettings']":   "cancelSettings"

    saveSettings: ->
        BlockUi "on"
        @model.save().done ->  EzBob.ShowMessage  "Saved successfully", "Successful"
        @model.save().complete -> BlockUi "off"
        @model.save()
        false

    update: ->
        xhr = @model.fetch()
        xhr.done => @render()

    cancelSettings: ->
        @update()

    onRender: ->
        @modelBinder.bind @model, @el, @bindings
        if !$("body").hasClass("role-manager") 
            #@$el.find("select[name='bwaBusinessCheck'], select[name='displayEarnedPoints']").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"});
            @$el.find("button[name='SaveGeneralSettings'], button[name='CancelGeneralSettings']").hide();

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

    onClose: ->
        @modelBinder.unbind()
