var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.CollectionStatusModel = Backbone.Model.extend({
	urlRoot: function() {
		return "" + window.gRootPath + "Underwriter/CollectionStatus/Index?Id=" + (this.get('customerId'));
	}
});

EzBob.Underwriter.CollectionStatuses = Backbone.Collection.extend({
	url: function() {
		return "" + window.gRootPath + "Underwriter/CollectionStatus/GetStatuses";
	}
});

EzBob.Underwriter.CollectionStatusLayout = Backbone.Marionette.Layout.extend({
	template: '#collection-status-layout-template',

	initialize: function() {
		this.modelBinder = new Backbone.ModelBinder();
		this.statuses = EzBob.Underwriter.StaticData.CollectionStatuses;
		this.customerId = this.model.get('customerId');
		return this;
	},

	bindings: {
		CurrentStatus: {
			selector: "#collection-status-items"
		},
		customerId: {
			selector: "input[name='customerId']"
		}
	},

	regions: {
		content: '#collection-view'
	},

	events: {
		'change #collection-status-items': 'changeStatus'
	},

	serializeData: function () {
		return {
			statuses: this.statuses.toJSON()
		};
	},
	onRender: function() {
		this.oldStatus = this.model.get('CurrentStatus');
		this.modelBinder.bind(this.model, this.el, this.bindings);
		this.$el.find("#collection-status-items [value='" + (this.oldStatus) + "']").attr("selected", "selected");
		this.renderStatusValue();
		return this;
	},

	changeStatus: function() {
		var currentStatus = $("#collection-status-items option:selected").val();
		this.model.clear({ silent: true });
		this.model.set({ "CurrentStatus": parseInt(currentStatus), "customerId" :this.customerId });
		this.renderStatusValue();
		return this;
	},

	renderStatusValue: function() {
		var collectionStatusView = new EzBob.Underwriter.CollectionStatusView({
			model: this.model
		});

		this.content.show(collectionStatusView);
		
		return false;
	},

	save: function() {
		if (this.oldStatus == this.model.get('CurrentStatus')) {
			EzBob.ShowMessageTimeout("The status didn't changed, nothing to save", "No change", 3, function() {
				return false;
			});
		}

		BlockUi("on");
		var self = this;

		var action = "" + window.gRootPath + "Underwriter/CollectionStatus/Save/";

		var postData = this.model.toJSON();

		$.post(action, postData).done(function() {
			self.trigger('saved');
			self.close();
		}).always(function() {
			BlockUi("off");
		});

		return false;
	},

	jqoptions: function() {
		return {
			modal: true,
			resizable: true,
			title: 'Customer Status',
			draggable: true,
			width: "400",
			buttons: {
			    "OK": _.bind(this.onSave, this),
			    "Cancel": _.bind(this.onCancel, this)
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
		},
		Amount: {
			selector: "#collectionAmount",
			converter: EzBob.BindingConverters.moneyFormat
		},
		ApplyForJudgmentDate: {
			selector: "#collectionApplyForJudgmentDate"
		},
		Type: {
			selector: "#collectionType"
		},
		Feedback: {
			selector: "#collectionFeedback"
		}
	},
	upload: function() {
		$("#addNewDoc").click();
		return false;
	},
	onRender: function() {
		this.modelBinder.bind(this.model, this.el, this.bindings);
		this.$el.parents('.ui-dialog').find("button").addClass('btn-back');
		this.$el.find('#collectionAmount').moneyFormat();
		this.$el.find('.collectionFee').autoNumeric({
			'aSep': ',',
			'aDec': '.',
			'aPad': true,
			'mNum': 16,
			'mRound': 'F',
			mDec: '2',
			vMax: '999999999999999'
		});

		this.$el.find("#collectionApplyForJudgmentDate").mask("99/99/9999").datepicker({
			autoclose: true,
			format: 'dd/mm/yyyy'
		});

		

		var status = this.model.get('CurrentStatus');
		var collectionStatus = EzBob.Underwriter.StaticData.CollectionStatuses.models[status];
		
		if (!collectionStatus) {
			return false;
		}

		var collectionStatusId = collectionStatus.get('Id');
		var collectionStatusIsDefault = collectionStatus.get('IsDefault');

		//Legal – claim process,Legal - apply for judgment,Legal: CCJ,Legal: charging order,WriteOff
		if(_.contains([16,17,18,20,8], collectionStatusId)) {
			this.$el.find('.amount_status').show();
		}

		//Legal: bailiff,Legal: charging order
		if (_.contains([19,20], collectionStatusId)) {
			this.$el.find('.type_status').show();
		}

		//Legal – claim process
		if (_.contains([16], collectionStatusId)) {
			this.$el.find('.apply_for_judgment_status').show();
		}

		//Collection: Tracing,Collection: Site Visit
		if (_.contains([21, 22], collectionStatusId)) {
			this.$el.find('.feedback_status').show();
		}

		if (collectionStatusIsDefault) {
			this.$el.find('.default_status').show();
		}
		
		return this;
	}
});
