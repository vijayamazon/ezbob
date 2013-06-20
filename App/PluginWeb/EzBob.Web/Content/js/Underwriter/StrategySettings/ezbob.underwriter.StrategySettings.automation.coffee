root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.StrategyAutomationView extends Backbone.View
    initialize: ->
        @template = _.template($("#automation-detail-settings").html())

    render: ->
        @$el.html @template()
        automation = @$el.find("#automation-settings")
        approval = @$el.find("#approvals-settings")
        rejection = @$el.find("#rejections-settings")

        @automationModel =  new EzBob.Underwriter.SettingsAutomationModel()
        @automationView =  new EzBob.Underwriter.SettingsAutomationView(
            el: automation
            model: @automationModel
        )

        @approvalModel =  new EzBob.Underwriter.SettingsApprovalModel()
        @approvalView =  new EzBob.Underwriter.SettingsApprovalView(
            el: approval
            model: @approvalModel
        )

        @rejectionModel =  new EzBob.Underwriter.SettingsRejectionModel()
        @rejectionView =  new EzBob.Underwriter.SettingsRejectionView(
            el: rejection
            model: @rejectionModel
        )


    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

    onClose: ->
        @modelBinder.unbind()