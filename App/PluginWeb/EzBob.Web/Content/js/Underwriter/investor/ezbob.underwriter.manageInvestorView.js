var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ManageInvestorView = Backbone.Marionette.ItemView.extend({
    template: '#manage-investor-template',
    initialize: function() {
        this.model = new EzBob.Underwriter.InvestorModel();
        this.model.on('change reset', this.render, this);
       
      
        this.views = {
            details: { view: this.investorDetailsView },
            manageBank: { view: this.manageInvestorBankView },
            manageContact: { view: this.manageInvestorContactView },
        };

        return this;
    }, //initialize

    ui: {
        manageRegion: '#manage-investor-region'

    },
    serializeData: function() {
        return {
            data: this.model.toJSON() //take the list from the model after fatch
        };
    },
    events: {
        'click #SubmitContactsEdit': 'SubmitContactsEdit',
        'click #addInvestorContact': 'manageInvestorContactView',
        'click #addInvestorBank': 'manageInvestorBankView',
        'click #CancelContactsEdit': 'CancelContactsEdit',
        'click #CancelBanksEdit': 'CancelContactsEdit',
        'click .edit-investor-contact-btn': 'ContactInvestorEdit',
        'click #SubmitBanksEdit': 'SubmitBanksEdit',
        'click .edit-investor-bank-btn': 'BankInvestorEdit',
       

    },
    //stas region?
    onRender: function() {
       
        this.initSwitch(".Investor-IsGettingAlerts-contact", this.IsContactGettingAlertsChange);
        this.initSwitch(".Investor-IsGettingReports-contact", this.IsContactGettingReportsChange);
        this.initSwitch(".Investor-primary-contact", this.IsContactPrimaryChange);
        this.initSwitch(".Investor-active-contact", this.IsContactActiveChange);
        this.initSwitch(".Investor-active-bank", this.IsBankActiveChange);
        return this;
    }, //onRender

    show: function(id) {
        this.model.set('InvestorID', id);
        //this.stateModel.set({ state: 'details' }, { silent: true });
        var self = this;
        this.model.fetch().done(function() {
            self.model.set('Contacts', new EzBob.Underwriter.InvestorContactModels(self.model.get('Contacts'), 'InvestorContactID'), { silent: true });
            self.model.set('Banks', new EzBob.Underwriter.InvestorBankModels(self.model.get('Banks')), { silent: true });

            //  console.log(self.model.get('Contacts').pluck("InvestorContactID"));
            self.$el.show();
        });
    }, //show

    hide: function() {
        return this.$el.hide();
    },

    initSwitch: function(elemClass, func) {
        this.$el.find(elemClass).bootstrapSwitch();

        var self = this;

        this.$el.find(elemClass).on('switch-change', function(event, innerState) {
            func.call(self, event, innerState);
        });
    }, // initSwitch
    IsContactGettingReportsChange :function(event, state, innerState) {
        var tr = $(event.currentTarget).closest('tr');
        var id = tr.data('id');
       
        var tochange = this.model.get('Contacts').find(function (item) {
            return Number(item.get('InvestorContactID')) === id;
        }); //get the model that need to get changed
        tochange.set('IsGettingReports', state.value);
    },
    IsContactGettingAlertsChange :function(event, state, innerState) {
        var tr = $(event.currentTarget).closest('tr');
        var id = tr.data('id');

        var tochange = this.model.get('Contacts').find(function (item) {
            return Number(item.get('InvestorContactID')) === id;
        }); //get the model that need to get changed
        tochange.set('IsGettingAlerts', state.value);
    },
    IsContactPrimaryChange: function(event, state, innerState) {
        var tr = $(event.currentTarget).closest('tr');
        var id = tr.data('id');
        var activeCheck = tr.find('.Investor-active-contact');
        var tochange = this.model.get('Contacts').find(function(item) {
            return Number(item.get('InvestorContactID')) === id;
        }); //get the model that need to get changed
        var found = this.model.get('Contacts').find(function(item) {
            return Number(item.get('IsPrimary')) === 1 && Number(item.get('InvestorContactID')) !== id;
        }); //check if there is another primary contact
        

        var ContactsLength = this.model.get('Contacts').length;
        if (((ContactsLength > 1 && !found) || ContactsLength === 1) ) {
            
            $(event.currentTarget).bootstrapSwitch('setState', true, true);
            tochange.set('IsPrimary', true);
            $(activeCheck).bootstrapSwitch('setState', true, true);
            tochange.set('IsActive', true);

        } else if (ContactsLength > 1) {
            $('.Investor-primary-contact').bootstrapSwitch('setState', false, true);
            $(event.currentTarget).bootstrapSwitch('setState', true, true);
            found.set('IsPrimary', false);
            tochange.set('IsPrimary', true);
            $(activeCheck).bootstrapSwitch('setState', true, true);
            tochange.set('IsActive', true);
        }
        return false;
    },

    IsContactActiveChange: function(event, state, innerState) {
        var tr = $(event.currentTarget).closest('tr');
        var id = tr.data('id');
        var tochange = this.model.get('Contacts').find(function(item) {
            return Number(item.get('InvestorContactID')) === id;
        });
        var tochangePrimaryVal = tochange.get('IsPrimary');
        var found = this.model.get('Contacts').find(function(item) {
            return Number(item.get('IsActive')) === 1 && Number(item.get('InvestorContactID')) !== id;
        });
        if (!found || tochangePrimaryVal===true) {
            $(event.currentTarget).bootstrapSwitch('setState', true, true);
            tochange.set('IsActive', true);
        } else {
            tochange.set('IsActive', state.value);
        }
        return false;
    },
    IsBankActiveChange: function(event, state, innerState) {
        var tr = $(event.currentTarget).closest('tr');
        var id = tr.data('id');

        var tochange = this.model.get('Banks').find(function(item) {
            return Number(item.get('InvestorBankAccountID')) === id;
        });
        var tochangeAccountType = tochange.get('AccountType');
        var tochangeAccountTypeStr = tochange.get('AccountTypeStr');

        var found = this.model.get('Banks').find(function(item) {
            return Number(item.get('IsActive')) === 1 && Number(item.get('InvestorBankAccountID')) !== id && Number(item.get('AccountType')) === tochangeAccountType;
        });
        var ContactsLength = this.model.get('Banks').length;
        if ((ContactsLength > 1 && !found) || ContactsLength === 1) {

            $(event.currentTarget).bootstrapSwitch('setState', true, true);
            tochange.set('IsActive', true);

        } else if (ContactsLength > 1) {
            $('.Investor-active-bank-' + tochangeAccountTypeStr).bootstrapSwitch('setState', false, true);
            $(event.currentTarget).bootstrapSwitch('setState', true, true);
            found.set('IsActive', false);
            tochange.set('IsActive', true);
        }
        return false;
    },
    SubmitContactsEdit: function() {
        var Contactlist = this.model.toJSON();
        var serialized = JSON.stringify(Contactlist);

        var xhr = $.post('' + window.gRootPath + 'Underwriter/Investor/SaveInvestorContactList', {
            'investor': serialized,
            'InvestorID': this.model.get('InvestorID')
        });
        var self = this;
        xhr.done(function(res) {

            if (res.success) {
                EzBob.ShowMessage('Contacts Edited successfully', 'Done', null, 'Ok');
            } else {
                EzBob.ShowMessage(res.error, "Failed saveing investor contact list", null, 'Ok');
            }
        });
    },
    SubmitBanksEdit: function() {
        var Bankslist = this.model.toJSON();
        var serialized = JSON.stringify(Bankslist);

        var xhr = $.post('' + window.gRootPath + 'Underwriter/Investor/SaveInvestorBanksList', {
            'investor': serialized,
            'InvestorID': this.model.get('InvestorID')
        });
        var self = this;
        xhr.done(function(res) {

            if (res.success) {
                EzBob.ShowMessage('Banks Edited successfully', 'Done', null, 'Ok');
            } else {
                EzBob.ShowMessage(res.error, "Failed saveing investor contact list", null, 'Ok');
            }
        });
    },
    CancelContactsEdit: function() {
        var self = this;
        this.model.fetch().done(function() {
            self.model.set('Contacts', new EzBob.Underwriter.InvestorContactModels(self.model.get('Contacts'), 'InvestorContactID'));
            self.model.set('Banks', new EzBob.Underwriter.InvestorBankModels(self.model.get('Banks')));
        });
    },

    investorDetailsView: function(self) {
        var view = new EzBob.Underwriter.ManageInvestorDetailsView({
            model: self.model,
        });
        view.on('manageBank', self.manageBank, self);
        /*view.on('manageContact', self.manageContact, self);*/
        view.on('saveContactChanges', self.saveContactChanges, self);
        view.on('cancelContactChange', self.cancelContactChange, self);
        return view;
    }, //investorDetailsView

    manageInvestorBankView: function(el) {
        var tr = $(el.currentTarget).closest('tr');
        this.$el.find('.edit-investor-bank').remove();
        this.$el.find('.bank-investor-row').removeClass('active');
        this.editBankView = null;
        var newRow = $('<tr class="add-bank-view-area"><td colspan="12"  class="td-no-padding"></td></tr>');
        tr.after(newRow);
        var newRowEl = this.$el.find('tr.add-bank-view-area td');
        var addBankView = new EzBob.Underwriter.ManageInvestorBankView({
            el: newRowEl,
            model: new EzBob.Underwriter.InvestorBankmodel(),
            InvestorID: this.model.get('InvestorID'),
        });
        addBankView.render();
        $(tr).hide();

       
    }, //manageInvestorBankView
   

    manageInvestorContactView: function(el) {
        var tr = $(el.currentTarget).closest('tr');
        this.$el.find('.contact-investor-row').removeClass('active');
        this.$el.find('.edit-investor-contact').remove();
        this.editContactView = null;
        var newRow = $('<tr class="add-contact-view-area"><td colspan="12"  class="td-no-padding"></td></tr>');
        tr.after(newRow);
        var newRowEl = this.$el.find('tr.add-contact-view-area td');
        var addContactView = new EzBob.Underwriter.ManageInvestorContactView({
            el: newRowEl,
            model: new EzBob.Underwriter.InvestorContactModel(),
            InvestorID: this.model.get('InvestorID'),
            
         });
        
        addContactView.render();
        $(tr).hide();

    }, //manageInvestorContactView
    ContactInvestorEdit: function (el) {
        var tr = $(el.currentTarget).closest('tr');
        var id = tr.data('id');
        this.$el.find('.edit-investor-contact').remove();
        this.$el.find('.add-contact-view-area').remove();
        this.$el.find('.contact-investor-row').removeClass('active');
        $('.add-contact-row').show();
        if (this.editContactView && this.editContactView.$el.data('id') === id) {
            this.editContactView = null;
            tr.removeClass('active');
        }
        else {

            var newRow = $('<tr class="edit-investor-contact"><td   class="td-no-padding" data-id="' + id + '" colspan="13"></td></tr>');
            tr.after(newRow);
            var newRowEl = this.$el.find('tr.edit-investor-contact td');
            var contact = this.model.get('Contacts').find(function (item) {
                return Number(item.get('InvestorContactID')) === id;
            });
            this.editContactView = new EzBob.Underwriter.ManageInvestorContactView({
                el: newRowEl,
                model: new EzBob.Underwriter.InvestorContactModel(),
                InvestorID: this.model.get('InvestorID'),
                Contact: contact,
                EditID: id

            });
            tr.addClass('active');
            this.editContactView.render();

        }
       

    },
    BankInvestorEdit : function(el) {
           var tr = $(el.currentTarget).closest('tr');
        var id = tr.data('id');
        this.$el.find('.edit-investor-bank').remove();
       this.$el.find('.add-bank-view-area').remove();
        $('.add-bank-row').show();
        this.$el.find('.bank-investor-row').removeClass('active');
        if (this.editBankView && this.editBankView.$el.data('id') === id) {
            this.editBankView = null;
            tr.removeClass('active');
            } else {
            tr.addClass('active');
                var newRow = $('<tr class="edit-investor-bank"><td data-id="'+id+'"  class="td-no-padding"colspan="12"></td></tr>');
                tr.after(newRow);
                var newRowEl = this.$el.find('tr.edit-investor-bank td');
                var bank = this.model.get('Banks').find(function (item) {
                    return Number(item.get('InvestorBankAccountID')) === id;
                });
                this.editBankView = new EzBob.Underwriter.ManageInvestorBankView({
                    el: newRowEl,
                    model: new EzBob.Underwriter.InvestorBankmodel(),
                    InvestorID: this.model.get('InvestorID'),
                    Bank: bank,
                    EditID: id
                });
                this.editBankView.render();
            }
    
    },
	saveContactChanges: function (id) {
	    console.log('saveContactChanges');
		return false;
	},
	cancelContactChange: function (id) {
	    console.log('cancelContactChange');
	    return false;
	},

	manageBank: function (id) {
	    this.stateModel.set('editID', id, { silent: true });
	    this.stateModel.set('state', 'manageBank');
	    return false;
	},

	manageContact: function (id) {
		this.stateModel.set('editID', id, { silent: true });
		this.stateModel.set('state', 'manageContact');
		return false;
	},

	backClicked: function () {
		this.stateModel.set('state', 'details');
		return false;
	},
    Cancel : function() {
        this.remove();
        this.unbind();
        this.model.unbind("change reset", this.modelChanged);
      
        return false;
    }
});
