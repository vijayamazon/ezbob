var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.RolloverView = Backbone.View.extend({
	initialize: function() {
		this.template = _.template($('#payment-rollover-template').html());
		this.model.hasActive = false;

		var ro = this.model.rollover;
		var nLen = ro.length;

		for (var i = 0; i < nLen; i++) {
			var val = ro[i];

			if (val.Status === 0) {
				this.model.hasActive = true;
				this.model.rollover = val;
				return null;
			}
		}

		return this;
	},

	jqoptions: function() {
		return {
			modal: true,
			resizable: false,
			title: !this.model.hasActive ? "Add roll over" : "Edit roll over",
			position: "center",
			draggable: false,
			dialogClass: "rollover-popup",
			width: 600
		};
	},

	events: {
		"click .confirm": "addRollover",
		"click .remove": "removeRollover",
		"change [name='ExperiedDate']": "updatePaymentData"
	},

	render: function() {
		this.$el.html(this.template({
			model: this.model
		}));

		this.$el.find('.ezDateTime').splittedDateTime();

		this.form = this.$el.find("#rollover-dialog");

		this.validator = EzBob.validateRollover(this.form);

		if (this.model.hasActive)
			SetDefaultDate(this.$el.find('#ExperiedDate'), this.model.rollover.ExpiryDate);

		this.$el.find('select[name=\"ScheduleId\"]').change();

		this.updatePaymentData();

		return this;
	},

	addRollover: function() {
		if (!this.validator.form())
			return false;

		var disabled = this.form.find(':input:disabled').removeAttr('disabled');
		this.trigger("addRollover", this.form.serializeArray());
		disabled.attr('disabled', 'disabled');
		return true;
	},

	removeRollover: function() {
		var rolloverId = this.$el.find('input[name=\"rolloverId\"]');
		this.trigger("removeRollover", rolloverId);
	},

	updatePaymentData: function() {
		var data = {
			loanId: this.model.loanId,
			isEdit: this.model.hasActive
		};

		var request = $.get(window.gRootPath + "Underwriter/LoanHistory/GetRolloverInfo", data);

		var self = this;
		
		request.done(function(r) {
			if (r.error)
				return;

			self.$el.find("#Payment").val(r.rolloverAmount);
			self.$el.find("#interest").val(r.interest);
			self.$el.find("#lateFees").val(r.lateCharge);
			self.$el.find("#MounthCount").val(r.mounthAmount);
		});
	}
});
