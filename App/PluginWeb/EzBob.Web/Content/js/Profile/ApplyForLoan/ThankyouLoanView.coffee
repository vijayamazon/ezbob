root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Profile = EzBob.Profile or {}


class EzBob.Profile.ThankyouLoan extends Backbone.View
  initialize: ->
    @template = _.template($("#thankyouloan-template").html())

  events:
    "click a": "close"

  render: ->
    @$el.html @template()

  close: ->
    @trigger "close"
    false