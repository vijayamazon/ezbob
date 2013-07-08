root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}


class EzBob.Underwriter.PersonInfoView extends Backbone.Marionette.ItemView
    template: "#profile-person-info-template"
    initialize: ->
        @bindTo @model, "change", @render, this

    onRender: ->
        @$el.find(".tltp").tooltip()
        @$el.find(".tltp-left").tooltip({placement: "left"})

    events:
        "click button[name=\"changeDisabledState\"]": "changeDisabledState"
        "click button[name=\"editEmail\"]": "editEmail"
        "click [name=\"isTestEditButton\"]": "isTestEditButton"
        "click [name=\"avoidAutomaticDecisionButton\"]": "avoidAutomaticDecisionButton"
        "click [name=\"updateCRM\"]": "updateCRM"

    templateHelpers:
        getIcon: ->
            return "icon-ok"  if @EmailState is "Confirmed" or @EmailState is "ManuallyConfirmed"
            "icon-question-sign"

    changeDisabledState: ->
        collectionStatusModel = new EzBob.Underwriter.CollectionStatusModel( 
            customerId : @model.get ('Id')
            currentStatus : @model.get('Disabled')
            )
        xhr = collectionStatusModel.fetch();
        xhr.done =>
            collectionStatusLayout = new EzBob.Underwriter.CollectionStatusLayout model: collectionStatusModel
            collectionStatusLayout.render()
            EzBob.App.jqmodal.show(collectionStatusLayout)
            collectionStatusLayout.on 'saved', () =>
                @model.set 'Disabled', collectionStatusModel.get ('currentStatus')
        

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
            title: "Avoid Automatic Decision ?"
            postValueName: "enbaled"
            checkboxName: "Avoid"
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
        @changeDisabled()

    changeDisabled: ->
        disabledText = ""
        disabled = @get("Disabled")
        switch disabled
            when 1
                disabledText = "Disabled"
            when 2
                disabledText = "Fraud"
            when 3
                disabledText = "Bankruptcy"
            when 4
                disabledText = "Default"
            when 5
                disabledText = "Fraud Suspect"
            else
                disabledText = "Enabled"
        @set "DisabledText", disabledText