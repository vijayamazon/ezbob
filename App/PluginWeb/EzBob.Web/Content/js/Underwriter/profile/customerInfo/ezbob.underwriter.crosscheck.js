var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.CrossCheckView = Backbone.View.extend({
	initialize: function () {
		this.crossCheckDone = false;
	}, // initialize

	render: function (d) {
		this.model = d;

		var view = this;

		var jx = $.get(window.gRootPath + "Underwriter/CrossCheck/Index/" + this.model.customerId, function (data) {
			view.$el.html(data);

			view.$el.find('.copy-buttons').one('mouseover', function () {
				view.$el.find(".btn-copy").each(function () {
					var element = $(this);

					if ( (/[^\s,]/g).test(element.data('address')) ) {
						element.zclip({
							path: window.gRootPath + "Content/flash/ZeroClipboard.swf",
							copy: function() {
								return element.data('address');
							}
						});
					}
					else
						element.addClass("disabled");
				});
			});
		});
		jx.error(function () { view.$el.html("Failed to get cross check data."); });
	}, // render

	doCrossCheck: function() {
		if (this.crossCheckDone)
			return;

		this.setExperianData();

		this.setHmrcData();

		this.crossCheckOne('#cross-check-company-name');

		this.crossCheckOne('#cross-check-company-address');

		this.crossCheckDone = true;
	}, // doCrossCheck

	crossCheckOne: function(sParentID) {
		var oParent = this.$el.find(sParentID);

		var sAppValue = oParent.find('.application').text();

		var sExperianValue = oParent.find('.experian').text();
		
		if (sAppValue != sExperianValue) {
			oParent.find('.checkoutcome').addClass('NoChecked');
			return;
		} // if

		var oHmrc = oParent.find('.hmrc');

		if (oHmrc.hasClass('hide')) {
			if (sAppValue == sExperianValue)
				oParent.find('.checkoutcome').addClass('Checked');

			return;
		} // if HMRC is hidden

		oParent.find('.checkoutcome').addClass(sAppValue == oHmrc.text() ? 'Checked' : 'NoChecked');
	}, // crossCheckOne

	setHmrcData: function() {
		if (this.$el.find('.cross-check-customer-email').attr('isoffline').toLowerCase() != 'y') {
			this.$el.find('.hmrc').addClass('hide').hide();
			return;
		} // if

		if (!this.options.marketPlaces)
			return;

		var oHMRC = null;

		_.every(this.options.marketPlaces.models, function(mdl) {
			var bContinue = true;

			mdl.collection.each(function(mp) {
				if (mp.get('Name') == 'HMRC') {
					oHMRC = mp;
					bContinue = false;
					return false;
				} // if

				return true;
			}); // for every collection member

			return bContinue;
		}); // for every model list in collection

		if (oHMRC == null) {
			this.$el.find('.hmrc').addClass('hide').hide();
			return;
		} // if

		console.log('HMRC is', oHMRC);

		var oCGData = oHMRC.get('CGData');

		if (!oCGData || !oCGData.VatReturn || !oCGData.VatReturn.length)
			return;

		var oCompanyID = oCGData.VatReturn[oCGData.VatReturn.length - 1];

		this.$el.find('#cross-check-company-name .hmrc').text($.trim(oCompanyID['BusinessName']));

		this.$el.find('#cross-check-company-address .hmrc').text(this.address(oCompanyID['BusinessAddress']));
	}, // setHmrcData

	setExperianData: function() {
		console.log('company score is', this.options.companyScore);

		if (!this.options.companyScore)
			return;

		var oDataset = this.options.companyScore.get('dataset');

		if (!oDataset)
			return;

		if (!oDataset['Limited Company Identification'])
			return;

		var oCompanyIdList = oDataset['Limited Company Identification'].Data;

		if (!oCompanyIdList || !oCompanyIdList[0])
			return;

		var oCompanyID = oCompanyIdList[0];

		this.$el.find('#cross-check-company-name .experian').text($.trim(oCompanyID['Company Name']));

		this.$el.find('#cross-check-company-address .experian').text(this.address(oCompanyID['Office Address']));
	}, // setExperianData

	address: function(oRawAddress) {
		var aryRawAddress = null;

		if (!oRawAddress)
			return '';

		switch (Object.prototype.toString.call(oRawAddress)) {
		case '[object Array]':
			aryRawAddress = oRawAddress;
			break;

		case '[object String]':
			aryRawAddress = oRawAddress.split('\n');
			break;

		default:
			return '';
			break;
		} // switch

		return _.map(
			_.filter(aryRawAddress, function (s) { return $.trim(s) != ''; }),
			function (s) { return $.trim(s); }
		).join('\n').toUpperCase();
	}, // address

	events: {
		"click #recheck-targeting": "recheckTargeting"
	}, // events

	recheckTargeting: function (e) {
		var el = $(e.currentTarget),
			postcode = el.attr("data-postcode"),
			companyName = el.attr("data-company-name"),
			companyLegalStatus = el.attr('data-company-legal-status')[0],
			that = this;

		if (el.hasClass("disabled"))
			return false;

		el.addClass("disabled");
		scrollTop();
		BlockUi("On");

		$.get(window.gRootPath + "Account/CheckingCompany", { companyName: companyName, postcode: postcode, filter: companyLegalStatus })
		.success(function (reqData) {
			if (reqData == undefined || reqData.success === false)
				EzBob.ShowMessage("Targeting service is not responding", "Error", null, "OK");
			else {
				switch (reqData.length) {
					case 0:
						EzBob.ShowMessage("Company was not found by post code.", "Warning", null, "OK");
						$("#recheck-targeting").removeClass("disabled");
						break;

					case 1:
						that.saveRefNum(reqData[0].BusRefNum);
						break;

					default:
						var companyTargets = new EzBob.companyTargets({ model: reqData });

						companyTargets.render();

						companyTargets.on("BusRefNumGetted", function (busRefNum) {
							that.saveRefNum(busRefNum);
						});

						break;
				} // switch
			} // if
		}).complete(function () {
			BlockUi("Off");
		});;

		return false;
	}, // recheckTargeting

	saveRefNum: function (refnum) {
		var that = this;
		$.post(window.gRootPath + "Underwriter/CrossCheck/SaveRefNum", { customerId: this.model.customerId, companyRefNum: refnum })
		.done(function () {
			EzBob.ShowMessage("Company Ref Number was updated", "Updated successfully", null, "OK");
		})
		.complete(function () {
			$("#recheck-targeting").removeClass("disabled");
			that.render(that.model);
		});
	}, // saveRefNum
}); // EzBob.Underwriter.CrossCheckView
