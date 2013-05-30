﻿root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.StrategySettingsModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/"

class EzBob.Underwriter.StrategySettingsView extends Backbone.Marionette.ItemView
    template: "#strategy-detail-settings"

    initialize: (options) ->
        @modelBinder = new Backbone.ModelBinder()
        @model = new EzBob.Underwriter.StrategySettingsModel
        @update()
        @

    bindings:
        EnableAutomaticRejection:   "select[name='enableAutomaticRejection']"
        EnableAutomaticApproval:    "select[name='enableAutomaticApproval']"
        LowCreditScore:             "input[name='lowCreditScore']"
        TotalAnnualTurnover:        "input[name='totalAnnualTurnover']"
        TotalThreeMonthTurnover:    "input[name='totalThreeMonthTurnover']"

    events:
        "click button[name='SaveBtn']":     "saveSettings"
        "click button[name='CancelBtn']":   "cancelSettings"

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
            @$el.find("select[name='enableAutomaticRejection'], select[name='enableAutomaticApproval'], input[name='lowCreditScore'], input[name='totalAnnualTurnover'], input[name='totalThreeMonthTurnover']").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"});
            @$el.find("button[name='SaveBtn'], button[name='CancelBtn']").hide();

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

    onClose: ->
        @modelBinder.unbind()