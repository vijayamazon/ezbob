var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};
EzBob.Underwriter.Settings = EzBob.Underwriter.Settings || {};

EzBob.Underwriter.Settings.LoanOfferMultiplierModel = Backbone.Model.extend({
	url: window.gRootPath + "Underwriter/StrategySettings/SettingsConfigTable?tableName=LoanOfferMultiplier"
});

EzBob.Underwriter.Settings.LoanOfferMultiplierView = Backbone.Marionette.ItemView.extend({
	template: "#loan-offer-multiplier-settings-template",
	initialize: function (options) {
		this.modelBinder = new Backbone.ModelBinder();
		this.model.on("reset change", this.render, this);
		this.update();
		return this;
	},
	events: {
		"click .addRange": "addRange",
		"click .removeRange": "removeRange",
		"click #SaveLoanOfferMultiplierSettings": "saveLoanOfferMultiplierSettings",
		"click #CancelLoanOfferMultiplierSettings": "update",
		"change .range-field": "valueChanged"
	},
	valueChanged: function (eventObject) {
		var id, newValue, ranges, row, typeIdentifier, _i, _len;
		typeIdentifier = eventObject.target.id.substring(0, 3);
		if (typeIdentifier === "end") {
			id = eventObject.target.id.substring(4);
			newValue = parseInt(eventObject.target.value);
		} else {
			id = eventObject.target.id.substring(6);
			if (typeIdentifier === "sta") {
				newValue = parseInt(eventObject.target.value);
			} else {
				newValue = parseFloat(eventObject.target.value);
			}
		}
		ranges = this.model.get('configTableEntries');
		for (_i = 0, _len = ranges.length; _i < _len; _i++) {
			row = ranges[_i];
			if (row.Id.toString() === id) {
				if (typeIdentifier === "end") {
					row.End = newValue;
				}
				if (typeIdentifier === "sta") {
					row.Start = newValue;
				}
				if (typeIdentifier === "val") {
					row.Value = newValue;
				}
				return false;
			}
		}
		return false;
	},
	saveLoanOfferMultiplierSettings: function () {
		var xhr;
		BlockUi("on");
		xhr = $.post("" + window.gRootPath + "Underwriter/StrategySettings/SaveConfigTable", {
			serializedModels: JSON.stringify(this.model.get('configTableEntries')),
			configTableType: 'LoanOfferMultiplier'
		});
		xhr.done(function (res) {
			if (res.error) {
				return EzBob.App.trigger('error', res.error);
			}
		});
		xhr.always(function () {
			return BlockUi("off");
		});
		return false;
	},
	removeRange: function (eventObject) {
		var index, rangeId, ranges, row, _i, _len;
		rangeId = eventObject.target.getAttribute('loan-offer-multiplier-id');
		index = 0;
		ranges = this.model.get('configTableEntries');
		for (_i = 0, _len = ranges.length; _i < _len; _i++) {
			row = ranges[_i];
			if (row.Id.toString() === rangeId) {
				ranges.splice(index, 1);
				this.render();
				return false;
			}
			index++;
		}
	},
	addRange: function (e, range) {
		var freeId, t, verified;
		freeId = -1;
		verified = false;
		while (!verified) {
			t = this.$el.find('#loanOfferMultiplierRow_' + freeId);
			if (t.length === 0) {
				verified = true;
			} else {
				freeId--;
			}
		}
		this.model.get('configTableEntries').push({
			Start: 0,
			Id: freeId,
			End: 0,
			Value: 0.0
		});
		this.render();
	},
	serializeData: function () {
		var data;
		data = {
			configTableEntries: this.model.get('configTableEntries')
		};
		return data;
	},
	update: function () {
		return this.model.fetch();
	},
	onRender: function () {
		var endObject, ranges, row, startObject, valueObject, _i, _len;
		if (!$("body").hasClass("role-manager")) {
			this.$el.find("select").addClass("disabled").attr({
				readonly: "readonly",
				disabled: "disabled"
			});
			this.$el.find("button").hide();
			this.$el.find("input").addClass("disabled").attr({
				readonly: "readonly",
				disabled: "disabled"
			});
		}
		ranges = this.model.get('configTableEntries');
		for (_i = 0, _len = ranges.length; _i < _len; _i++) {
			row = ranges[_i];
			startObject = this.$el.find('#start_' + row.Id);
			if (startObject.length === 1) {
				startObject.numericOnly();
			}
			endObject = this.$el.find('#end_' + row.Id);
			if (endObject.length === 1) {
				endObject.numericOnly();
			}
			valueObject = this.$el.find('#value_' + row.Id);
			if (valueObject.length === 1) {
				valueObject.autoNumeric(EzBob.percentFormat).blur();
			}
		}
		return false;
	},
	show: function (type) {
		return this.$el.show();
	},
	hide: function () {
		return this.$el.hide();
	}
});

EzBob.Underwriter.Settings.BasicInterestRateModel = Backbone.Model.extend({
	url: window.gRootPath + "Underwriter/StrategySettings/SettingsConfigTable?tableName=BasicInterestRate"
});

EzBob.Underwriter.Settings.BasicInterestRateView = Backbone.Marionette.ItemView.extend({
	template: "#basic-interest-rate-settings-template",
	initialize: function (options) {
		this.modelBinder = new Backbone.ModelBinder();
		this.model.on("reset change", this.render, this);
		this.update();
		return this;
	},
	events: {
		"click .addRange": "addRange",
		"click .removeRange": "removeRange",
		"click #SaveBasicInterestRateSettings": "saveBasicInterestRateSettings",
		"click #CancelBasicInterestRateSettings": "update",
		"change .range-field": "valueChanged"
	},
	valueChanged: function (eventObject) {
		var id, newValue, ranges, row, typeIdentifier, _i, _len;
		typeIdentifier = eventObject.target.id.substring(0, 3);
		if (typeIdentifier === "end") {
			id = eventObject.target.id.substring(4);
			newValue = parseInt(eventObject.target.value);
		} else {
			id = eventObject.target.id.substring(6);
			if (typeIdentifier === "sta") {
				newValue = parseInt(eventObject.target.value);
			} else {
				newValue = parseFloat(eventObject.target.value);
			}
		}
		ranges = this.model.get('configTableEntries');
		for (_i = 0, _len = ranges.length; _i < _len; _i++) {
			row = ranges[_i];
			if (row.Id.toString() === id) {
				if (typeIdentifier === "end") {
					row.End = newValue;
				}
				if (typeIdentifier === "sta") {
					row.Start = newValue;
				}
				if (typeIdentifier === "val") {
					row.Value = newValue;
				}
				return false;
			}
		}
		return false;
	},
	saveBasicInterestRateSettings: function () {
		var xhr;
		BlockUi("on");
		xhr = $.post("" + window.gRootPath + "Underwriter/StrategySettings/SaveConfigTable", {
			serializedModels: JSON.stringify(this.model.get('configTableEntries')),
			configTableType: 'BasicInterestRate'
		});
		xhr.done(function (res) {
			if (res.error) {
				return EzBob.App.trigger('error', res.error);
			}
		});
		xhr.always(function () {
			return BlockUi("off");
		});
		return false;
	},
	removeRange: function (eventObject) {
		var index, rangeId, ranges, row, _i, _len;
		rangeId = eventObject.target.getAttribute('basic-interest-rate-id');
		index = 0;
		ranges = this.model.get('configTableEntries');
		for (_i = 0, _len = ranges.length; _i < _len; _i++) {
			row = ranges[_i];
			if (row.Id.toString() === rangeId) {
				ranges.splice(index, 1);
				this.render();
				return false;
			}
			index++;
		}
	},
	addRange: function (e, range) {
		var freeId, t, verified;
		freeId = -1;
		verified = false;
		while (!verified) {
			t = this.$el.find('#basicInterestRateRow_' + freeId);
			if (t.length === 0) {
				verified = true;
			} else {
				freeId--;
			}
		}
		this.model.get('configTableEntries').push({
			Start: 0,
			Id: freeId,
			End: 0,
			Value: 0.0
		});
		this.render();
	},
	serializeData: function () {
		var data;
		data = {
			configTableEntries: this.model.get('configTableEntries')
		};
		return data;
	},
	update: function () {
		return this.model.fetch();
	},
	onRender: function () {
		var endObject, ranges, row, startObject, valueObject, _i, _len;
		if (!$("body").hasClass("role-manager")) {
			this.$el.find("select").addClass("disabled").attr({
				readonly: "readonly",
				disabled: "disabled"
			});
			this.$el.find("button").hide();
			this.$el.find("input").addClass("disabled").attr({
				readonly: "readonly",
				disabled: "disabled"
			});
		}
		ranges = this.model.get('configTableEntries');
		for (_i = 0, _len = ranges.length; _i < _len; _i++) {
			row = ranges[_i];
			startObject = this.$el.find('#start_' + row.Id);
			if (startObject.length === 1) {
				startObject.numericOnly();
			}
			endObject = this.$el.find('#end_' + row.Id);
			if (endObject.length === 1) {
				endObject.numericOnly();
			}
			valueObject = this.$el.find('#value_' + row.Id);
			if (valueObject.length === 1) {
				valueObject.autoNumeric(EzBob.percentFormat).blur();
			}
		}
		return false;
	},
	show: function (type) {
		return this.$el.show();
	},
	hide: function () {
		return this.$el.hide();
	}
});

EzBob.Underwriter.Settings.EuLoanMonthlyInterestModel = Backbone.Model.extend({
	url: window.gRootPath + "Underwriter/StrategySettings/SettingsConfigTable?tableName=EuLoanMonthlyInterest"
});

EzBob.Underwriter.Settings.EuLoanMonthlyInterestView = Backbone.Marionette.ItemView.extend({
	template: "#eu-loan-monthly-interest-settings-template",
	initialize: function (options) {
		this.modelBinder = new Backbone.ModelBinder();
		this.model.on("reset change", this.render, this);
		this.update();
		return this;
	},
	events: {
		"click .addRange": "addRange",
		"click .removeRange": "removeRange",
		"click #SaveEuLoanMonthlyInterestSettings": "saveEuLoanMonthlyInterestSettings",
		"click #CancelEuLoanMonthlyInterestSettings": "update",
		"change .range-field": "valueChanged"
	},
	valueChanged: function (eventObject) {
		var id, newValue, ranges, row, typeIdentifier, _i, _len;
		typeIdentifier = eventObject.target.id.substring(0, 3);
		if (typeIdentifier === "end") {
			id = eventObject.target.id.substring(4);
			newValue = parseInt(eventObject.target.value);
		} else {
			id = eventObject.target.id.substring(6);
			if (typeIdentifier === "sta") {
				newValue = parseInt(eventObject.target.value);
			} else {
				newValue = parseFloat(eventObject.target.value);
			}
		}
		ranges = this.model.get('configTableEntries');
		for (_i = 0, _len = ranges.length; _i < _len; _i++) {
			row = ranges[_i];
			if (row.Id.toString() === id) {
				if (typeIdentifier === "end") {
					row.End = newValue;
				}
				if (typeIdentifier === "sta") {
					row.Start = newValue;
				}
				if (typeIdentifier === "val") {
					row.Value = newValue;
				}
				return false;
			}
		}
		return false;
	},
	saveEuLoanMonthlyInterestSettings: function () {
		var xhr;
		BlockUi("on");
		xhr = $.post("" + window.gRootPath + "Underwriter/StrategySettings/SaveConfigTable", {
			serializedModels: JSON.stringify(this.model.get('configTableEntries')),
			configTableType: 'EuLoanMonthlyInterest'
		});
		xhr.done(function (res) {
			if (res.error) {
				return EzBob.App.trigger('error', res.error);
			}
		});
		xhr.always(function () {
			return BlockUi("off");
		});
		return false;
	},
	removeRange: function (eventObject) {
		var index, rangeId, ranges, row, _i, _len;
		rangeId = eventObject.target.getAttribute('eu-loan-monthly-interest-id');
		index = 0;
		ranges = this.model.get('configTableEntries');
		for (_i = 0, _len = ranges.length; _i < _len; _i++) {
			row = ranges[_i];
			if (row.Id.toString() === rangeId) {
				ranges.splice(index, 1);
				this.render();
				return false;
			}
			index++;
		}
	},
	addRange: function (e, range) {
		var freeId, t, verified;
		freeId = -1;
		verified = false;
		while (!verified) {
			t = this.$el.find('#euLoanMonthlyInterestRow_' + freeId);
			if (t.length === 0) {
				verified = true;
			} else {
				freeId--;
			}
		}
		this.model.get('configTableEntries').push({
			Start: 0,
			Id: freeId,
			End: 0,
			Value: 0.0
		});
		this.render();
	},
	serializeData: function () {
		var data;
		data = {
			configTableEntries: this.model.get('configTableEntries')
		};
		return data;
	},
	update: function () {
		return this.model.fetch();
	},
	onRender: function () {
		var endObject, ranges, row, startObject, valueObject, _i, _len;
		if (!$("body").hasClass("role-manager")) {
			this.$el.find("select").addClass("disabled").attr({
				readonly: "readonly",
				disabled: "disabled"
			});
			this.$el.find("button").hide();
			this.$el.find("input").addClass("disabled").attr({
				readonly: "readonly",
				disabled: "disabled"
			});
		}
		ranges = this.model.get('configTableEntries');
		for (_i = 0, _len = ranges.length; _i < _len; _i++) {
			row = ranges[_i];
			startObject = this.$el.find('#start_' + row.Id);
			if (startObject.length === 1) {
				startObject.numericOnly();
			}
			endObject = this.$el.find('#end_' + row.Id);
			if (endObject.length === 1) {
				endObject.numericOnly();
			}
			valueObject = this.$el.find('#value_' + row.Id);
			if (valueObject.length === 1) {
				valueObject.autoNumeric(EzBob.percentFormat).blur();
			}
		}
		return false;
	},
	show: function (type) {
		return this.$el.show();
	},
	hide: function () {
		return this.$el.hide();
	}
});

EzBob.Underwriter.Settings.DefaultRateCompanyModel = Backbone.Model.extend({
	url: window.gRootPath + "Underwriter/StrategySettings/SettingsConfigTable?tableName=DefaultRateCompany"
});

EzBob.Underwriter.Settings.DefaultRateCompanyView = Backbone.Marionette.ItemView.extend({
	template: "#default-rate-company-settings-template",
	initialize: function (options) {
		this.modelBinder = new Backbone.ModelBinder();
		this.model.on("reset change", this.render, this);
		this.update();
		return this;
	},
	events: {
		"click .addRange": "addRange",
		"click .removeRange": "removeRange",
		"click #SaveDefaultRateCompanySettings": "saveDefaultRateCompanySettings",
		"click #CancelDefaultRateCompanySettings": "update",
		"change .range-field": "valueChanged"
	},
	valueChanged: function (eventObject) {
		var id, newValue, ranges, row, typeIdentifier, _i, _len;
		typeIdentifier = eventObject.target.id.substring(0, 3);
		if (typeIdentifier === "end") {
			id = eventObject.target.id.substring(4);
			newValue = parseInt(eventObject.target.value);
		} else {
			id = eventObject.target.id.substring(6);
			if (typeIdentifier === "sta") {
				newValue = parseInt(eventObject.target.value);
			} else {
				newValue = parseFloat(eventObject.target.value);
			}
		}
		ranges = this.model.get('configTableEntries');
		for (_i = 0, _len = ranges.length; _i < _len; _i++) {
			row = ranges[_i];
			if (row.Id.toString() === id) {
				if (typeIdentifier === "end") {
					row.End = newValue;
				}
				if (typeIdentifier === "sta") {
					row.Start = newValue;
				}
				if (typeIdentifier === "val") {
					row.Value = newValue;
				}
				return false;
			}
		}
		return false;
	},
	saveDefaultRateCompanySettings: function () {
		var xhr;
		BlockUi("on");
		xhr = $.post("" + window.gRootPath + "Underwriter/StrategySettings/SaveConfigTable", {
			serializedModels: JSON.stringify(this.model.get('configTableEntries')),
			configTableType: 'DefaultRateCompany'
		});
		xhr.done(function (res) {
			if (res.error) {
				return EzBob.App.trigger('error', res.error);
			}
		});
		xhr.always(function () {
			return BlockUi("off");
		});
		return false;
	},
	removeRange: function (eventObject) {
		var index, rangeId, ranges, row, _i, _len;
		rangeId = eventObject.target.getAttribute('default-rate-company-id');
		index = 0;
		ranges = this.model.get('configTableEntries');
		for (_i = 0, _len = ranges.length; _i < _len; _i++) {
			row = ranges[_i];
			if (row.Id.toString() === rangeId) {
				ranges.splice(index, 1);
				this.render();
				return false;
			}
			index++;
		}
	},
	addRange: function (e, range) {
		var freeId, t, verified;
		freeId = -1;
		verified = false;
		while (!verified) {
			t = this.$el.find('#defaultRateCompanyRow_' + freeId);
			if (t.length === 0) {
				verified = true;
			} else {
				freeId--;
			}
		}
		this.model.get('configTableEntries').push({
			Start: 0,
			Id: freeId,
			End: 0,
			Value: 0.0
		});
		this.render();
	},
	serializeData: function () {
		var data;
		data = {
			configTableEntries: this.model.get('configTableEntries')
		};
		return data;
	},
	update: function () {
		return this.model.fetch();
	},
	onRender: function () {
		var endObject, ranges, row, startObject, valueObject, _i, _len;
		if (!$("body").hasClass("role-manager")) {
			this.$el.find("select").addClass("disabled").attr({
				readonly: "readonly",
				disabled: "disabled"
			});
			this.$el.find("button").hide();
			this.$el.find("input").addClass("disabled").attr({
				readonly: "readonly",
				disabled: "disabled"
			});
		}
		ranges = this.model.get('configTableEntries');
		for (_i = 0, _len = ranges.length; _i < _len; _i++) {
			row = ranges[_i];
			startObject = this.$el.find('#start_' + row.Id);
			if (startObject.length === 1) {
				startObject.numericOnly();
			}
			endObject = this.$el.find('#end_' + row.Id);
			if (endObject.length === 1) {
				endObject.numericOnly();
			}
			valueObject = this.$el.find('#value_' + row.Id);
			if (valueObject.length === 1) {
				valueObject.autoNumeric(EzBob.percentFormat).blur();
			}
		}
		return false;
	},
	show: function (type) {
		return this.$el.show();
	},
	hide: function () {
		return this.$el.hide();
	}
});

EzBob.Underwriter.Settings.DefaultRateCustomerModel = Backbone.Model.extend({
	url: window.gRootPath + "Underwriter/StrategySettings/SettingsConfigTable?tableName=DefaultRateCustomer"
});

EzBob.Underwriter.Settings.DefaultRateCustomerView = Backbone.Marionette.ItemView.extend({
	template: "#default-rate-customer-settings-template",
	initialize: function (options) {
		this.modelBinder = new Backbone.ModelBinder();
		this.model.on("reset update change", this.render, this);
		this.update();
		return this;
	},
	events: {
		"click .addRange": "addRange",
		"click .removeRange": "removeRange",
		"click #SaveDefaultRateCustomerSettings": "saveDefaultRateCustomerSettings",
		"click #CancelDefaultRateCustomerSettings": "update",
		"change .range-field": "valueChanged"
	},
	valueChanged: function (eventObject) {
		var id, newValue, ranges, row, typeIdentifier, _i, _len;
		typeIdentifier = eventObject.target.id.substring(0, 3);
		if (typeIdentifier === "end") {
			id = eventObject.target.id.substring(4);
			newValue = parseInt(eventObject.target.value);
		} else {
			id = eventObject.target.id.substring(6);
			if (typeIdentifier === "sta") {
				newValue = parseInt(eventObject.target.value);
			} else {
				newValue = parseFloat(eventObject.target.value);
			}
		}
		ranges = this.model.get('configTableEntries');
		for (_i = 0, _len = ranges.length; _i < _len; _i++) {
			row = ranges[_i];
			if (row.Id.toString() === id) {
				if (typeIdentifier === "end") {
					row.End = newValue;
				}
				if (typeIdentifier === "sta") {
					row.Start = newValue;
				}
				if (typeIdentifier === "val") {
					row.Value = newValue;
				}
				return false;
			}
		}
		return false;
	},
	saveDefaultRateCustomerSettings: function () {
		var xhr;
		BlockUi("on");
		xhr = $.post("" + window.gRootPath + "Underwriter/StrategySettings/SaveConfigTable", {
			serializedModels: JSON.stringify(this.model.get('configTableEntries')),
			configTableType: 'DefaultRateCustomer'
		});
		xhr.done(function (res) {
			if (res.error) {
				return EzBob.App.trigger('error', res.error);
			}
		});
		xhr.always(function () {
			return BlockUi("off");
		});
		return false;
	},
	removeRange: function (eventObject) {
		var index, rangeId, ranges, row, _i, _len;
		rangeId = eventObject.target.getAttribute('default-rate-customer-id');
		index = 0;
		ranges = this.model.get('configTableEntries');
		for (_i = 0, _len = ranges.length; _i < _len; _i++) {
			row = ranges[_i];
			if (row.Id.toString() === rangeId) {
				ranges.splice(index, 1);
				this.render();
				return false;
			}
			index++;
		}
	},
	addRange: function (e, range) {
		var freeId, t, verified;
		freeId = -1;
		verified = false;
		while (!verified) {
			t = this.$el.find('#defaultRateCustomerRow_' + freeId);
			if (t.length === 0) {
				verified = true;
			} else {
				freeId--;
			}
		}
		this.model.get('configTableEntries').push({
			Start: 0,
			Id: freeId,
			End: 0,
			Value: 0.0
		});
		this.render();
	},
	serializeData: function () {
		var data;
		data = {
			configTableEntries: this.model.get('configTableEntries')
		};
		return data;
	},
	update: function () {
		return this.model.fetch();
	},
	onRender: function () {
		var endObject, ranges, row, startObject, valueObject, _i, _len;
		if (!$("body").hasClass("role-manager")) {
			this.$el.find("select").addClass("disabled").attr({
				readonly: "readonly",
				disabled: "disabled"
			});
			this.$el.find("button").hide();
			this.$el.find("input").addClass("disabled").attr({
				readonly: "readonly",
				disabled: "disabled"
			});
		}
		ranges = this.model.get('configTableEntries');
		for (_i = 0, _len = ranges.length; _i < _len; _i++) {
			row = ranges[_i];
			startObject = this.$el.find('#start_' + row.Id);
			if (startObject.length === 1) {
				startObject.numericOnly();
			}
			endObject = this.$el.find('#end_' + row.Id);
			if (endObject.length === 1) {
				endObject.numericOnly();
			}
			valueObject = this.$el.find('#value_' + row.Id);
			if (valueObject.length === 1) {
				valueObject.autoNumeric(EzBob.percentFormat).blur();
			}
		}
		return false;
	},
	show: function (type) {
		return this.$el.show();
	},
	hide: function () {
		return this.$el.hide();
	}
});
