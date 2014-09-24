var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};
EzBob.Underwriter.Settings = EzBob.Underwriter.Settings || {};

EzBob.Underwriter.Settings.RangeModel = Backbone.Model.extend({
	url: function() {
		return window.gRootPath + "Underwriter/StrategySettings/SettingsConfigTable?tableName=" + this.get('type');
	}
});

EzBob.Underwriter.Settings.RangeView = Backbone.Marionette.ItemView.extend({
	template: "#range-template",
	initialize: function () {
		this.modelBinder = new Backbone.ModelBinder();
		this.model.on("reset change", this.render, this);
		this.update();
		return this;
	},
	events: {
		"click .addRange": "addRange",
		"click .removeRange": "removeRange",
		"click .save": "save",
		"click .cancel": "update",
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
	save: function () {
		var xhr;
		BlockUi("on");
		xhr = $.post("" + window.gRootPath + "Underwriter/StrategySettings/SaveConfigTable", {
			serializedModels: JSON.stringify(this.model.get('configTableEntries')),
			configTableType: this.model.type
		});
		xhr.done(function (res) {
			if (res.error) {
				EzBob.App.trigger('error', res.error);
			}
		});
		xhr.always(function () {
			BlockUi("off");
		});
		
		return false;
	},
	removeRange: function (eventObject) {
		var index, rangeId, ranges, row, _i, _len;
		rangeId = eventObject.target.getAttribute('range-id');
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
		return false;
	},
	addRange: function (e, range) {
		var freeId, t, verified;
		freeId = -1;
		verified = false;
		while (!verified) {
			t = this.$el.find('#row_' + freeId);
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
		return {
			configTableEntries: this.model.get('configTableEntries'),
			typeName: this.model.get('typeName')
		};
	},
	update: function () {
		this.model.fetch();
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
		this.$el.show();
	},
	hide: function () {
		this.$el.hide();
	}
});
