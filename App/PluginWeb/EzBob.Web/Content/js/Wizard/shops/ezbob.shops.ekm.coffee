root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.EKMAccountInfoView extends Backbone.Marionette.ItemView
    template: '#EKMAccoutInfoTemplate'
    
    events: 
        'click a.connect-ekm': 'connect'
        "click a.back": "back"
        'change input': 'inputChanged'
        'keyup input': 'inputChanged'

    ui:
        login       : '#ekm_login'
        password    : '#ekm_password'
        connect     : 'a.connect-ekm'
        form        : 'form'

    inputChanged: ->
        enabled =  EzBob.Validation.checkForm(@validator)
        @ui.connect.toggleClass('disabled', !enabled)

    connect: ->
        if not EzBob.Validation.checkForm(@validator)
            @validator.form()
            return false
        return false if @$el.find('a.connect-ekm').hasClass('disabled')            
        acc = new EzBob.EKMAccountModel({login: @ui.login.val(), password: @ui.password.val()})
        xhr = acc.save()
        if not xhr
            EzBob.App.trigger 'error', 'EKM Account Saving Error'
            return false

        BlockUi('on')
        xhr.always =>
            BlockUi('off')

        xhr.fail (jqXHR, textStatus, errorThrown) =>
            EzBob.App.trigger 'error', 'EKM Account Saving Error'

        xhr.done (res) =>
            if (res.error)
                EzBob.App.trigger 'error', res.error
                return false
            try
                @model.add(acc)

            EzBob.App.trigger('info', "EKM Account Added Successfully");
            @ui.login.val("") 
            @ui.password.val("")
            @inputChanged()
            @trigger('completed');
            @trigger 'back'

        false

    render: ->
        super()

        @validator = EzBob.validateEkmShopForm @ui.form
        return @

    back: ->
        @trigger 'back'
        false

    getDocumentTitle: ->
        EzBob.App.trigger 'clear'
        "Link EKM Account"


class EzBob.EKMAccountModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/EkmMarketPlaces/Accounts"

class EzBob.EKMAccounts extends Backbone.Collection
    model: EzBob.EKMAccountModel
    url: "#{window.gRootPath}Customer/EkmMarketPlaces/Accounts"
