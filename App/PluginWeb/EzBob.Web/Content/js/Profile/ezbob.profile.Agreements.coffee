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

  events:
    "change .preAgreementTermsRead": "preAgreementTermsReadChange"

  preAgreementTermsReadChange: ->
    readPreAgreement = $(".preAgreementTermsRead").is(":checked")
    @$el.find(".agreementTermsRead").attr "disabled", not readPreAgreement
    if readPreAgreement
      @$el.find("a[href=\"#tab4\"]").tab "show"
    else
      @$el.find("a[href=\"#tab3\"]").tab "show"
      @$el.find(".agreementTermsRead").attr "checked", false

  render: (data)->
    @$el.find(".agreementTermsRead").attr("checked", false).attr("disabled", true)
    super(data)
