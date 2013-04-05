root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.BankAccountDetailsView extends Backbone.Marionette.ItemView
    template: "#bank-account-details-template"