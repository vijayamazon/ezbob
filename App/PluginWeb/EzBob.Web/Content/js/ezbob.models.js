var EzBob = EzBob || {};

EzBob.AddressModel = Backbone.Model.extend({
	defaults: {
		Found: '',
		Credits_display_text: '',
		Accountadminpage: '',
		Errormessage: '',
		Id: '',
		Organisation: '',
		Line1: '',
		Line2: '',
		Line3: '',
		Town: '',
		County: '',
		Postcode: '',
		Country: '',
		Rawpostcode: '',
		Deliveryposuffix: '',
		Nohouseholds: '',
		Smallorg: '',
		Pobox: '',
		Mailsortcod: '',
		Udprn: ''
	}, // defaults
	idAttribute: "AddressId",
	url: function() {
		var rootPath = window.gRootPath;

		if (!window.gRootPath) {
			console.warn('window.gRootPath is not initialized!');
			rootPath = '/';
		} // if

		return rootPath + 'Postcode/GetFullAddressFromId?id=' + this.get('Id');
	}, // url

	initialize: function () {
	}, // initialize
}); // EzBob.AddressModel

EzBob.AddressModels = Backbone.Collection.extend({
	addresses: EzBob.AddressModel
}); // EzBob.AddressModels

EzBob.DirectorModel = Backbone.Model.extend({
	initialize: function() {
		this.set('DirectorAddress', new EzBob.AddressModels(this.get('DirectorAddress')));
	}, // initialize
}); // EzBob.DirectorModel

EzBob.Directors = Backbone.Collection.extend({
	model: EzBob.DirectorModel
}); // EzBob.Directors

EzBob.CompanyModel = Backbone.Model.extend({
	urlRoot: window.gRootPath + "Underwriter/CompanyScore/ChangeCompany",
	
	initialize: function () {
		this.set('CompanyAddress', new EzBob.AddressModels(this.get('CompanyAddress')));
	}, // initialize
}); // EzBob.DirectorModel
