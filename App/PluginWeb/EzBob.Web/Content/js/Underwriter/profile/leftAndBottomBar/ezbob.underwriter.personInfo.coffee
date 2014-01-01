root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}


class EzBob.Underwriter.PersonInfoView extends Backbone.Marionette.ItemView
    template: "#profile-person-info-template"
    initialize: ->
        @bindTo @model, "change sync", @render, this

    onRender: ->
        @setCciMark()
        @$el.find(".tltp").tooltip()
        @$el.find(".tltp-left").tooltip({placement: "left"})

    setCciMark: ->
        oSpan = @$el.find '.cci-mark'

        if @model.get 'CciMark'
            oSpan.text('on').closest('td').addClass 'red_cell'
        else
            oSpan.text('off').closest('td').removeClass 'red_cell'
    # end of setCciMark

    toggleCciMark: ->
        id = @model.get 'Id'

        BlockUi()

        $.post(window.gRootPath + 'Underwriter/ApplicationInfo/ToggleCciMark',
            id: id
        ).done( (result) =>
            if result.error
                EzBob.App.trigger 'error', result.error
            else
                @model.set('CciMark', result.mark)
                @setCciMark()
        ).always( ->
            UnBlockUi()
        )
    # end of toggleCciMark

    events:
        "click button[name=\"changeDisabledState\"]": "changeDisabledState"
        "click button[name=\"editEmail\"]": "editEmail"
        "click [name=\"isTestEditButton\"]": "isTestEditButton"
        "click [name=\"avoidAutomaticDecisionButton\"]": "avoidAutomaticDecisionButton"
        "click [name=\"changeFraudStatusManualy\"]": "changeFraudStatusManualyClicked"
        'click button.cci-mark-toggle': 'toggleCciMark'

    changeFraudStatusManualyClicked: ->
        fraudStatusModel = new EzBob.Underwriter.FraudStatusModel( 
            customerId : @model.get ('Id')
            currentStatus : @model.get('FraudCheckStatusId')
            )
        BlockUi "on"
        xhr = fraudStatusModel.fetch();
        xhr.done =>
            fraudStatusLayout = new EzBob.Underwriter.FraudStatusLayout model: fraudStatusModel
            fraudStatusLayout.render()
            EzBob.App.jqmodal.show(fraudStatusLayout)
            BlockUi "off"
            fraudStatusLayout.on 'saved', () =>
                @model.set 'FraudCheckStatusId', fraudStatusModel.get ('currentStatus')
                @model.set 'FraudCheckStatus', fraudStatusModel.get ('currentStatusText')

    templateHelpers:
        getIcon: ->
            return "icon-ok"  if @EmailState is "Confirmed" or @EmailState is "ManuallyConfirmed"
            "icon-question-sign"

    changeDisabledState: ->
        collectionStatusModel = new EzBob.Underwriter.CollectionStatusModel( 
            customerId : @model.get ('Id')
            currentStatus : @model.get('Disabled')
            )
        prevStatus = @model.get('Disabled')
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
                    that.model.set 'Disabled', newStatus
                    that.model.set 'IsWarning', isWarning
                    xhr2 = $.post "#{window.gRootPath}Underwriter/ApplicationInfo/LogStatusChange", {newStatus: newStatus, prevStatus: prevStatus, customerId: customerId}
                    xhr2.done () =>
                        BlockUi "off"

    isTestEditButton: ->
        d = new EzBob.Dialogs.CheckBoxEdit(
            model: @model
            propertyName: "IsTest"
            title: "Is Testing User"
            width: 350
            postValueName: "enbaled"
            checkboxName: "Test"
            url: "Underwriter/ApplicationInfo/ChangeTestStatus"
            data:
                id: @model.get("Id")
        )
        d.render()
        return

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

    disablingChanged: ->
        disabled = @$el.find("select[name=\"disabling\"] option:selected").val()
        id = @model.get("Id")
        that = this
        @model.set "Disabled", disabled
        $.post(window.gRootPath + "Underwriter/ApplicationInfo/ChangeDisabled",
            id: id
            disabled: disabled
        ).done ->
            that.trigger "DisableChange", id


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
        @on "change:Disabled", @changeDisabled, this
        @on "change:FraudCheckStatusId", @changeFraudCheckStatus, this
        @changeDisabled()
        @changeFraudCheckStatus()
        if (@StatusesArr == undefined)
            @statuses = EzBob.Underwriter.StaticData.CollectionStatuses

        @StatusesArr = {}
        for status in @statuses.models
            @StatusesArr[status.get('Id')] = status.get('Name')

    changeDisabled: (silent = false)->
        disabledText = ""
        disabled = @get("Disabled")
        if (disabled == undefined)
            return

        disabledText = @StatusesArr[disabled]
        if (disabledText == undefined)
            disabledText = "Enabled"

        @set {"DisabledText": disabledText}, {silent:true}

    changeFraudCheckStatus: ->
        #fraudText = @get("FraudCheckStatus")
        fraud = @get("FraudCheckStatusId")
        fraudCss = ""
        
        switch fraud
            when 2
                fraudCss = "red_cell"
        
        #@set "FraudCheckStatus", fraudText
        @set "FraudHighlightCss", fraudCss