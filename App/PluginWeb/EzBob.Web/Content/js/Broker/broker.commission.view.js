EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.CommissionView = EzBob.Broker.BaseView.extend({
	initialize: function(options) {
	    this.properties = options.properties;
	    this.$el = $('#commission-view');
	}, // initialize

	render: function () {
	    if (this.properties.LinkedBank) {
	        this.$el.find('.linked_bank').show();
	        this.$el.find('.not_linked_bank').hide();
	    } else {
	        this.$el.find('.linked_bank').hide();
	        this.$el.find('.not_linked_bank').show();
	    }
	    var firstName = this.properties.ContactName;
	    if (this.properties.ContactName.indexOf(' ') > -1)
	        firstName = this.properties.ContactName.split(' ')[0];
        
	    this.$el.find('.broker-name').text(firstName);
	    this.$el.find('.broker-commission').text(EzBob.formatPounds(this.properties.CommissionAmount));
	    this.$el.find('.broker-approved').text(EzBob.formatPounds(this.properties.ApprovedAmount));
	}, //render

	clear: function () {

	}, // clear

	setAuthOnRender: function() {
		return false;
	}, // setAuthOnRender
	
}); // EzBob.Broker.CommissionView
