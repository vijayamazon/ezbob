root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Profile = EzBob.Profile or {}

class EzBob.Profile.YourInfoMainView extends Backbone.Marionette.Layout
    template: '#your-info-template'

    initialize: ->
        EzBob.App.on 'dash-director-address-change', @directorModelChange, @

    events: 
        'click .edit-personal': 'editPersonalViewShow'
        'click .submit-personal': 'saveData'
        'click .cancel': 'reload'
        'change .personEditInput': 'inputChanged'
        'keyup .personEditInput': 'inputChanged'

    ui:
       form: 'form.editYourInfoForm'

    setInputReadOnly:(isReadOnly) ->
        # TODO: why is it 'modifed' and not 'modified' & where is it even used
        @.$el.find('.personEditInput').attr('readonly', isReadOnly).attr('modifed', !isReadOnly)
        @.$el.find('#PersonalAddress .addAddressInput').attr('modifed', !isReadOnly)
        @.$el.find('#OtherPropertiesAddresses .addAddressInput').attr('modifed', !isReadOnly)

        if isReadOnly
            @.$el.find('.submit-personal, .cancel,#PersonalAddress .addAddressInput,#PersonalAddress .addAddress,#PersonalAddress .removeAddress,#PersonalAddress .attardi-input,#PersonalAddress .required').hide()

            otherPropertiesModels = @model.get('OtherPropertiesAddresses')
            if (otherPropertiesModels == undefined || otherPropertiesModels.length < 1)
                @.$el.find('#otherPropertiesDiv').hide()
            else
                @.$el.find('#otherPropertiesDiv .removeAddress, #otherPropertiesDiv .addAddressContainer').hide()

            @.$el.find('textarea').removeClass('form_field').css('margin-top', 0)
            @.$el.find('.edit-personal').show()
         else 
            @.$el.find('.submit-personal, .cancel,#PersonalAddress .removeAddress').show()
            @.$el.find('#otherPropertiesDiv, #otherPropertiesDiv .removeAddress, #otherPropertiesDiv .addAddressContainer').show()

            @.$el.find('.edit-personal').hide()

    editPersonalViewShow: ->
        @setInputReadOnly false

    onAddingDirector: ->
        @ui.form.hide()

    onDirectorAdded: ->
        @ui.form.hide()
        @reload()

    onBackFromDirector: ->
        @ui.form.show()

    addressAreValid: ->
        address = @model.get 'PersonalAddress'
        if address.length < 1
            return false

        typeOfBusinessName = @model.get 'BusinessTypeReduced'

        if typeOfBusinessName == 'Limited'
            address = @model.get 'CompanyAddress'
            if address.length < 1
                return false
        else if typeOfBusinessName == 'NonLimited'
            address = @model.get 'CompanyAddress'
            if address.length < 1
                return false

        if @model.get typeOfBusinessName + 'Info'
            directors = @model.get(typeOfBusinessName + 'Info').Directors

            for dir in directors
                if dir.DirectorAddress.length < 1
                    return false

        true

    directorModelChange: (newModel) ->
        directors = @model.get(@model.get('BusinessTypeReduced') + 'Info').Directors

        _.each directors, (dir) ->
            if dir.Id == newModel.get 'Id'
                dir.DirectorAddress = newModel.get('DirectorAddress').models

        @addressModelChange()

    addressModelChange: ->
        @inputChanged()

        @setInvalidAddressLabel @model.get('PersonalAddress'), '#PersonalAddress'

        typeOfBusinessName = @model.get 'BusinessTypeReduced'

        if typeOfBusinessName == 'Limited'
            @setInvalidAddressLabel @model.get('CompanyAddress'), '#LimitedCompanyAddress'
        else if typeOfBusinessName == 'NonLimited'
            @setInvalidAddressLabel @model.get('CompanyAddress'), '#NonLimitedAddress'

        self = @

        if @model.get(typeOfBusinessName + 'Info')
            directors = @model.get(typeOfBusinessName + 'Info').Directors

            _.each directors, (dir) ->
                self.setInvalidAddressLabel dir.DirectorAddress, '.directorAddress' + dir.Id + ' #DirectorAddress', dir.Id

    setInvalidAddressLabel: (address, element, dirId) -> 
        if address.length < 1
            EzBob.Validation.addressErrorPlacement @$el.find(element), (if dirId then @model else address), dirId, @model.get 'BusinessTypeReduced'
        else 
            EzBob.Validation.unhighlight @$el.find(element)

    saveData: ->
        if !@validator.form() or !@addressAreValid()
            EzBob.App.trigger('error', 'You must fill in all of the fields.')
            return false

        typeOfBusinessName = @model.get('BusinessTypeReduced') + 'Info'

        if @model.get(typeOfBusinessName)
            directors = @model.get(typeOfBusinessName).Directors;
            _.each directors, (val) ->
                _.each val.DirectorAddress, (add)->
                    add['DirectorId'] = val.Id

        data = @ui.form.serializeArray()
        action = @ui.form.attr('action')
        request = $.post action, data

        request.done => 
            @reload()
            EzBob.App.trigger 'info', 'Your information updated successfully'

        request.fail () =>
            EzBob.App.trigger 'error', 'Business check service temporary unavaliable, please contact with system administrator', ''

    reload: -> 
        @model.fetch()
        .done =>
            @render()
            scrollTop()
            @setInputReadOnly true

    regions:
        personal: '.personal-info'
        company: '.company-info'

    inputChanged: ->
        isInvalid = not @validator.form() or not @addressAreValid()
        @$el.find('.submit-personal').toggleClass('disabled', isInvalid).prop('disabled', isInvalid)

    onRender: ->
        @renderPersonal()

        typeOfBusinessName = this.model.get 'BusinessTypeReduced'

        if typeOfBusinessName == 'Limited'
            @renderLimited()
        else if typeOfBusinessName == 'NonLimited'
            @renderNonLimited()

        @setInputReadOnly true
        @validator = EzBob.validateYourInfoEditForm(@ui.form)
        @.$el.find('.phonenumber').numericOnly(11);
        @.$el.find('.cashInput').numericOnly(15);
        $('input.form_field_right_side').css 'margin-left', '3px'
        EzBob.UiAction.registerView @

    renderPersonal: ->
        personalInfoView = new EzBob.Profile.PersonalInfoView({ model: @model })
        @model.get('PersonalAddress').on 'all', @addressModelChange, @
        @personal.show(personalInfoView)

    renderNonLimited: ->
        view = new EzBob.Profile.NonLimitedInfoView({ model: @model, parentView: @ });
        @model.get('CompanyAddress').on 'all', @addressModelChange, @
        @company.show(view)

    renderLimited: ->
        view = new EzBob.Profile.LimitedInfoView({ model: @model, parentView: @ });
        @model.get('CompanyAddress').on 'all', @addressModelChange, @
        @company.show(view)

class EzBob.Profile.PersonalInfoView extends Backbone.Marionette.Layout
    template: '#personal-info-template'

    regions: 
        personAddress: '#PersonalAddress'
        otherPropertiesAddresses: '#OtherPropertiesAddresses'

    onRender: ->
        address = new EzBob.AddressView({
            model: @model.get('PersonalAddress'),
            name: 'PersonalAddress',
            max: 10,
            isShowClear: true,
            uiEventControlIdPrefix: @personAddress.getEl(@personAddress.el).attr('data-ui-event-control-id-prefix'),
        })
        @personAddress.show(address)

        otherPropertiesAddressesView = new EzBob.AddressView({
            model: @model.get('OtherPropertiesAddresses'),
            name: 'OtherPropertiesAddresses',
            max: 3,
            required: "empty",
            isShowClear: true,
            uiEventControlIdPrefix: @otherPropertiesAddresses.getEl(@otherPropertiesAddresses.el).attr('data-ui-event-control-id-prefix')
        })
        @otherPropertiesAddresses.show(otherPropertiesAddressesView)

        @

class EzBob.Profile.NonLimitedInfoView extends Backbone.Marionette.Layout
    template: '#nonlimited-info-template'

    initialize: (options) ->
        @parentView = options.parentView
        @lastTimeDupDirFound = false

    regions: 
        nonlimitedAddress: '#NonLimitedAddress'
        director: '.director-container' 

    events:
        "click .add-director": "addDirectorClicked"

    onRender: ->
        address = new EzBob.AddressView({
            model: @model.get('CompanyAddress'),
            name: 'NonLimitedCompanyAddress',
            max: 10,
            isShowClear: true,
            uiEventControlIdPrefix: @nonlimitedAddress.getEl(@nonlimitedAddress.el).attr('data-ui-event-control-id-prefix'),
        })
        @nonlimitedAddress.show(address)

        directors = @model.get('CompanyInfo').Directors;

        if directors isnt null and directors.length isnt 0
            directorView = new EzBob.Profile.DirectorCompositeView ({collection: new EzBob.Directors(directors)}) 
            @director.show (directorView)

        if not @model.get('IsOffline')
            @$el.find('.offline').remove()
        else
            @$el.find('.notoffline').remove()

        @

    addDirectorClicked: (event) ->
        event.stopPropagation()
        event.preventDefault()

        @parentView.onAddingDirector()

        director = new EzBob.DirectorModel()
        directorEl = $('.add-director-container')

        customerInfo = _.extend({},
            @model.get('CustomerPersonalInfo'),
            { PostCode: @model.get('PersonalAddress').models[0].get('Rawpostcode'), },
            { Directors: @model.get('CompanyInfo').Directors }
        )

        if(!@addDirector)
            @addDirector = new EzBob.AddDirectorInfoView({ model: director, el: directorEl, customerInfo: customerInfo, failOnDuplicate: true })
            @addDirector.setBackHandler ( =>
                directorEl.hide()
                @parentView.onBackFromDirector()
            )
            @addDirector.setSuccessHandler ( =>
                directorEl.hide()
                @parentView.onDirectorAdded()
            )
            @addDirector.setDupCheckCompleteHandler ( (bDupFound) =>
                if bDupFound
                    unless @lastTimeDupDirFound
                        EzBob.App.trigger 'clear'
                        EzBob.App.trigger 'error', 'Duplicate director detected.'

                    @lastTimeDupDirFound = true
                else
                    EzBob.App.trigger 'clear'
                    @lastTimeDupDirFound = false
            )
            @addDirector.render()

        directorEl.show()
        false

class EzBob.Profile.LimitedInfoView extends Backbone.Marionette.Layout
    template: '#limited-info-template'

    initialize: (options) ->
        @parentView = options.parentView
        @lastTimeDupDirFound = false

    regions: 
        limitedAddress: '#LimitedCompanyAddress'
        director: '.director-container' 

    events:
        "click .add-director": "addDirectorClicked"

    onRender: ->
        address = new EzBob.AddressView({
            model: @model.get('CompanyAddress'),
            name: 'LimitedCompanyAddress',
            max: 10,
            isShowClear: true,
            uiEventControlIdPrefix: @limitedAddress.getEl(@limitedAddress.el).attr('data-ui-event-control-id-prefix'),
        })
        @limitedAddress.show(address)
        
        directors = @model.get('CompanyInfo').Directors;

        if directors isnt null and directors.length isnt 0
            directors = _.filter(directors, (director) ->
                return not director.IsExperianDirector and not director.IsExperianShareholder;
            )
            directorView = new EzBob.Profile.DirectorCompositeView ({collection: new EzBob.Directors(directors)}) 
            @director.show (directorView)

        if not @model.get('IsOffline')
            @$el.find('.offline').remove()

        @

    addDirectorClicked: (event) ->
        event.stopPropagation()
        event.preventDefault()

        @parentView.onAddingDirector()

        director = new EzBob.DirectorModel()
        directorEl = $('.add-director-container')

        customerInfo = _.extend({},
            @model.get('CustomerPersonalInfo'),
            { PostCode: @model.get('PersonalAddress').models[0].get('Rawpostcode'), },
            { Directors: @model.get('CompanyInfo').Directors }
        )

        if(!@addDirector)
            @addDirector = new EzBob.AddDirectorInfoView({ model: director, el: directorEl, customerInfo: customerInfo, failOnDuplicate: true })
            @addDirector.setBackHandler ( =>
                directorEl.hide()
                @parentView.onBackFromDirector()
            )
            @addDirector.setSuccessHandler ( =>
                directorEl.hide()
                @parentView.onDirectorAdded()
            )
            @addDirector.setDupCheckCompleteHandler ( (bDupFound) =>
                if bDupFound
                    unless @lastTimeDupDirFound
                        EzBob.App.trigger 'clear'
                        EzBob.App.trigger 'error', 'Duplicate director detected.'

                    @lastTimeDupDirFound = true
                else
                    EzBob.App.trigger 'clear'
                    @lastTimeDupDirFound = false
            )
            @addDirector.render()

        directorEl.show()
        false

class EzBob.Profile.DirectorInfoView extends Backbone.Marionette.Layout
    template: '#director-info-template'

    regions: 
        directorAddress: '#DirectorAddress'

    addressModelChange: ->
        EzBob.App.trigger 'dash-director-address-change', @model

    onRender: ->
        address = new EzBob.AddressView({
            model: @model.get('DirectorAddress'),
            name: "DirectorAddress[#{@model.get('Position')}]",
            max: 10,
            isShowClear: true,
            directorId: @model.get('Id')
            uiEventControlIdPrefix: @directorAddress.getEl(@directorAddress.el).attr('data-ui-event-control-id-prefix'),
        })
        @model.get('DirectorAddress').on 'all', @addressModelChange, @
        @directorAddress.show(address)
        @$el.find('.addressEdit').addClass 'directorAddress' + @model.get 'Id'

class EzBob.Profile.DirectorCompositeView extends Backbone.Marionette.CompositeView
    template: '#directors-info'
    itemView: EzBob.Profile.DirectorInfoView
    itemViewContainer:'div'


