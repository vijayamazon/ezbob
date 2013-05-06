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

        'cut    #volusion_login': 'loginChanged'
        'change #volusion_login': 'loginChanged'
        'keyup  #volusion_login': 'loginChanged'
        'paste  #volusion_login': 'loginChanged'

        'cut    #volusion_url': 'urlChanged'
        'change #volusion_url': 'urlChanged'
        'keyup  #volusion_url': 'urlChanged'
        'paste  #volusion_url': 'urlChanged'

        'cut    #volusion_password': 'passwordChanged'
        'change #volusion_password': 'passwordChanged'
        'keyup  #volusion_password': 'passwordChanged'
        'paste  #volusion_password': 'passwordChanged'

    ui:
        login       : '#volusion_login'
        password    : '#volusion_password'
        url         : '#volusion_url'
        connect     : 'a.connect-volusion'
        form        : 'form'

    loginChanged: =>
        @$el.find('#volusion_loginImage').field_status({ required: true, initial_status: if @ui.login.val() then 'ok' else 'fail' })
        @inputChanged()

    passwordChanged: =>
        @$el.find('#volusion_passwordImage').field_status({ required: true, initial_status: if @ui.password.val() then 'ok' else 'fail' })
        @inputChanged()

    urlChanged: =>
        @$el.find('#volusion_urlImage').field_status({ required: true, initial_status: if @ui.url.val() then 'ok' else 'fail' })
        @inputChanged()

    inputChanged: =>
        enabled = @ui.login.val() and @ui.password.val() and @ui.url.val()
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
            console.log textStatus
            EzBob.App.trigger 'error', 'Volusion Account Saving Error'

        xhr.done (res) =>
            if (res.error)
                EzBob.App.trigger 'error', res.error
                return false

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
        @validator = EzBob.validateVolusionShopForm @ui.form
        return @

    back: ->
        @trigger 'back'
        false


class EzBob.VolusionAccountModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/VolusionMarketPlaces/Accounts"

class EzBob.VolusionAccounts extends Backbone.Collection
    model: EzBob.VolusionAccountModel
    url: "#{window.gRootPath}Customer/VolusionMarketPlaces/Accounts"
