root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.CustomerLoginView extends Backbone.View
  initialize: ->
    @template = _.template($("#customerlogin-template").html())

  events:
    "click :submit": "submit"
    "keyup input": "inputChanged"
    "change input": "inputChanged"
    "click.checks #RememberMe": "rememberMeChanged" 

  render: ->
    @$el.html @template()
    @form = @$el.find(".simple-login")
    @validator = EzBob.validateLoginForm(@form)

    @$el.find("img[rel]").setPopover "left"
    @$el.find("li[rel]").setPopover "left"

    oFieldStatusIcons = this.$el.find('IMG.field_status')
    oFieldStatusIcons.filter('.required').field_status({ required: true })
    oFieldStatusIcons.not('.required').field_status({ required: false })

    $('#Password').focus()
    $('#UserName').focus()
    EzBob.UiAction.registerView @
    @

  rememberMeChanged: ->
    rememberMe = @$el.find("#RememberMe")
    rememberMe.val(rememberMe.is(':checked'));
  inputChanged: ->
        enabled =  EzBob.Validation.checkForm(@validator) 
        $("#loginSubmit").toggleClass('disabled', !enabled)

  submit: ->
    return false if @$el.find(":submit").hasClass("disabled")
    return false if not EzBob.Validation.checkForm(@validator)
    @blockBtn true

    unless EzBob.Validation.checkForm(@validator) 
        @blockBtn false
        return false

    data = @form.serialize()

    console.log 'data is', data

    xhr = $.post @form.attr("action"), data

    xhr.done (result, status) =>
        EzBob.ServerLog.debug 'login request completed with status', status

        if status is "success"
            if result.success
                if result.broker
                    document.location.href = "#{window.gRootPath}Broker#login"
                else
                    document.location.href = "#{window.gRootPath}Customer/Profile"
            else
                EzBob.App.trigger "error", result.errorMessage
                @blockBtn false
        else
            EzBob.App.trigger "error", result.errorMessage if result.errorMessage
            @blockBtn false
    false

  blockBtn: (isBlock) ->
    BlockUi (if isBlock then "on" else "off")
    @$el.find(":submit").toggleClass "disabled", isBlock
