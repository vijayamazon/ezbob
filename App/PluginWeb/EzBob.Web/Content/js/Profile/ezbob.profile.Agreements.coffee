root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Profile = EzBob.Profile or {}

class EzBob.Profile.AgreementViewBase extends Backbone.View
  initialize: ->
    @template = Handlebars.compile($(@getTemplate()).html())
  render: (data) ->    
    @$el.html @template(data)    
    @addScroll()
    @$el.find("a[data-toggle=\"tab\"]").on "shown", (e) =>
      @.addScroll()
    this
  addScroll: ->
    @$el.find(".overview").jScrollPane()

class EzBob.Profile.CompaniesAgreementView extends EzBob.Profile.AgreementViewBase
  getTemplate: ->
    "#companies-agreement-template"

class EzBob.Profile.ConsumersAgreementView extends EzBob.Profile.AgreementViewBase
  getTemplate: ->
    "#consumers-agreement-template"

  render: (data)->
    $(".company-preAgreement").hide()
    $(".consumer-preAgreement").show()
    super(data)
