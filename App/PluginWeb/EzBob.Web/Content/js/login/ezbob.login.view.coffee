root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.CustomerLoginView extends Backbone.View
  initialize: ->
    @template = _.template($("#customerlogin-template").html())

  events:
    "click :submit": "submit"
    "keydown input[name='UserName']": "inputChanged"
    "paste input[name='UserName']": "inputChanged"
    "input input[name='UserName']": "inputChanged"
    "change input[name='UserName']": "emailChanged"
    "keydown input[name='Password']": "inputChanged"
    "paste input[name='Password']": "inputChanged"
    "input input[name='Password']": "inputChanged"
    "change input[name='Password']": "passwordChanged"

  render: ->
    @$el.html @template()
    @form = @$el.find(".simple-login")
    @validator = EzBob.validateLoginForm(@form)
    @$el.find("img[rel]").setPopover "left"
    @$el.find("li[rel]").setPopover "left"
    oFieldStatusIcons = this.$el.find('IMG.field_status')
    oFieldStatusIcons.filter('.required').field_status({ required: true })
    oFieldStatusIcons.not('.required').field_status({ required: false })
    @

  inputChanged: ->
    if EzBob.Validation.checkForm(@validator)
      $("#loginSubmit.disabled").removeClass "disabled"
    else
      $("#loginSubmit").addClass "disabled"

  emailChanged: ->
    @$el.find('#UserNameImage').field_status('set', if EzBob.Validation.element(@validator,@$el.find('#UserName')) then 'ok' else 'fail')

  passwordChanged: ->
    @$el.find('#PasswordImage').field_status('set', if EzBob.Validation.element(@validator,@$el.find('#Password')) then 'ok' else 'fail')

  submit: ->
    return false if @$el.find(":submit").hasClass("disabled")
    return false if not @validator.form()
    @blockBtn true

    unless EzBob.Validation.validateAndNotify(@validator) 
        @blockBtn false
        return false

    xhr = $.post @form.attr("action"), @form.serialize()
    
    xhr.done (result, status) =>
        
        if status is "success"
            if result.success
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
