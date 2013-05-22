root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.PlayAccountButtonView extends EzBob.StoreButtonView
    initialize: ->
        super({name: 'Play', logoText: '', shops: @model})
    update: ->
        @model.fetch()

    #isAddingAllowed: -> true @model.length == 0

class EzBob.PlayAccountInfoView extends Backbone.Marionette.ItemView
    template: '#PlayAccoutInfoTemplate'

    events:
        'click a.connect-play': 'connect'
        'click a.back': 'back'
        'change input': 'inputChanged'
        'keyup input': 'inputChanged'

    ui:
        login       : '#play_login'
        password    : '#play_password'
        name         : '#play_name'
        connect     : 'a.connect-play'
        form        : 'form'

    inputChanged: =>
        enabled =  EzBob.Validation.checkForm(@validator) 
        @ui.connect.toggleClass('disabled', !enabled)

    connect: ->
        return false if not @validator.form()
        return false if @$el.find('a.connect-play').hasClass('disabled')

        acc = new EzBob.PlayAccountModel({
            login: @ui.login.val(),
            password: @ui.password.val(),
            name: @ui.name.val()
        })

        xhr = acc.save()
        if not xhr
            EzBob.App.trigger 'error', 'Play.com Account Saving Error'
            return false

        BlockUi('on')
        xhr.always =>
            BlockUi('off')

        xhr.fail (jqXHR, textStatus, errorThrown) =>
            console.log textStatus
            EzBob.App.trigger 'error', 'Play.com Account Saving Error'

        xhr.done (res) =>
            if (res.error)
                EzBob.App.trigger 'error', res.error
                return false
            
            try
                @model.add(acc)

            EzBob.App.trigger('info', "Play.com Account Added Successfully");
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

        @validator = EzBob.validatePlayShopForm @ui.form
        return @

    back: ->
        @trigger 'back'
        false

    getDocumentTitle: ->
        "Link Play.com Account"


class EzBob.PlayAccountModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/PlayMarketPlaces/Accounts"

class EzBob.PlayAccounts extends Backbone.Collection
    model: EzBob.PlayAccountModel
    url: "#{window.gRootPath}Customer/PlayMarketPlaces/Accounts"
