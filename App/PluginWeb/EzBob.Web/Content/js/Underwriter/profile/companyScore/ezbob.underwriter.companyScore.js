var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.CompanyScoreModel = Backbone.Model.extend({
	url: function () {
		return window.gRootPath + "Underwriter/CompanyScore/Index/" + this.customerId;
	}
});

EzBob.Underwriter.CompanyScoreView = Backbone.View.extend({
	initialize: function () {
		this.template = _.template($('#company-score-template').html());
		this.model.on('change sync', this.render, this);
		this.list = null;
		this.activePanel = 0;
	}, // initialize

	events: {
		"click .change-company": "changeCompany",
		"click #RunCompanyCheckBtn": "runCompanyCheckBtnClick",
	},

	render: function () {
		var onAfterRender = [];

		var sHtml = this.template({
			companyScoreData: this.model.toJSON(),
			isOwner: false,
			onAfterRender: onAfterRender,
			caption: 'Company'
		});

		this.activePanel = 0;

		var oOwners = this.model.get('Owners');

		var i;

		if (oOwners) {
			for (i = 0; i < oOwners.length; i++) {
				sHtml += this.template({
					companyScoreData: oOwners[i],
					isOwner: true,
					onAfterRender: onAfterRender,
					caption: 'Company Owner'
				});

				this.activePanel++;
			} // for each owner
		} // if has owners

		this.$el.html(sHtml);

		for (i = 0; i < onAfterRender.length; i++)
			onAfterRender[i].call(undefined);

		this.redisplayAccordion();

		this.companiesHouseView = new EzBob.Underwriter.CompaniesHouseView({
			el: this.$el.find('#companies-house-data'),
			model: this.model
		});
		this.companiesHouseView.render();

	}, // render

	redisplayAccordion: function () {
		if (this.list) {
			this.list.accordion('destroy');
			this.list = null;
		} // if

		this.list = this.$el.find('.company-score-data').accordion({
			heightStyle: 'content',
			collapsible: true,
			active: this.activePanel
		});

		this.list.addClass('box');
		this.list.find('.ui-state-default').addClass('box-title');
		this.list.find('.box-content').css('height', ''); // Ugly temporary patch.
	}, // redisplayAccordion

	changeCompany: function () {
		this.companyModel = new EzBob.CompanyModel(this.model.get('CompanyDetails'));
		this.changeCompany = new EzBob.Underwriter.ChangeCompanyView({ model: this.companyModel });
		EzBob.App.jqmodal.show(this.changeCompany);
		this.changeCompany.on('companyChanged', this.reload, this);
	},

	runCompanyCheckBtnClick: function (e) {
		if ($(e.currentTarget).hasClass("disabled")) return false;

		var that = this;
		BlockUi("on");
		$.post(window.gRootPath + "Underwriter/CreditBureau/IsCompanyCacheRelevant", { customerId: this.model.customerId })
			.done(function(response) {
				if (response.NoCompany) {
					EzBob.ShowMessage("Customer don't have a company", "Nothing to recheck");
				} else {
					if (response.IsRelevant == "True") {
						EzBob.ShowMessage("Last check was done at " + response.LastCheckDate + " and cache is valid for " + response.CacheValidForDays + " days. Run check anyway?", "No need for check warning",
							function() {
								that.RunCompanyCheck(true);
								return true;
							},
							"Yes", null, "No");
					} else {
						that.RunCompanyCheck(false);
					}
				}
			})
			.complete(function() {
				BlockUi("off");
			});

		return false;
	},
	RunCompanyCheck: function (forceCheck) {
		BlockUi("on");

		$.post(window.gRootPath + "Underwriter/CreditBureau/RunCompanyCheck", { id: this.model.customerId, forceCheck: forceCheck })
			.done(function(response) {
				EzBob.ShowMessage(response.Message, "Information");
			})
			.fail(function(data) {
				console.error(data.responseText);
			})
			.complete(function() {
				BlockUi("off");
			});
		return false;
	},

	reload: function () {
		Backbone.history.loadUrl();
	}
});

EzBob.Underwriter.CompaniesHouseView = Backbone.Marionette.ItemView.extend({
	template: '#companies-house-template',
	initialize: function () {
		return this;
	},
	onRender: function () {
		return this;
	},

	serializeData: function () {
		return {
			data: this.model.get('CompaniesHouseModel')
		};
	}
});

EzBob.Underwriter.ChangeCompanyView = Backbone.Marionette.ItemView.extend({
	template: "#change-company-template",
	initialize: function () {
		this.modelBinder = new Backbone.ModelBinder();
		this.model.on('change sync', this.render, this);

	},

	onRender: function () {
		var oAddressContainer = this.$el.find('#CompanyAddress');

		this.addressView = new EzBob.AddressView({
			model: this.model.get('CompanyAddress'),
			name: 'CompanyAddress',
			max: 1,
			required: "empty"
		});

		this.addressView.render().$el.appendTo(oAddressContainer);
		this.ui.addressCaption.hide();
		this.modelBinder.bind(this.model, this.el, this.bindings);
		this.ui.typeOfBusiness.focus();
    },

	bindings: {
		CompanyName: "#CompanyName",
		CompanyRefNum: "#CompanyNumber",
		TypeOfBusiness: "#TypeOfBusiness"
	},

	events: {
		"click .btn-change-company": "targetCompany"
	},

	ui: {
		addressCaption: ".addressCaption",
		companyName: "#CompanyName",
		companyNum: "#CompanyNumber",
		typeOfBusiness: "#TypeOfBusiness"
	},

	targetCompany: function () {

		var that = this;
		var filter = "N";
		switch (this.ui.typeOfBusiness.val()) {
			case "Limited":
			case "LLP":
				filter = "L";
		}
		var postcode = "";
		if (this.model.get("CompanyAddress").models && this.model.get("CompanyAddress").models.length > 0) {
			postcode = this.model.get("CompanyAddress").models[0].get('Postcode');
		} else {
			EzBob.ShowMessage("Please input company address", "Error", null, "OK");
			return false;
		}

		BlockUi("On");

	    $.get(window.gRootPath + "Account/CheckingCompany", { companyName: this.ui.companyName.val(), postcode: postcode, filter: filter, refNum: this.ui.companyNum.val(), customerId: this.model.get("CustomerId") })
			.success(function (reqData) {
				if (reqData == undefined || reqData.success === false)
					EzBob.ShowMessage("Targeting service is not responding", "Error", null, "OK");
				else {
					switch (reqData.length) {
						case 0:
							EzBob.ShowMessage("Company was not found", "Warning", null, "OK");
							that.saveTargetingData("NotFound");
							break;
						case 1:
							that.saveTargetingData(reqData[0]);
							break;
						default:
							var companyTargets = new EzBob.companyTargets({ model: reqData });

							companyTargets.render();

							companyTargets.on("BusRefNumGot", function (targetingData) {
								that.saveTargetingData(targetingData);
							});

							break;
					} // switch
				} // if
			}).always(function () {
				BlockUi("Off");
			});
		return false;
	},
	saveTargetingData: function (targetingData) {
		var that = this;
		this.model.save().done(function (res) {
			if (!res.success) {
				EzBob.ShowMessage("Failed to change company: " + res.error, "Error");
			} else {
				if (!targetingData || targetingData.BusRefNum == 'skip') {
					targetingData.BusRefNum = 'NotFound';
					targetingData.BusName = 'Company not found';
				}
				$.post(window.gRootPath + "Underwriter/CrossCheck/SaveTargetingData",
					{
						customerId: that.model.get('CustomerId'),
						companyRefNum: targetingData.BusRefNum,
						companyName: targetingData.BusName,
						addr1: targetingData.AddrLine1,
						addr2: targetingData.AddrLine2,
						addr3: targetingData.AddrLine3,
						addr4: targetingData.AddrLine4,
						postcode: targetingData.PostCode,
					})
					.done(function () {
						EzBob.ShowMessage('Company changed successfully!<br>Run new credit line to recalculate medal.', 'Success', function () {
							that.trigger('companyChanged');
							that.close();
						}, "OK");
					});
			}
		}).always(function () {
			BlockUi("Off");
		});

		return false;
	},
	jqoptions: function () {
		return {
			modal: true,
			resizable: true,
			title: "Change customer's company",
			position: "center",
			draggable: true,
			dialogClass: "changeCompany",
			width: 660
		};
	}
});