root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.SettingsRejectionModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/AutomationRejection"

class EzBob.Underwriter.SettingsRejectionView extends Backbone.Marionette.ItemView
    template: "#rejection-settings-template"

    initialize: (options) ->
        @modelBinder = new Backbone.ModelBinder()
        @model.on "change reset", @render, @
        @update()
        @

    bindings:
        EnableAutomaticRejection:   "select[name='enableAutomaticRejection']"
        LowCreditScore:             "input[name='lowCreditScore']"
        TotalAnnualTurnover:        "input[name='totalAnnualTurnover']"
        TotalThreeMonthTurnover:    "input[name='totalThreeMonthTurnover']"
        Reject_Defaults_CreditScore:"input[name='reject_Defaults_CreditScore']"
        Reject_Defaults_AccountsNum:"input[name='reject_Defaults_AccountsNum']"
        Reject_Defaults_Amount:     "input[name='reject_Defaults_Amount']"
        Reject_Defaults_MonthsNum:  "input[name='reject_Defaults_MonthsNum']"



    events:
        "click button[name='SaveRejectionSettings']":     "saveSettings"
        "click button[name='CancelRejectionSettings']":   "cancelSettings"

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
            @$el.find("select[name='enableAutomaticRejection'], input[name='lowCreditScore'], input[name='totalAnnualTurnover'], input[name='totalThreeMonthTurnover'], input[name='reject_Defaults_CreditScore'], input[name='reject_Defaults_AccountsNum'], input[name='reject_Defaults_Amount'], input[name='reject_Defaults_MonthsNum']").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"});
            @$el.find("button[name='SaveRejectionSettings'], button[name='CancelRejectionSettings']").hide();

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

    onClose: ->
        @modelBinder.unbind()
