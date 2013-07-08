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
        EnableAutomaticRejection:   "select[name='enableAutomaticRejection']"
        LowCreditScore:             "input[name='lowCreditScore']"
        TotalAnnualTurnover:        "input[name='totalAnnualTurnover']"
        TotalThreeMonthTurnover:    "input[name='totalThreeMonthTurnover']"
        Reject_Defaults_CreditScore:"input[name='reject_Defaults_CreditScore']"
        Reject_Defaults_AccountsNum:"input[name='reject_Defaults_AccountsNum']"
        Reject_Defaults_Amount:     "input[name='reject_Defaults_Amount']"
        Reject_Defaults_MonthsNum:  "input[name='reject_Defaults_MonthsNum']"
        EnableAutomaticReRejection:             "select[name='enableAutomaticReRejection']"
        AutoRejectionException_CreditScore:     "input[name='autoRejectionException_CreditScore']"
        AutoRejectionException_AnualTurnover:   "input[name='autoRejectionException_AnualTurnover']"


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
            @$el.find(" select[name='enableAutomaticRejection'], 
                        input[name='lowCreditScore'], 
                        input[name='totalAnnualTurnover'], 
                        input[name='totalThreeMonthTurnover'], 
                        input[name='reject_Defaults_CreditScore'], 
                        input[name='reject_Defaults_AccountsNum'], 
                        input[name='reject_Defaults_Amount'], 
                        select[name='enableAutomaticReRejection'], 
                        input[name='autoRejectionException_CreditScore'], 
                        input[name='autoRejectionException_AnualTurnover'],
                        input[name='reject_Defaults_MonthsNum']").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"});
            @$el.find("button[name='SaveRejectionSettings'], button[name='CancelRejectionSettings']").hide();
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
                lowCreditScore:
                    required: true
                    min: 0
                totalAnnualTurnover:
                    required: true
                    min: 0
                totalThreeMonthTurnover:
                    required: true
                    min: 0
                reject_Defaults_CreditScore:
                    required: true
                    min: 0
                reject_Defaults_AccountsNum:
                    required: true
                    min: 0
                reject_Defaults_Amount:
                    required: true
                    min: 0
                reject_Defaults_MonthsNum:
                    required: true
                    min: 0
                autoRejectionException_CreditScore:
                    required: true
                    min: 0
                autoRejectionException_AnualTurnover:
                    required: true
                    min: 0