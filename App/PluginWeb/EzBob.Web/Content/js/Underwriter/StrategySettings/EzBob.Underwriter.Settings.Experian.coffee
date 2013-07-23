root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}
EzBob.Underwriter.Settings = EzBob.Underwriter.Settings or {}

class EzBob.Underwriter.Settings.ExperianModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/SettingsExperian"

class EzBob.Underwriter.Settings.ExperianView extends Backbone.Marionette.ItemView
    template: "#experian-settings-template"

    initialize: (options) ->
        @modelBinder = new Backbone.ModelBinder()
        @model.on "reset", @render, @
        @update()
        @

    bindings:
        FinancialAccounts_MainApplicant: "[name='FinancialAccounts_MainApplicant']"
        FinancialAccounts_AliasOfMainApplicant: "[name='FinancialAccounts_AliasOfMainApplicant']"
        FinancialAccounts_AssociationOfMainApplicant: "[name='FinancialAccounts_AssociationOfMainApplicant']"
        FinancialAccounts_JointApplicant: "[name='FinancialAccounts_JointApplicant']"
        FinancialAccounts_AliasOfJointApplicant: "[name='FinancialAccounts_AliasOfJointApplicant']"
        FinancialAccounts_AssociationOfJointApplicant: "[name='FinancialAccounts_AssociationOfJointApplicant']"
        FinancialAccounts_No_Match: "[name='FinancialAccounts_No_Match']"

    events:
        "click #SaveExperianSettings": "saveSettings"
        "click #CancelExperianSettings": "cancelSettings"

    saveSettings: ->
        BlockUi "on"
        @model.save().done ->  EzBob.ShowMessage  "Saved successfully", "Successful"
        @model.save().complete -> BlockUi "off"
        false

    cancelSettings: ->
        @update()
        false
    
    update: ->
        xhr = @model.fetch()
        xhr.done => @render()

    onRender: -> 
        @modelBinder.bind @model, @el, @bindings
        if !$("body").hasClass("role-manager") 
            @$el.find("select").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"})
            @$el.find("button").hide()

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

    onClose: ->
        @modelBinder.unbind()