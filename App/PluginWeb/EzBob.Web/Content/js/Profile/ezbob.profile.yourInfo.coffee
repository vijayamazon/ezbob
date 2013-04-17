﻿root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Profile = EzBob.Profile or {}

class EzBob.Profile.YourInfoMainView extends Backbone.Marionette.Layout
    template: "#your-info-template"
    
    initialize: ->
        @isAddressValidation = true

    events: 
        'click .edit-personal': 'editPersonalViewShow'
        'click .submit-personal': 'saveData'
        'click .cancel': 'reload'
        'change .personEditInput': 'inputChanged'
        'keyup .personEditInput': 'inputChanged'

    ui:
       form: "form.editYourInfoForm"
     
    setInputReadOnly:(isReadOnly) ->
        @.$el.find('.personEditInput').attr('readonly', isReadOnly).attr('modifed', !isReadOnly)
        @.$el.find('.addAddressInput').attr('modifed', !isReadOnly)
        if isReadOnly
            @.$el.find('.submit-personal, .cancel, .addAddressInput, .addAddress, .removeAddress').hide()
            @.$el.find('.edit-personal').show()
         else 
            @.$el.find('.submit-personal, .cancel, .addAddressInput, .addAddress, .removeAddress').show()
            @.$el.find('.edit-personal').hide()

    editPersonalViewShow: ->
        @setInputReadOnly false

     addressModelChange: ->
        adress = @model.get 'PersonalAddress'
        @addressValidation adress, '#PersonalAddress'
        typeOfBusinessName = @model.get 'BusinessTypeReduced'

        if typeOfBusinessName == "Limited"
            adress = @model.get 'LimitedCompanyAddress'
            @addressValidation adress, '#LimitedCompanyAddress'

        else if typeOfBusinessName == "NonLimited"
            adress = @model.get 'NonLimitedCompanyAddress'
            @addressValidation adress, '#NonLimitedAddress'

    addressValidation: (address, element) -> 
        @isAddressValidation = address.length > 0
        @setError element, !@isAddressValidation

    setError: (element, isError) -> 
        if isError
            @addAddressError element
        else 
            @clearAddressError element
    
    addAddressError: (el) ->
        error = $('<label class="error" generated="true">This field is required</label>')
        EzBob.Validation.errorPlacement error, @$el.find(el)
    
    clearAddressError: (el) ->
        EzBob.Validation.unhighlight @$el.find(el)
    
    saveData: ->
        if !@validator.form() or !@isAddressValidation
            EzBob.App.trigger("error", "You must fill in all of the fields.")
            return false;

        data = @ui.form.serializeArray()
        action = @ui.form.attr('action')
        that = @
        request = $.post action, data;

        request.success => 
            that.reload()
            EzBob.App.trigger 'info', "Your information updated successfully"

        request.fail () =>
            EzBob.App.trigger 'error', "Business check service temporary unavaliable, please contact with system administrator", ""
      
    reload: -> 
        xhr = this.model.fetch()
        that = @
        xhr.done => 
            that.render()
            scrollTop()
            that.setInputReadOnly true

    regions:
        personal: '.personal-info'
        company: '.company-info'

    inputChanged: ->
        @isValid = !@validator.form() or !@isAddressValidation
        @$el.find('.submit-personal').toggleClass 'disabled', @isValid

    onRender: ->
        @renderPersonal()
        typeOfBusinessName = this.model.get 'BusinessTypeReduced'
        
        if typeOfBusinessName == "Limited"
            @renderLimited()
        else if typeOfBusinessName == "NonLimited"
            @renderNonLimited()

        @setInputReadOnly true
        @validator = EzBob.validateYourInfoEditForm(@ui.form)
        @.$el.find('.phonenumber').numericOnly(11);
        @.$el.find('.cashInput').numericOnly(11);
    
    renderPersonal: ->
        personalInfoView = new EzBob.Profile.PersonalInfoView({ model: @model })
        @model.get('PersonalAddress').on "all", @addressModelChange, @
        @personal.show(personalInfoView)

    renderNonLimited: ->
        view = new EzBob.Profile.NonLimitedInfoView({ model: @model });
        @model.get('NonLimitedCompanyAddress').on "all", @addressModelChange, @
        @company.show(view)

    renderLimited: ->
        view = new EzBob.Profile.LimitedInfoView({ model: @model });
        @model.get('LimitedCompanyAddress').on "all", @addressModelChange, @
        @company.show(view)

##############
class EzBob.Profile.PersonalInfoView extends Backbone.Marionette.Layout
    template: "#personal-info-template"

    regions: 
        personAddress: '#PersonalAddress'

    onRender: ->
        address = new EzBob.AddressView({ model: @model.get('PersonalAddress'), name: "PersonalAddress", max: 10, isShowClear:true })
        @personAddress.show(address)
        @

##############
class EzBob.Profile.NonLimitedInfoView extends Backbone.Marionette.Layout
    template: "#nonlimited-info-template"

    regions: 
        nonlimitedAddress: '#NonLimitedAddress'
        director: '.director-conteiner' 

    onRender: ->
        address = new EzBob.AddressView({ model: @model.get('NonLimitedCompanyAddress'), name: "NonLimitedCompanyAddress", max: 10, isShowClear:true })
        @nonlimitedAddress.show(address)
        
        directors = @model.get("NonLimitedInfo").Directors;
        
        if directors isnt null and directors.length isnt 0
            directorView = new EzBob.Profile.DirectorCompositeView ({collection: new EzBob.Directors(directors)}) 
            @director.show (directorView)
        @

##############
class EzBob.Profile.LimitedInfoView extends Backbone.Marionette.Layout
    template: "#limited-info-template"

    regions: 
        limitedAddress: '#LimitedCompanyAddress'
        director: '.director-conteiner' 

    onRender: ->
        address = new EzBob.AddressView({ model: @model.get('LimitedCompanyAddress'), name: "LimitedCompanyAddress", max: 10, isShowClear:true })
        @limitedAddress.show(address)
        
        directors = @model.get("LimitedInfo").Directors;

        if directors isnt null and directors.length isnt 0
            directorView = new EzBob.Profile.DirectorCompositeView ({collection: new EzBob.Directors(directors)}) 
            @director.show (directorView)
        @

class EzBob.Profile.DirectorInfoView extends Backbone.Marionette.Layout
    template: '#director-info-template'

    regions: 
        directorAddress: '#DirectorAddress'

    onRender: ->
        address = new EzBob.AddressView({ model: @model.get('DirectorAddress'), name: "DirectorAddress", max: 10, isShowClear:false })
        @directorAddress.show(address)

class EzBob.Profile.DirectorCompositeView extends Backbone.Marionette.CompositeView
    template: "#directors-info"
    itemView: EzBob.Profile.DirectorInfoView
    itemViewContainer:'div'
