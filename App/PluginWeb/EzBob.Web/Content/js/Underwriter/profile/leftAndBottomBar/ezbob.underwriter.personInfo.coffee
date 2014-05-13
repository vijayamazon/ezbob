root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}


class EzBob.Underwriter.PersonInfoView extends Backbone.Marionette.ItemView
    template: "#profile-person-info-template"
    initialize: ->
        @bindTo @model, "change sync", @render, this

    onRender: ->
        if (this.$el.find('.red_cell').length != 0)
            this.$el.parent().parent().find('#customer-label-span').removeClass('label-success').addClass('label-warning')

        if @model.get('BrokerID')
            @$el.find('#with-broker').addClass 'with-broker'

        @$el.find(".tltp").tooltip()
        @$el.find(".tltp-left").tooltip({placement: "left"})

        if @model.get 'IsWizardComplete'
            @$el.find('#ForceFinishWizard').addClass 'hide'

    toggleCciMark: ->
        id = @model.get 'Id'

        BlockUi()

        $.post(window.gRootPath + 'Underwriter/ApplicationInfo/ToggleCciMark',
            id: id
        ).done( (result) =>
            if result.error
                EzBob.App.trigger 'error', result.error
            else
                @setAlertStatus(result.mark, '.cci-mark','.cci-mark-td', 'on', 'off')
        ).always( ->
            UnBlockUi()
        )

    toggleIsTest: ->
        id = @model.get 'Id'

        BlockUi()

        $.post(window.gRootPath + 'Underwriter/ApplicationInfo/ToggleIsTest',
            id: id
        ).done( (result) =>
            if result.error
                EzBob.App.trigger 'error', result.error
            else
                @setAlertStatus(result.isTest, '.is-test', '.is-test-td', 'Yes', 'No')
        ).always( ->
            UnBlockUi()
        )

    setAlertStatus: (isAlert, span, td, alertText = '', okText = '') ->
        oSpan = @$el.find span
        oTd = @$el.find td

        if isAlert
            if alertText != ''
                oSpan.text(alertText)
            oSpan.closest('td').addClass 'red_cell'
            oTd.addClass 'red_cell'
        else
            if okText != ''
                oSpan.text(okText)
            oSpan.closest('td').removeClass 'red_cell'
            oTd.removeClass 'red_cell'

    events:
        "click button[name=\"changeDisabledState\"]": "changeDisabledState"
        "click button[name=\"editEmail\"]": "editEmail"
        "click [name=\"avoidAutomaticDecisionButton\"]": "avoidAutomaticDecisionButton"
        "click [name=\"changeFraudStatusManualy\"]": "changeFraudStatusManualyClicked"
        'click button.cci-mark-toggle': 'toggleCciMark'
        'click button.istest-toggle': 'toggleIsTest'
        'click [name="TrustPilotStatusUpdate"]': 'updateTrustPilotStatus'
        'click #MainStrategyHidden': 'activateMainStratgey'
        'click #ForceFinishWizard': 'activateFinishWizard'
        
    activateMainStratgey: ->
        xhr = $.post "#{window.gRootPath}Underwriter/ApplicationInfo/ActivateMainStrategy", customerId: @model.get('Id')

    activateFinishWizard: ->
        xhr = $.post "#{window.gRootPath}Underwriter/ApplicationInfo/ActivateFinishWizard", customerId: @model.get('Id')

    updateTrustPilotStatus: ->
        d = new EzBob.Dialogs.ComboEdit
            model: @model
            propertyName: 'TrustPilotStatusName'
            title: 'Trust Pilot Status'
            width: 500
            postValueName: 'status'
            comboValues: @model.get 'TrustPilotStatusList'
            url: "Underwriter/ApplicationInfo/UpdateTrustPilotStatus"
            data: { id: @model.get('Id') }

        d.render()

        d.on( 'done', =>
            @model.fetch()
        )
        return

    changeFraudStatusManualyClicked: ->
        fraudStatusModel = new EzBob.Underwriter.FraudStatusModel( 
            customerId : @model.get ('Id')
            currentStatus : @model.get('FraudCheckStatusId')
            )
        BlockUi "on"
        that = this
        xhr = fraudStatusModel.fetch();
        xhr.done =>
            fraudStatusLayout = new EzBob.Underwriter.FraudStatusLayout model: fraudStatusModel
            fraudStatusLayout.render()
            EzBob.App.jqmodal.show(fraudStatusLayout)
            BlockUi "off"
            fraudStatusLayout.on 'saved', () =>
                currentStatus = fraudStatusModel.get ('currentStatus')
                @model.set 'FraudCheckStatusId', currentStatus
                @model.set 'FraudCheckStatus', fraudStatusModel.get ('currentStatusText')
                that.setAlertStatus(currentStatus != 0, '.fraud-status', '.fraud-status-td')

    templateHelpers:
        getIcon: ->
            return "icon-ok"  if @EmailState is "Confirmed" or @EmailState is "ManuallyConfirmed"
            "icon-question-sign"

    serializeData: ->
        data = @model.toJSON()
        return {
            data: data,
            getIcon: @templateHelpers.getIcon
        };

    changeDisabledState: ->
        collectionStatusModel = new EzBob.Underwriter.CollectionStatusModel( 
            customerId : @model.get ('Id')
            currentStatus : @model.get('CustomerStatusId')
            )
        prevStatus = @model.get('CustomerStatusId')
        customerId = @model.get('Id')
        BlockUi "on"
        xhr = collectionStatusModel.fetch();
        xhr.done =>
            collectionStatusLayout = new EzBob.Underwriter.CollectionStatusLayout model: collectionStatusModel
            collectionStatusLayout.render()
            EzBob.App.jqmodal.show(collectionStatusLayout)
            BlockUi "off"
            collectionStatusLayout.on 'saved', () =>
                newStatus = collectionStatusModel.get ('currentStatus')
                that = this
                xhr = $.post "#{window.gRootPath}Underwriter/ApplicationInfo/GetIsStatusWarning", {status: newStatus}
                xhr.done (result) =>    
                    BlockUi "on"        
                    isWarning = result
                    that.model.fetch()
                    xhr2 = $.post "#{window.gRootPath}Underwriter/ApplicationInfo/LogStatusChange", {newStatus: newStatus, prevStatus: prevStatus, customerId: customerId}
                    xhr2.done () =>
                        BlockUi "off"

    avoidAutomaticDecisionButton: ->
        d = new EzBob.Dialogs.CheckBoxEdit(
            model: @model
            propertyName: "IsAvoid"
            title: "Manual Decision"
            width: 350
            postValueName: "enbaled"
            checkboxName: "Enable Manual Decision"
            url: "Underwriter/ApplicationInfo/AvoidAutomaticDecision"
            data:
                id: @model.get("Id")
        )
        d.render()
        return

    editEmail: ->
        view = new EzBob.EmailEditView(model: @model)
        EzBob.App.jqmodal.show view
        view.on "showed", ->
            view.$el.find("input").focus()

        false

class EzBob.Underwriter.PersonalInfoModel extends Backbone.Model
    idAttribute: "Id"
    urlRoot: window.gRootPath + "Underwriter/CustomerInfo/Index"
    initialize: ->
        @on "change:FraudCheckStatusId", @changeFraudCheckStatus, this
        @changeFraudCheckStatus()
        if (@StatusesArr == undefined)
            @statuses = EzBob.Underwriter.StaticData.CollectionStatuses

        @StatusesArr = {}
        for status in @statuses.models
            @StatusesArr[status.get('Id')] = status.get('Name')
            
    changeFraudCheckStatus: ->
        fraud = @get("FraudCheckStatusId")
        fraudCss = ""
        
        switch fraud
            when 2
                fraudCss = "red_cell"
        
        @set "FraudHighlightCss", fraudCss