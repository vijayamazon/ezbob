var EzBob = EzBob || {};

EzBob.DirectorMainView = Backbone.View.extend({
	initialize: function(options) {
		this.preffix = options.name || 'directors';

		this.template = _.template($('#director-edit-template').html());

		this.directorTemplate = _.template($('#oneDirector').html());

		this.model = new EzBob.DirectorsModels();

		this.name = 'DirectorAddress';
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
		this.$el.find('.phonenumber').numericOnly(11);
		this.$el.find('.addressCaption').hide();

		$.each(this.model.models, function(i, val) {
			var addressElem = that.preffix + 'Address' + i,
				name = that.preffix + '[' + i + '].' + that.name,
				addressView = new EzBob.AddressView({ model: val.get('Address'), name: name, max: 1 }),
				dateOfBirthValName = that.preffix + '[' + i + '].' + 'DateOfBirth';

			val.get('Address').on('all', function() {
				that.trigger('director:addressChanged');
			});

			addressView.render().$el.appendTo(that.directorArea.find('#' + addressElem));
			SetDefaultDate(dateOfBirthValName, val.get('DateOfBirth'));
			that.addressErrorPlacement(addressView.$el, addressView.model);

			that.$el.find('.phonenumber').numericOnly(11);

			that.$el.find('input[type="text"], input[type="email"], select').blur();
			that.$el.find('input[type="radio"]:checked').click();
		}); // foreach

		this.$el.attardi_labels('toggle_all');
		this.trigger('director:change');
	}, // renderDirector

	validateAddresses: function() {
		var result = true;

		$.each(this.model.models, function(i, val) {
			if (val.get('Address').length === 0)
				result = false;
		}); // for each

		return result;
	}, // validateAddresses

	addressErrorPlacement: function(el, model) {
		var $el = $(el);

		$el.on('focusout', function() {
			if (model.length === 0) {
				var oButton = $el.find('.addAddress');

				oButton.tooltip({ title: 'Please lookup your post code' });

				$el.hover(
					function() { oButton.tooltip('show'); },
					function() { oButton.tooltip('hide'); }
				); // on hover
			} // if no address foudn
		}); // on focusout

		model.on('change', function() {
			if (model.length > 0)
				$el.find('.addAddress').tooltip('destroy');
		}); // on model change
	}, // addressErrorPlacement

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
