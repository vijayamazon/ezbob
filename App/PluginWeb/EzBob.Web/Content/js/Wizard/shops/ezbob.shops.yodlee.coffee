root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.YodleeAccountButtonView extends EzBob.StoreButtonView
    initialize: ->
        super({name: 'Yodlee', logoText: '', shops: @model})

    update: ->
        @model.fetch().done -> EzBob.App.trigger 'ct:storebase.shop.connected'

class EzBob.YodleeAccountInfoView extends Backbone.Marionette.ItemView
    template: '#YodleeAccoutInfoTemplate'
 
    events: 
        'click a.back': 'back'
        'change input[name="Bank"]': 'bankChanged'
        'click .radio-fx': 'parentBankSelected'
        'change .SubBank': 'subBankSelectionChanged'
        'click #yodleeLinkAccountBtn': 'linkAccountClicked'

    loadBanks: () ->
        @YodleeBanks.fetch().done =>
            if @YodleeBanks.length > 0
                @render
                
    initialize: (options) ->
        that = this;
        @YodleeBanks = new EzBob.YodleeBanks()
        @loadBanks()
        window.YodleeAccountAdded = (result) ->
            if (result.error)
                EzBob.App.trigger('error', result.error);
            else
                EzBob.App.trigger('info', 'Congratulations. Bank account was added successfully.');
            
            that.trigger('completed');
            that.trigger('ready');
            that.trigger('back');

        window.YodleeAccountAddingError = (msg) ->
            EzBob.App.trigger('error', msg)
            that.trigger('back')

        window.YodleeAccountRetry = () ->
            that.attemptsLeft = (that.attemptsLeft || 5) - 1
            return {url: that.$el.find('#yodleeContinueBtn').attr('href'), attemptsLeft: that.attemptsLeft}

        return false
        
    subBankSelectionChanged:(el) ->
        return false if (this.$el.find(".SubBank option:selected").length == 0) 
        url = "#{window.gRootPath}Customer/YodleeMarketPlaces/AttachYodlee?csId=#{@$el.find("option:selected").val()}&bankName=#{this.$el.find("input[type='radio'][name='Bank']:checked").attr('value')}"
        @$el.find("#yodleeContinueBtn").attr("href", url)
        @$el.find("#yodleeLinkAccountBtn").removeClass('disabled')
        
    bankChanged: ->
        @$el.find("input[type='radio'][name!='Bank']:checked").removeAttr('checked')
        currentSubBanks = @$el.find(".SubBank:not([class*='hide'])")
        currentSubBanks.addClass('hide')  
        @$el.find("#subTypeHeader[class*='hide']").removeClass('hide') 
        currentSubBanks.find('option').removeAttr('selected')
        bank = @$el.find("input[type='radio'][name='Bank']:checked").val()
        @$el.find("." + bank + "Container").removeClass('hide')
        $("#yodleeLinkAccountBtn:not([class*='disabled'])").addClass('disabled')
    
    linkAccountClicked: ->
        return false if @$el.find('#yodleeLinkAccountBtn').hasClass('disabled')
        @$el.find('.yodlee_help').colorbox({ inline:true, transition: 'none' });
    
    parentBankSelected: (evt)->
        evt.preventDefault()
        @$el.find('#Bank_' + evt.currentTarget.id).click()
        @$el.find('span.on').removeClass('on').addClass('off')
        $(evt.currentTarget).find('span.off').removeClass('off').addClass('on')
        return

    render: ->
        super()
        $.colorbox.close()
        
        return @

    serializeData: ->
        return {YodleeBanks: @YodleeBanks.toJSON()}

    back: ->
        @trigger 'back'
        false

    getDocumentTitle: ->
        "Link Yodlee Account"
        
class EzBob.YodleeAccountModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/YodleeMarketPlaces/Accounts"

class EzBob.YodleeAccounts extends Backbone.Collection
    model: EzBob.YodleeAccountModel
    url: "#{window.gRootPath}Customer/YodleeMarketPlaces/Accounts"
    
class EzBob.YodleeBankModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/YodleeMarketPlaces/Banks"

class EzBob.YodleeBanks extends Backbone.Collection
    model: EzBob.YodleeBankModel
    url: "#{window.gRootPath}Customer/YodleeMarketPlaces/Banks"
    

