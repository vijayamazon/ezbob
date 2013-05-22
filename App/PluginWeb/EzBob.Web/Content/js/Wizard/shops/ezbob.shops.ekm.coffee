root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.EKMAccountButtonView extends EzBob.StoreButtonView
    initialize: ->
        super({name: 'EKM', logoText: '', shops: @model})
    update: ->
        @model.fetch()

    #isAddingAllowed: -> true @model.length == 0

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
        return false if not @validator.form()            
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
        oFieldStatusIcons = $ 'IMG.field_status'
        oFieldStatusIcons.filter('.required').field_status({ required: true })
        oFieldStatusIcons.not('.required').field_status({ required: false })

        @validator = EzBob.validateEkmShopForm @ui.form
        return @

    back: ->
        @trigger 'back'
        false

    getDocumentTitle: ->
        "Link EKM Account"


class EzBob.EKMAccountModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/EkmMarketPlaces/Accounts"

class EzBob.EKMAccounts extends Backbone.Collection
    model: EzBob.EKMAccountModel
    url: "#{window.gRootPath}Customer/EkmMarketPlaces/Accounts"
