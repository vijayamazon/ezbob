root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.BankAccountDetailsView extends Backbone.Marionette.ItemView
    template: "#bank-account-details-template"

    jqoptions: ->
        modal: true
        resizable: false
        title: "Bank Account Details"
        position: "center"
        draggable: false
        dialogClass: "bank-account-details-popup"
        