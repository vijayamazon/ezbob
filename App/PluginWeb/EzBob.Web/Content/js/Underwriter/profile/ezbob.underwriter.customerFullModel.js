var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.CustomerFullModel = Backbone.Model.extend({
	url: function() {
		return '' + window.gRootPath + 'Underwriter/FullCustomer/Index/?id=' + this.get('customerId') + '&history=' + this.get('history');
	}, // url
}); // EzBob.Underwriter.CustomerFullModel
