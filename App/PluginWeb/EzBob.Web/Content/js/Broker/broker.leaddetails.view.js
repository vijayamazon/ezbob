EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.LeadDetailsView = EzBob.Broker.BaseView.extend({
	initialize: function() {
		EzBob.Broker.LeadDetailsView.__super__.initialize.apply(this, arguments);

		this.LeadID = this.options.leadid;
		
		this.$el = $('.section-lead-details');
		this.$el.off();

	}, // initialize

	events: function() {
		var evt = {};

		evt['click .back-to-list'] = 'backToList';
		evt['click .lead-send-invitation'] = 'sendInvitation';
		evt['click .lead-fill-wizard'] = 'fillWizard';
		
		return evt;
	}, // events

	backToList: function (event) {
	    event.preventDefault();
	    event.stopPropagation();

	    this.clear();
	    location.assign('#dashboard');

	    return false;
	}, // backToList

	sendInvitation: function () {
	    var nLeadID = this.LeadID;

	    if (nLeadID < 1)
	        return;

	    BlockUi();

	    var oRequest = $.post('' + window.gRootPath + 'Broker/BrokerHome/SendInvitation', {
	        nLeadID: nLeadID,
	        sContactEmail: this.router.getAuth(),
	    });

	    var self = this;

	    oRequest.success(function (res) {
	        UnBlockUi();

	        if (res.success) {
	            EzBob.App.trigger('info', 'An invitation has been sent.');
	            self.reloadCustomerList();
	            return;
	        } // if

	        if (res.error)
	            EzBob.App.trigger('error', res.error);
	        else
	            EzBob.App.trigger('error', 'Failed to send an invitation.');
	    }); // on success

	    oRequest.fail(function () {
	        UnBlockUi();
	        EzBob.App.trigger('error', 'Failed to send an invitation.');
	    });
	}, // sendInvitation

	fillWizard: function () {
	    var nLeadID = this.LeadID;

	    if (nLeadID < 1)
	        return;

	    location.assign(
			'' + window.gRootPath + 'Broker/BrokerHome/FillWizard' +
			'?nLeadID=' + nLeadID +
			'&sContactEmail=' + encodeURIComponent(this.router.getAuth())
		);
	}, // fillWizard

	clear: function() {
		EzBob.Broker.LeadDetailsView.__super__.clear.apply(this, arguments);
    }, // clear


	render: function() {
		if (this.router.isForbidden()) {
			this.clear();
			return this;
		} // if

		this.reloadData();

		return this;
	}, // render

	reloadData: function() {
		
		var self = this;

		$.getJSON(
			window.gRootPath + 'Broker/BrokerHome/LoadLeadDetails',
			{ sLeadID: this.LeadID, sContactEmail: this.router.getAuth(), },
			function(oResponse) {
				if (!oResponse.success) {
					if (oResponse.error)
						EzBob.App.trigger('error', oResponse.error);
					else
						EzBob.App.trigger('error', 'Failed to load lead details.');

					return;
				} // if

				self.$el.find('.value').load_display_value({
					data_source: oResponse.personal_data,
					callback: function(sFieldName, oFieldValue) {
					    switch (sFieldName) {
					        case 'DateCreated':
					        case 'DateLastInvitationSent':
							    return EzBob.formatDate(oFieldValue);
						    default:
							    return oFieldValue;
						} // switch
					} // callback
				});

			    var sendInventaionButton = self.$el.find('.lead-send-invitation');
			    if(oResponse.personal_data.DateLastInvitationSent) {
			        sendInventaionButton.attr('title', 'Send another invitation to the client to fill himself.');
			    } else {
			        sendInventaionButton.attr('title', 'Send an invitation to the client to fill himself.');
			    }
			
			} // on success loading lead details
		);
	}, // reloadData

	
}); // EzBob.Broker.LeadDetailsView
