var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.LoanHistoryDetailsModel = Backbone.Model.extend({
	url: function() {
		return "" + window.gRootPath + "Underwriter/LoanHistory/Details?customerid=" + this.id + "&loanid=" + this.loanid;
	}
});

EzBob.Underwriter.LoanDetailsView = Backbone.Marionette.View.extend({
	initialize: function() {
		this.template = _.template($("#loan-details-template").html());
		return this.bindTo(this.model, "change reset", this.render, this);
	},

	attributes: {
		"class": "underwriter-loan-details"
	},

	render: function() {
		var that = this;

		this.$el.dialog({
			modal: true,
			resizable: true,
			title: "Loan Details - " + this.options.loan.RefNumber,
			position: "center",
			draggable: true,
			width: "1000",
			height: "900",
			close: function() {
				$(this).dialog("destroy");
				return that.trigger("close");
			}
		});

		this.renderContent();
		return this;
	},

	renderContent: function() {
		var modelLoan = this.options.loan;
		var model = this.model.toJSON();
		var details = model.details;

		this.$el.html(this.template({
			loan: modelLoan,
			transactions: details && details.Transactions,
			schedule: details && details.Schedule,
			pacnetTransactions: details && details.PacnetTransactions,
			area: "Underwriter",
			rollovers: details && details.Rollovers,
			charges: details && details.Charges,
			showFailed: this.$el.find('.filter-errors').is(':checked'),
			rolloverCount: model.rolloverCount,
			rolloverAvailableClass: model.rolloverAvailableClass
		}));

		if (modelLoan.Modified) {
			this.$el.find('.offer-status').append("<strong>Loan was manually modified</strong>");
		}
	},

	events: {
		"click .rollover": "rollover",
		"click .make-payment": "makePayment",
		"click #btn-options": "showDialogOptions",
		"change .filter-errors": "renderContent",
		"click .pdf-link": "exportToPdf",
		"click .excel-link": "exportToExcel"
	},

	rollover: function(e) {
		if (!this.checkForActiveLoan())
			return false;

		if (this.model.get("notExperiedRollover") && this.model.get("notExperiedRollover").PaidPaymentAmount > 0) {
			EzBob.ShowMessage("Rollover is partially paid. Cannot be edited.");
			return false;
		}

		var model = {
			schedule: this.model.get("details").Schedule,
			rollover: this.model.get("details").Rollovers,
			configValues: this.model.get("configValues"),
			notExperiedRollover: this.model.get("notExperiedRollover"),
			loanId: this.model.loanid
		};

		this.rolloverView = new EzBob.Underwriter.RolloverView({ model: model });

		EzBob.App.jqmodal.show(this.rolloverView);

		this.rolloverView.on("addRollover", this.addRollover, this);
		this.rolloverView.on("removeRollover", this.removeRollover, this);
		return false;
	},

	removeRollover: function(roloverId) {
		var that = this;
		BlockUi("on");

		$.post(window.gRootPath + "Underwriter/LoanHistory/RemoveRollover", roloverId).success(function(request) {
			if (request.success === false) {
				EzBob.ShowMessage(request.error, "Something went wrong");
				return;
			}

			EzBob.ShowMessage("Rollover succesfully removed");
			that.model.fetch();
		}).done(function() {
			EzBob.App.jqmodal.hideModal(that.rolloverView);
			BlockUi("off");
		});
	},

	addRollover: function(model) {
		var that;
		that = this;
		BlockUi("on");
		return $.post(window.gRootPath + "Underwriter/LoanHistory/AddRollover", model).success(function(request) {
			if (request.success === false) {
				EzBob.ShowMessage(request.error, "Something went wrong");
				return;
			}
			EzBob.ShowMessage("Rollover succesfully " + (SerializeArrayToEasyObject(model).isEditCurrent === "true" ? "edited" : "added"));
			that.model.fetch();
			return that.trigger("RolloverAdded");
		}).done(function() {
			EzBob.App.jqmodal.hideModal(that.rolloverView);
			return BlockUi("off");
		});
	},

	makePayment: function(e) {
		var model = {
			loanId: this.options.loan.Id
		};
		var view = new EzBob.Underwriter.ManualPaymentView({
			model: new Backbone.Model(model)
		});
		EzBob.App.jqmodal.show(view);
		view.on("addPayment", this.addPayment, this);
	},

	addPayment: function(data) {
		var that = this;

		data += "&CustomerId=" + this.model.id;
		data += "&LoanId=" + this.options.loan.Id;

		BlockUi("on");

		$.post(window.gRootPath + "Underwriter/LoanHistory/ManualPayment", data).success(function(response) {
			if (response.error) {
				EzBob.ShowMessage(response.error, "Something went wrong", function() { });
			} else {
				EzBob.ShowMessage("Manual payment succesfully added", "", function() {
					that.model.fetch();
					that.trigger("ManualPaymentAdded");
					return true;
				});
				$('body').removeClass('stop-scroll');
				$('body').scrollTo('.confirmationDialog');
				$('body').addClass('stop-scroll');
			}
		}).done(function() {
			BlockUi("off");
		});
	},

	showDialogOptions: function() {
		this.loanOptionsModel = new EzBob.Underwriter.LoanOptionsModel();
		this.loanOptionsModel.loanId = this.model.loanid;

		var xhr = this.loanOptionsModel.fetch();

		var that = this;

		xhr.done(function() {
			this.optionsView = new EzBob.Underwriter.LoanOptionsView({ model: that.loanOptionsModel });
			this.optionsView.render();
			EzBob.App.jqmodal.show(this.optionsView);
		});
	},

	checkForActiveLoan: function() {
		if (this.options.loan.Status === "PaidOff") {
			EzBob.ShowMessage("Loan is  paid off", "Info");
			return false;
		}
		return true;
	},

	exportToPdf: function(e) {
		var customerId = this.model.id;
		var $el = $(e.currentTarget);
		$el.attr("href", window.gRootPath + "Underwriter/LoanHistory/ExportDetails?id=" + customerId + "&loanid=" + this.options.loan.Id + "&isExcel=false" + "&wError=" + this.$el.find('.filter-errors').is(':checked'));
	},

	exportToExcel: function(e) {
		var customerId = this.model.id;
		var $el = $(e.currentTarget);
		$el.attr("href", window.gRootPath + "Underwriter/LoanHistory/ExportDetails?id=" + customerId + "&loanid=" + this.options.loan.Id + "&isExcel=true" + "&wError=" + this.$el.find('.filter-errors').is(':checked'));
	}
});
