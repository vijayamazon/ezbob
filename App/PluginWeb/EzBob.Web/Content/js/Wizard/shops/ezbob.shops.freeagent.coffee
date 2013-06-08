root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.FreeAgentAccountButtonView extends EzBob.StoreButtonView
    initialize: ->
        super({name: 'FreeAgent', logoText: '', shops: @model})
    update: ->
        @model.fetch()

class EzBob.FreeAgentAccountInfoView extends Backbone.Marionette.ItemView
    template: '#FreeAgentAccoutInfoTemplate'
    
    events: 
        'click a.connect-freeagent': 'connect'
        "click a.back": "back"
        'change input': 'inputChanged'
        'keyup input': 'inputChanged'

    ui:
        displayName : '#freeagent_name'
        connect     : 'a.connect-freeagent'
        form        : 'form'

    inputChanged: ->
        enabled =  EzBob.Validation.checkForm(@validator)
        @ui.connect.toggleClass('disabled', !enabled)

    connect: ->
        return false if not @validator.form()            
        return false if @$el.find('a.connect-freeagent').hasClass('disabled')            
        acc = new EzBob.FreeAgentAccountModel({displayName: @ui.displayName.val()})
        xhr = acc.save()
        if not xhr
            EzBob.App.trigger 'error', 'FreeAgent Account Saving Error'
            return false

        BlockUi('on')
        xhr.always =>
            BlockUi('off')

        xhr.fail (jqXHR, textStatus, errorThrown) =>
            EzBob.App.trigger 'error', 'FreeAgent Account Saving Error'

        xhr.done (res) =>
            if (res.error)
                EzBob.App.trigger 'error', res.error
                return false
            try
                @model.add(acc)

            EzBob.App.trigger('info', "FreeAgent Account Added Successfully");
            @ui.displayName.val("") 
            @inputChanged()
            @trigger('completed');
            @trigger 'back'

        false

    render: ->
        super()
        oFieldStatusIcons = $ 'IMG.field_status'
        oFieldStatusIcons.filter('.required').field_status({ required: true })
        oFieldStatusIcons.not('.required').field_status({ required: false })

        @validator = EzBob.validateFreeAgentAccountForm @ui.form
        return @

    back: ->
        @trigger 'back'
        false

    getDocumentTitle: ->
        "Link FreeAgent Account"


class EzBob.FreeAgentAccountModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/FreeAgentMarketPlaces/Accounts"

class EzBob.FreeAgentAccounts extends Backbone.Collection
    model: EzBob.FreeAgentAccountModel
    url: "#{window.gRootPath}Customer/FreeAgentMarketPlaces/Accounts"
