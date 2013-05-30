root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.VolusionAccountButtonView extends EzBob.StoreButtonView
    initialize: ->
        super({name: 'Volusion', logoText: '', shops: @model})
    update: ->
        @model.fetch()

    #isAddingAllowed: -> true @model.length == 0

class EzBob.VolusionAccountInfoView extends Backbone.Marionette.ItemView
    template: '#VolusionAccoutInfoTemplate'

    events:
        'click a.connect-volusion': 'connect'
        'click a.back': 'back'
        'change input': 'inputChanged'
        'keyup input': 'inputChanged'

    ui:
        login       : '#volusion_login'
        password    : '#volusion_password'
        url         : '#volusion_url'
        connect     : 'a.connect-volusion'
        form        : 'form'

    inputChanged: =>
        enabled =  EzBob.Validation.checkForm(@validator) 
        @ui.connect.toggleClass('disabled', !enabled)

    connect: ->
        return false if not @validator.form()
        return false if @$el.find('a.connect-volusion').hasClass('disabled')

        aryDisplayName = /^http[s]?:\/\/([^\/\?]+)/.exec(@ui.url.val())
        sDisplayName = if aryDisplayName and aryDisplayName.length and aryDisplayName.length == 2 then aryDisplayName[1] else @ui.url.val()
        sDisplayName = sDisplayName or @ui.url.val()
        
        acc = new EzBob.VolusionAccountModel({
            login: @ui.login.val(),
            password: @ui.password.val(),
            displayName: sDisplayName,
            url: @ui.url.val()
        })

        xhr = acc.save()
        if not xhr
            EzBob.App.trigger 'error', 'Volusion Account Saving Error'
            return false

        BlockUi('on')
        xhr.always =>
            BlockUi('off')

        xhr.fail (jqXHR, textStatus, errorThrown) =>
            EzBob.App.trigger 'error', 'Volusion Account Saving Error'

        xhr.done (res) =>
            if (res.error)
                EzBob.App.trigger 'error', res.error
                return false
            try
                @model.add(acc)

            EzBob.App.trigger('info', "Volusion Account Added Successfully");
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

        @validator = EzBob.validateVolusionShopForm @ui.form
        return @

    back: ->
        @trigger 'back'
        false

    getDocumentTitle: ->
        "Link Volusion Account"


class EzBob.VolusionAccountModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/VolusionMarketPlaces/Accounts"

class EzBob.VolusionAccounts extends Backbone.Collection
    model: EzBob.VolusionAccountModel
    url: "#{window.gRootPath}Customer/VolusionMarketPlaces/Accounts"
