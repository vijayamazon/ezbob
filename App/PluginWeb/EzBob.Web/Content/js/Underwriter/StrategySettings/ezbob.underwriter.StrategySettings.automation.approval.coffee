root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.SettingsApprovalModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/AutomationApproval"

class EzBob.Underwriter.SettingsApprovalView extends Backbone.Marionette.ItemView
    template: "#approval-settings-template"

    initialize: (options) ->
        @modelBinder = new Backbone.ModelBinder()
        @model.on "change reset", @render, @
        @update()
        @

    bindings:
        EnableAutomaticApproval:   "select[name='enableAutomaticApproval']"

    events:
        "click button[name='SaveApprovalSettings']":     "saveSettings"
        "click button[name='CancelApprovalSettings']":   "cancelSettings"

    saveSettings: ->
        @model.save()

    update: ->
        xhr = @model.fetch()
        xhr.done => @render()

    cancelSettings: ->
        @update()

    onRender: ->
        @modelBinder.bind @model, @el, @bindings
        if !$("body").hasClass("role-manager") 
            @$el.find("select[name='enableAutomaticApproval']").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"});
            @$el.find("button[name='SaveApprovalSettings'], button[name='CancelApprovalSettings']").hide();

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

    onClose: ->
        @modelBinder.unbind()
