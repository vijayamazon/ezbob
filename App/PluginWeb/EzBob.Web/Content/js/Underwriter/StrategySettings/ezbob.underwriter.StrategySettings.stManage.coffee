root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.StrategySettingsModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/"

class EzBob.Underwriter.StrategySettingsView extends Backbone.Marionette.ItemView
    template: "#strategy-detail-settings"

    initialize: (options) ->
        @modelBinder = new Backbone.ModelBinder()
        @model = new EzBob.Underwriter.StrategySettingsModel
        xhr = @model.fetch()
        xhr.done => @render()
        #@bind 'change', @model.save()
        @

    bindings:
        EnableAutomaticRejection: "select[name='enableAutomaticRejection']"
        EnableAutomaticApproval: "select[name='enableAutomaticApproval']"
        LowCreditScore: "input[name='lowCreditScore']"
        TotalAnnualTurnover: "input[name='totalAnnualTurnover']"
        TotalThreeMonthTurnover: "input[name='totalThreeMonthTurnover']"
        EnableAutomaticRejectionDesc: "td[name='enableAutomaticRejectionDesc']"
        EnableAutomaticApprovalDesc: "td[name='enableAutomaticApprovalDesc']"
        LowCreditScoreDesc: "td[name='lowCreditScoreDesc']"
        TotalAnnualTurnoverDesc: "td[name='totalAnnualTurnoverDesc']"
        TotalThreeMonthTurnoverDesc: "td[name='totalThreeMonthTurnoverDesc']"

    events:
        "click button[name='SaveBtn']": "saveSettings"
        "click button[name='CancelBtn']": "cancelSettings"

    saveSettings: ->
        @model.save()

    cancelSettings: ->
        xhr = @model.fetch()
        xhr.done => @render()


    onRender: -> 
        @modelBinder.bind @model, @el, @bindings

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

    onClose: ->
        @modelBinder.unbind()