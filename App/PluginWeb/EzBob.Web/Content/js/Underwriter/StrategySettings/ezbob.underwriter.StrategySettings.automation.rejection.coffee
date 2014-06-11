root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.SettingsRejectionModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/AutomationRejection"

class EzBob.Underwriter.SettingsRejectionView extends Backbone.Marionette.ItemView
    template: "#rejection-settings-template"

    initialize: (options) ->
        @modelBinder = new Backbone.ModelBinder()
        @model.on "reset", @render, @
        @update()
        @

    bindings:
        EnableAutomaticRejection:               "select[name='EnableAutomaticRejection']"
        LowCreditScore:                         "input[name='LowCreditScore']"
        TotalAnnualTurnover:                    "input[name='TotalAnnualTurnover']"
        TotalThreeMonthTurnover:                "input[name='TotalThreeMonthTurnover']"
        Reject_Defaults_CreditScore:            "input[name='Reject_Defaults_CreditScore']"
        Reject_Defaults_AccountsNum:            "input[name='Reject_Defaults_AccountsNum']"
        Reject_Defaults_Amount:                 "input[name='Reject_Defaults_Amount']"
        Reject_Defaults_MonthsNum:              "input[name='Reject_Defaults_MonthsNum']"
        Reject_Minimal_Seniority:               "input[name='Reject_Minimal_Seniority']"
        EnableAutomaticReRejection:             "select[name='EnableAutomaticReRejection']"
        AutoRejectionException_CreditScore:     "input[name='AutoRejectionException_CreditScore']"
        AutoRejectionException_AnualTurnover:   "input[name='AutoRejectionException_AnualTurnover']"
        Reject_LowOfflineAnnualRevenue:         "input[name='Reject_LowOfflineAnnualRevenue']"
        Reject_LowOfflineQuarterRevenue:        "input[name='Reject_LowOfflineQuarterRevenue']"
        Reject_LateLastMonthsNum:               "input[name='Reject_LateLastMonthsNum']"
        Reject_NumOfLateAccounts:               "input[name='Reject_NumOfLateAccounts']"


    events:
        "click button[name='SaveRejectionSettings']":     "saveSettings"
        "click button[name='CancelRejectionSettings']":   "cancelSettings"

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
        @modelBinder.bind @model, @el, @bindings
        if !$("body").hasClass("role-manager") 
            @$el.find("select, input").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"});
            @$el.find("button").hide();
        @setValidator()

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

    onClose: ->
        @modelBinder.unbind()

     setValidator: ->
        @validator = @$el.find('form').validate
            onfocusout: -> return true
            onkeyup: -> return false
            onclick: -> return false
            rules:
                LowCreditScore:
                    required: true
                    min: 0
                TotalAnnualTurnover:
                    required: true
                    min: 0
                TotalThreeMonthTurnover:
                    required: true
                    min: 0
                Reject_Defaults_CreditScore:
                    required: true
                    min: 0
                Reject_Defaults_AccountsNum:
                    required: true
                    min: 0
                Reject_Defaults_Amount:
                    required: true
                    min: 0
                Reject_Defaults_MonthsNum:
                    required: true
                    min: 0
                Reject_Minimal_Seniority:
                    required: true
                    min: 0
                AutoRejectionException_CreditScore:
                    required: true
                    min: 0
                AutoRejectionException_AnualTurnover:
                    required: true
                    min: 0
                Reject_LowOfflineAnnualRevenue:
                    required: true
                    min: 0
                Reject_LowOfflineQuarterRevenue:
                    required: true
                    min: 0
                Reject_LateLastMonthsNum:
                    required: true
                    min: 0
                Reject_NumOfLateAccounts:
                    required: true
                    min: 0
