root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.PayPalAccountDetailsModel extends Backbone.Model
    idAttribute: "marketplaceId"
    url: ->
        "#{window.gRootPath}Underwriter/PaymentAccounts/Details/#{@id}"

class EzBob.Underwriter.PayPalAccountDetailsView extends Backbone.Marionette.ItemView
    template: "#payPalAccount-values-template"

    initialize: ->
        @bindTo @model, "change init", @render, this

    serializeData: ->
        paymentAccounts: @model.toJSON()

    jqoptions: ->
        modal: true
        resizable: false
        title: @model.get("Name")
        position: "center"
        draggable: false
        dialogClass: "paypalDetail"