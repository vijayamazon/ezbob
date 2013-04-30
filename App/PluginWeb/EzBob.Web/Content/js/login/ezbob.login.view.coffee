root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.CustomerLoginView extends Backbone.View
  initialize: ->
    @template = _.template($("#customerlogin-template").html())
    @model = new EzBob.CustomerLoginModel()
    @on "ready", @ready, this
    @model.on "change:loggedIn", @render, this

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
    #@$el.html @template(@loginModel.toJSON())
    @form = @$el.find(".simple-login")
    @validator = EzBob.validateLoginForm(@form)
    #if @model.get("loggedIn")
    #  @setReadOnly()
    #  @trigger "ready"
    #  @trigger "next"
    @$el.find("img[rel]").setPopover "left"
    @$el.find("li[rel]").setPopover "left"
    this

  inputChanged: ->
    if EzBob.Validation.checkForm(@validator)
      $("#loginSubmit.disabled").removeClass "disabled"
    else
      $("#loginSubmit").addClass "disabled"

  emailChanged: ->
    EzBob.Validation.displayIndication @validator, "EmailImage", "#UserName", "#RotateImage", "#OkImage", "#FailImage"

  passwordChanged: ->
    EzBob.Validation.displayIndication @validator, "PasswordImage", "#Password", "#RotateImage", "#OkImage", "#FailImage"

  submit: ->
    console.log('submit')
    return false if @$el.find(":submit").hasClass("disabled")
    return false if not @validator.form()
    @blockBtn true
    that = this
    #if @model.get("signedIn") or (@model.get("loggedIn"))
    #  @trigger "ready"
    #  @trigger "next"
    #  that.blockBtn false
    #  return false
    console.log('submit1')
    unless EzBob.Validation.validateAndNotify(that.validator) 
        that.blockBtn false
        return false

    console.log('submit2')
    $.post that.form.attr("action"), that.form.serialize(), ((result) ->
        if result.success
            console.log('submit suc')
            that.$el.find("input[type='password'], input[type='text']").tooltip "hide"
            EzBob.App.trigger "loggedIn"
            EzBob.App.trigger "clear"
        
            #EzBob.App.trigger('info', "You have successfully registered. The message was sent to your email.");
            that.model.set "loggedIn", true
            that.trigger "ready"
            that.trigger "next"
            $.get(window.gRootPath + "Start/TopButton").done (dat) ->
            $("#pre_header").html dat
        else
            console.log('submit fail')          
            EzBob.App.trigger "error", result.errorMessage  if result.errorMessage
            that.captcha.reload()
            that.blockBtn false
    ), "json"
    console.log('submit4')
    that.blockBtn false
    false

  ready: ->
    @setReadOnly()

  setReadOnly: ->
    @readOnly = true
    @$el.find(":input").not(":submit").attr("disabled", "disabled").attr("readonly", "readonly").css "disabled"

  blockBtn: (isBlock) ->
    BlockUi (if isBlock then "on" else "off")
    @$el.find(":submit").toggleClass "disabled", isBlock


class EzBob.CustomerLoginModel extends Backbone.Model
    defaults:
        loggedIn: false
        completed: false


