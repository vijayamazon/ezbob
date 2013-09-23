root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}


class EzBob.Underwriter.PersonInfoView extends Backbone.Marionette.ItemView
    template: "#profile-person-info-template"
    initialize: ->
        @bindTo @model, "change sync", @render, this

    onRender: ->
        @$el.find(".tltp").tooltip()
        @$el.find(".tltp-left").tooltip({placement: "left"})

    events:
        "click button[name=\"changeDisabledState\"]": "changeDisabledState"
        "click button[name=\"editEmail\"]": "editEmail"
        "click [name=\"isTestEditButton\"]": "isTestEditButton"
        "click [name=\"avoidAutomaticDecisionButton\"]": "avoidAutomaticDecisionButton"
        "click [name=\"updateCRM\"]": "updateCRM"
        "click [name=\"changeFraudStatusManualy\"]": "changeFraudStatusManualyClicked"

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
                xhr = $.post "#{window.gRootPath}Underwriter/ApplicationInfo/GetIsStatusWarning", {status: newStatus, async:false}
                xhr.done (result) =>            
                    isWarning = result
                    disabled =  waiting or !isStatusEnabled
                    that.model.set 'Disabled', newStatus
                    that.model.set 'IsWarning', isWarning
        

    isTestEditButton: ->
        d = new EzBob.Dialogs.CheckBoxEdit(
            model: @model
            propertyName: "IsTest"
            title: "Is Testing User"
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
            postValueName: "enbaled"
            checkboxName: "Enable Manual Decision"
            url: "Underwriter/ApplicationInfo/AvoidAutomaticDecision"
            data:
                id: @model.get("Id")
        )
        d.render()
        return


    updateCRM: ->
        that = this
        BlockUi "On"
        xhr = $.post("#{window.gRootPath}Underwriter/CustomerInfo/UpdateCrm",
            id: @model.get("Id")
        )
        xhr.done ->
            xhr2 = that.model.fetch()
            xhr2.done ->
                BlockUi "Off"

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
        EzBob.App.modal.show view
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
            @statuses = new EzBob.Underwriter.CollectionStatuses()
            @statuses.fetch({async:false})

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