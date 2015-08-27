var EzBob = EzBob || {};

EzBob.DirectorMainView = Backbone.View.extend({
	initialize: function(options) {
		this.customerInfo = options.customerInfo;

		this.preffix = options.name || 'directors';

		this.template = _.template($('#director-edit-template').html());

		this.directorTemplate = _.template($('#oneDirector').html());

		this.model = new EzBob.DirectorsModels();

		this.name = 'DirectorAddress';

		this.lastTimeDupFound = false;
	}, // initialize

	events: {
		'click #addDirector': 'addDirector',
		'click .removeDirector': 'removeDirector',

		'change   input': 'inputChanged',
		'click    input': 'inputChanged',
		'focusout input': 'inputChanged',
		'keyup    input': 'inputChanged',

		'change   select': 'inputChanged',
		'click    select': 'inputChanged',
		'focusout select': 'inputChanged',
		'keyup    select': 'inputChanged',
	}, // events

	inputChanged: function(event) {
		var val = $(event.currentTarget).val();
		var name = $(event.currentTarget).attr('name');

		var hidden = $('[name="' + name + 'Image"]');

		if (!val && $(event.currentTarget).hasClass('nonrequired')) {
			hidden.val('empty');
			var el = event ? $(event.currentTarget) : null;
			el.closest('div').find('.field_status').field_status('set', 'empty', 2);
		}
		else
			hidden.val('ok');
	}, // inputChanged

	render: function() {
		this.$el.html(this.template());
		this.directorArea = this.$el.find('.directorArea');

		return this;
	}, // render

	renderDirector: function() {
		var that = this;
		this.directorArea.html(this.directorTemplate({ preffix: this.preffix, directors: this.model.toJSON() }));
		this.directorArea.find('.ezDateTime').splittedDateTime();

		var oFieldStatusIcons = this.$el.find('IMG.field_status');
		oFieldStatusIcons.filter('.required').field_status({ required: true });
		oFieldStatusIcons.not('.required').field_status({ required: false });

		EzBob.UiAction.registerView(this);

		this.$el.find('.alphaOnly').alphaOnly();
		this.$el.find('.phonenumber').mask('0?9999999999', { placeholder: ' ' });
		this.$el.find('.phonenumber').numericOnly(11);
		this.$el.find('.addressCaption').hide();

		$.each(this.model.models, function(i, val) {
			var addressElem = that.preffix + 'Address' + i;
			var oAddressContainer = that.directorArea.find('#' + addressElem);
			var name = that.preffix + '[' + i + '].' + that.name;

			var addressView = new EzBob.AddressView({
				model: val.get('Address'),
				name: name,
				max: 1,
				uiEventControlIdPrefix: oAddressContainer.attr('data-ui-event-control-id-prefix'),
			});
			var dateOfBirthValName = that.preffix + '[' + i + '].' + 'DateOfBirth';

			val.get('Address').on('all', function() {
				that.trigger('director:addressChanged');
			});

			addressView.render().$el.appendTo(oAddressContainer);
			SetDefaultDate(dateOfBirthValName, val.get('DateOfBirth'));

			var gender = that.preffix + '[' + i + '].' + 'FormRadioCtrl_' + val.get('Gender');
			that.$el.find('[id="' + gender + '"]').attr('checked', 'checked').change().blur();

			EzBob.Validation.addressErrorPlacement(addressView.$el, addressView.model);

			that.$el.find('.phonenumber').numericOnly(11);

			that.$el.find('input[type="text"], input[type="email"]').blur();
		    that.$el.find('select option:selected:not([value="-"])').parent().blur();
			that.$el.find('input[type="radio"]:checked').click();
		}); // foreach

		this.$el.attardi_labels('toggle_all');
		this.trigger('director:change');
	}, // renderDirector

	validateDuplicates: function() {
		this.updateModel();

		var bResult = true;

		var oKeyList = {};

		var sCustomerKey = this.detailsToKey(
			this.customerInfo.FirstName,
			this.customerInfo.Surname,
			this.customerInfo.DateOfBirth,
			null,
			this.customerInfo.Gender,
			this.customerInfo.PostCode
		);

		oKeyList[sCustomerKey] = 1;

		var self = this;

		$.each(this.model.models, function(i, val) {
			var sPostCode = '';

			if (val.get('Address').length)
				sPostCode = val.get('Address').models[0].get('Rawpostcode');

			var sDirKey = self.detailsToKey(
				val.get('Name'),
				val.get('Surname'),
				val.get('DateOfBirth'),
				'D/M/YYYY',
				val.get('Gender'),
				sPostCode
			);

			if (oKeyList[sDirKey]) {
				if (!self.lastTimeDupFound) {
					EzBob.App.trigger('clear');
					EzBob.App.trigger('error', 'Duplicate director detected.');
				} // if

				bResult = false;
				return false;
			} // if

			oKeyList[sDirKey] = 1;

			return true;
		});

		this.lastTimeDupFound = !bResult;

		if (bResult)
			EzBob.App.trigger('clear');

		return bResult;
	}, // validateDuplicates

	detailsToKey: function(sFirstName, sLastName, oBirthDate, sDateFormat, sGender, sPostCode) {
		var oDate = sDateFormat ? moment(oBirthDate, sDateFormat) : moment(oBirthDate);
		var sBirthDate = '';

		if (oDate)
			sBirthDate = oDate.utc().format('YYYY-MM-DD');

		switch (sGender) {
			case 0:
			case '0':
			case 'm':
			case 'M':
				sGender = 'M';
				break;

			case 1:
			case '1':
			case 'f':
			case 'F':
				sGender = 'F';
				break;
		} // switch

		return JSON.stringify({ f: sFirstName, l: sLastName, b: sBirthDate, g: sGender, p: sPostCode, });
	}, // detailsToKey

	validateAddresses: function() {
		var result = true;

		$.each(this.model.models, function(i, val) {
			if (val.get('Address').length === 0)
				result = false;
		}); // for each

		return result;
	}, // validateAddresses

	addDirector: function() {
		this.model.add(new EzBob.DirectorsModel({ Address: new EzBob.AddressModels() }));
		this.updateModel();
		this.renderDirector();
		return false;
	}, // addDirector

	removeDirector: function(el) {
		this.updateModel();
		var num = $(el.currentTarget).attr('number') - 1;
		this.model.remove(this.model.at(num));
		this.renderDirector();
		return false;
	}, // removeDirector

	updateModel: function() {
		var model = SerializeArrayToEasyObject(this.directorArea.parents('form').serializeArray()),
			that = this;

		$.each(this.model.models, function(i, oneModel) {
			$.each(oneModel.toJSON(), function(key) {
				$.each(model, function(k, formModelValue) {
					var valueName = that.preffix + '[' + i + '].' + key;

					if (k == valueName)
						oneModel.set(key, formModelValue);
				}); // for each model
			}); // for each oneModel
		}); // for each in this.models.model
	}, // updateModel
}); // EzBob.DirectorMainView

EzBob.DirectorsModel = Backbone.Model.extend({
	defaults: {
		Name: '',
		Middle: '',
		Surname: '',
		Gender: '',
		DateOfBirth: '',
		Address: new EzBob.AddressModels(),
		NameImage: '',
		MiddleImage: '',
		SurnameImage: '',
		GenderImage: '',
		DateOfBirthImage: '',
		Email: '',
		EmailImage: '',
		Phone: '',
		PhoneImage: '',
	}, // defaults
}); // EzBob.DirectorsModel

EzBob.DirectorsModels = Backbone.Collection.extend({ model: EzBob.DirectorModel });
