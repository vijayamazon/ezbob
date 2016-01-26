var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ManageInvestorDetails = Backbone.Marionette.ItemView.extend({
    template: '#manage-investor-details-template',
    initialize: function(options) {

        this.model.on('change reset', this.render, this);
        this.model.set('InvestorID', options.InvestorID, { silent: true });

        this.model.set('Details', options.Details, { silent: true });
        // this.model.on("closeDetails", this.cancel);

    }, //initialize

    events: {
        'click #investorDetailsSubmit': 'submit',
        'click #investorDetailsCancel': 'cancel'

    }, //events

    ui: {
        'form': '#addEditInvestorDetilasForm',
        'phone': '.phone',

        'numeric': '.numeric',
        'CompanyName': '#CompanyName',
        'InvestorType': '#InvestorType',
        'IsActive': '#IsActive',
        'money': '.cashInput',
        'FundingLimitForNotification': '#FundingLimitForNotification',

      
        
		
	},//ui

	onRender: function () {
		this.ui.numeric.numericOnly(20);
		this.editID = this.model.get('InvestorID');
		this.ui.money.moneyFormat();
		var self = this;
		if (this.editID) {
			var details = this.model.get('Details');
			if (details) {
				this.ui.CompanyName.val(details['CompanyName']).change();
				this.ui.InvestorType.val(details['InvestorTypeID']).change();
				this.ui.IsActive.prop('checked', details['IsActive']).change();
				this.ui.FundingLimitForNotification.val(details['FundingLimitForNotification']).blur();
			}
		}
	
		this.ui.form.validate({
			rules: {
				CompanyName: { required: true },
				InvestorType: { required: true },
				
			},
			messages: {
			},
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlight,
			ignore: ':not(:visible)'
		});
		
	},//onRender

	submit: function(){
		if (!this.ui.form.valid()) {
			return false;
		}
		BlockUi();
		var investorID = this.model.get('InvestorID');
		var data = this.ui.form.serializeArray();
		data.push({ name: 'InvestorID', value: this.model.get('InvestorID') });
		var self = this;
		_.find(data, function(d) {
		    return d.name === 'FundingLimitForNotification';
		}).value = self.$el.find('#FundingLimitForNotification').autoNumeric('get');
		

		var xhr = $.post('' + window.gRootPath + 'Underwriter/Investor/ManageInvestorDetails', data);
		xhr.done(function(res) {
			if (res.success) {
				EzBob.ShowMessage('Investor details updated successfully', 'Done', null, 'Ok');
				self.trigger('cancel');
               
				$(".manage-investor-row[data-id='" + investorID + "']").click();
			} else {
				EzBob.ShowMessage(res.error, 'Failed saving investor bank', null, 'Ok');
			}
			self.trigger('submitDetails',investorID);
		});

		xhr.fail(function (res) {
			EzBob.ShowMessage(res, 'Failed saving investor details', null, 'Ok');
		});
		xhr.always(function() {
			UnBlockUi();
		});

		return false;
	},//end of submit

	cancel: function () {
	    this.remove();
	    this.unbind();
	    this.model.unbind("change reset", this.modelChanged);

	 //   $('.add-contact-row').show();
	    return false;
		
	} //cancel
});//EzBob.Underwriter.ManageInvestorBankView