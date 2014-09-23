var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.CollectionStatusModel = Backbone.Model.extend({
	urlRoot: function() {
		return "" + window.gRootPath + "Underwriter/CollectionStatus/Index?Id=" + (this.get('customerId')) + "&currentStatus=" + (this.get('currentStatus'));
	}
});

EzBob.Underwriter.CollectionStatuses = Backbone.Collection.extend({
	url: function() {
		return "" + window.gRootPath + "Underwriter/CollectionStatus/GetStatuses";
	}
});

EzBob.CollectionStatusItemsView = Backbone.Marionette.ItemView.extend({
	template: '#collection-status-items-template',

	initialize: function() {
		return this.statuses = EzBob.Underwriter.StaticData.CollectionStatuses;
	},

	serializeData: function() {
		return {
			statuses: this.statuses.toJSON()
		};
	}
});

EzBob.Underwriter.CollectionStatusLayout = Backbone.Marionette.Layout.extend({
	template: '#collection-status-layout-template',

	initialize: function() {
		this.modelBinder = new Backbone.ModelBinder();
		this.statuses = EzBob.Underwriter.StaticData.CollectionStatuses;
	},

	bindings: {
		currentStatus: {
			selector: "input[name='currentStatus']"
		},
		customerId: {
			selector: "input[name='customerId']"
		}
	},

	regions: {
		list: '#list-items',
		content: '#collection-view'
	},

	events: {
		'change #collection-status-items': 'changeStatus'
	},

	changeStatus: function() {
		var currentStatus = $("#collection-status-items option:selected").val();

		this.model.set({ "currentStatus": parseInt(currentStatus) });
		this.renderStatusValue();
		return this;
	},

	renderStatusValue: function() {
		var currentStatus = this.model.get("currentStatus");

		if (this.statuses !== void 0 && this.statuses.models[currentStatus] !== void 0 && this.statuses.models[currentStatus].get('Name') === 'Default') {
			this.model.fetch();
			this.$el.find('#collection-view').show();

			var collectionStatusView = new EzBob.Underwriter.CollectionStatusView({
				model: this.model
			});

			this.content.show(collectionStatusView);
		} else
			this.$el.find('#collection-view').hide();

		return false;
	},

	save: function() {
		BlockUi("on");

		var form = this.$el.find('form');
		var postData = form.serialize();
		var action = "" + window.gRootPath + "Underwriter/CollectionStatus/Save/";

		var self = this;

		$.post(action, postData).done(function() {
			self.trigger('saved');
			self.close();
		}).complete(function() {
			BlockUi("off");
		});

		return false;
	},

	onRender: function() {
		this.modelBinder.bind(this.model, this.el, this.bindings);
		var common = new EzBob.CollectionStatusItemsView;
		this.list.show(common);
		this.$el.find("#collection-status-items [value='" + (this.model.get('CurrentStatus')) + "']").attr("selected", "selected");
		this.renderStatusValue();
		return this;
	},

	jqoptions: function() {
		return {
			modal: true,
			resizable: false,
			title: 'Customer Status',
			draggable: true,
			width: "400",
			buttons: {
				"OK": this.onSave,
				"Cancel": this.onCancel
			}
		};
	},

	onCancel: function() {
		this.close();
	},

	onSave: function() {
		this.save();
	}
});

EzBob.Underwriter.CollectionStatusView = Backbone.Marionette.Layout.extend({
	template: '#collection-status-template',
	initialize: function() {
		return this.modelBinder = new Backbone.ModelBinder();
	},
	events: {
		"click .uploadFiles": "upload"
	},
	bindings: {
		CollectionDescription: {
			selector: "#collectionDescription"
		}
	},
	upload: function() {
		$("#addNewDoc").click();
		return false;
	},
	onRender: function() {
		this.modelBinder.bind(this.model, this.el, this.bindings);
		this.$el.parents('.ui-dialog').find("button").addClass('btn-back');
		this.$el.find('.collectionFee').autoNumeric({
			'aSep': ',',
			'aDec': '.',
			'aPad': true,
			'mNum': 16,
			'mRound': 'F',
			mDec: '2',
			vMax: '999999999999999'
		});
		return this;
	}
});
