root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.StrategySettingsView extends Backbone.View
    initialize: ->
        @template = _.template($("#strategy-detail-settings").html())

    render: ->
        @$el.html @template()
        general = @$el.find "#general-settings"
        charges = @$el.find "#charges-settings"
        experian = @$el.find "#experian-settings"
        campaign = @$el.find "#campaign-settings"

        @generalModel =  new EzBob.Underwriter.SettingsGeneralModel()
        @generalView =  new EzBob.Underwriter.SettingsGeneralView(
            el: general
            model: @generalModel
        )
        @chargesModel =  new EzBob.Underwriter.SettingsChargesModel()
        @chargesView =  new EzBob.Underwriter.SettingsChargesView(
            el: charges
            model: @chargesModel
        )
        @experianModel =  new EzBob.Underwriter.Settings.ExperianModel()
        @experianView =  new EzBob.Underwriter.Settings.ExperianView(
            el: experian
            model: @experianModel
        )
        @campaignModel = new EzBob.Underwriter.Settings.CampaignModel()
        @campaignView =  new EzBob.Underwriter.Settings.CampaignView(
            el: campaign
            model: @campaignModel
        )
    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

    onClose: ->
        @modelBinder.unbind()