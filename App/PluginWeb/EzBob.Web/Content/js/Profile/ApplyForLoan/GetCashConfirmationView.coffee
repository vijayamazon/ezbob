root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Profile = EzBob.Profile or {}


class EzBob.GetCashConfirmation extends Backbone.Marionette.ItemView
  template: "#apply-forloan-confirmation-template"
  events:
    "click a.cancel": "btnClose"
    "click a.save": "btnSave"

  btnClose: ->
    @close()
    false

  btnSave: ->
    @trigger "modal:save"
    @onOk()
    @close()
    false

  onOk: ->