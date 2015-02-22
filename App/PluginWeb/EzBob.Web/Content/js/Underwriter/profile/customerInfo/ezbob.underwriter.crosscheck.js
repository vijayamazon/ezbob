var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.CrossCheckView = Backbone.View.extend({
    initialize: function () {
        //this.segmentType = '';
        this.marketPlaces = null;
        this.companyScore = null;
        this.experianDirectors = null;
        this.fullModel = null;
    }, // initialize

    render: function (d) {
        this.model = d;

        var view = this;

        var jx = $.get(window.gRootPath + "Underwriter/CrossCheck/Index/" + this.model.customerId, function (data) {
            view.$el.html(data);

            view.$el.find('.copy-buttons').one('mouseover', function () {
                view.$el.find(".btn-copy").each(function () {
                    var element = $(this);

                    if ((/[^\s,]/g).test(element.data('address'))) {
                        element.zclip({
                            path: window.gRootPath + "Content/flash/ZeroClipboard.swf",
                            copy: function () {
                                return element.data('address');
                            }
                        });
                    }
                    else
                        element.addClass("disabled");
                });
            });

            view.doCrossCheck();
        });

        jx.error(function () { view.$el.html("Failed to get cross check data."); });
    }, // render

    doCrossCheck: function () {
        this.setExperianData();

        this.setHmrcData();

        var cb = null;
        var sBusinessType = (
			(this.fullModel ?
				((cb = this.fullModel.get('CreditBureauModel')) ? cb.BorrowerType : '') :
				'') || ''
		).toLowerCase();

        if (sBusinessType && (sBusinessType != 'entrepreneur')) {
            var bCompanyNameSuccess = this.crossCheckOne('#cross-check-company-name');

            var bCompanyAddressSuccess = this.crossCheckOne('#cross-check-company-address');

            var bDirectorsSuccess = this.crossCheckDirectors();

            this.$el.find('.cross-check-summary-company-name').addClass(bCompanyNameSuccess ? 'Checked' : 'NoChecked');
            this.$el.find('.cross-check-summary-company-address').addClass(bCompanyAddressSuccess ? 'Checked' : 'NoChecked');
            this.$el.find('.cross-check-summary-directors').addClass(bDirectorsSuccess ? 'Checked' : 'NoChecked');
        } // if
    }, // doCrossCheck

    crossCheckDirectors: function () {
        var oUiDirector = this.$el.find('#cross-check-experian-only-directors');

        oUiDirector.show();

        if (!this.experianDirectors) {
            oUiDirector.hide();
            return true;
        } // if

        var self = this;

        var oExperian = {};
        var nExperianCount = 0;

        _.each(this.experianDirectors, function (sDirectorName) {
            var sName = self.name(sDirectorName.toUpperCase());

            if (!sName)
                return;

            oExperian[sName] = 1;
            nExperianCount++;
        }); // for each experian director

        if (0 == nExperianCount) {
            oUiDirector.hide();
            return true;
        } // if

        var oApplication = {};

        this.$el.find('.cross-check-director-details').each(function () {
            var sName = self.name($(this).attr('director-name'));

            if (!sName)
                return;

            oApplication[sName] = 1;
        }); // for each app director

        var nOnlyCount = 0;

        var oUiDirectorList = oUiDirector.find('ul');

        for (var sDirName in oExperian) {
            if (oApplication[sDirName])
                continue;

            oUiDirectorList.append($('<li></li>').text(sDirName));
            nOnlyCount++;

            oUiDirector.show();
        } // for

        if (0 == nOnlyCount) {
            oUiDirector.hide();
            return true;
        } // if

        return false;
    }, // crossCheckDirectors

    crossCheckOne: function (sParentID) {
        var oParent = this.$el.find(sParentID);

        var sAppValue = oParent.find('.application').text();

        var sExperianValue = oParent.find('.experian').text();

        if (sAppValue != sExperianValue) {
            oParent.find('.checkoutcome').addClass('NoChecked');
            return false;
        } // if

        var oHmrc = oParent.find('.hmrc');

        if (oHmrc.hasClass('hide')) {
            if (sAppValue == sExperianValue) {
                oParent.find('.checkoutcome').addClass('Checked');
                return true;
            } // if

            return false;
        } // if HMRC is hidden

        var bSuccess = sAppValue == oHmrc.text();

        oParent.find('.checkoutcome').addClass(bSuccess ? 'Checked' : 'NoChecked');

        return bSuccess;
    }, // crossCheckOne

    setHmrcData: function () {
        //if (this.segmentType != 'Offline') {
        //    this.$el.find('.hmrc').addClass('hide').hide();
        //    return;
        //} // if

        this.$el.find('.hmrc').removeClass('hide').show();

        if (!this.marketPlaces)
            return;

        var oHMRC = null;

        _.every(this.marketPlaces.models, function (mdl) {
            var bContinue = true;

            mdl.collection.each(function (mp) {
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

        var oHmrcData = oHMRC.get('HmrcData');

        if (!oHmrcData || !oHmrcData.VatReturn || !oHmrcData.VatReturn.length)
            return;

        var oCompanyID = oHmrcData.VatReturn[oHmrcData.VatReturn.length - 1];

        this.$el.find('#cross-check-company-name .hmrc').text($.trim(oCompanyID['BusinessName']));

        this.$el.find('#cross-check-company-address .hmrc').text(this.address(oCompanyID['BusinessAddress']));
    }, // setHmrcData

    setExperianData: function () {
        var oCompanyIdList = this.loadExperian('Limited Company Identification');

        if (!oCompanyIdList)
            return;

        var oCompanyID = oCompanyIdList[0];

        this.$el.find('#cross-check-company-name .experian').text($.trim(oCompanyID.Values['Company Name']));

        this.$el.find('#cross-check-company-address .experian').text(this.address(oCompanyID.Values['Office Address']));
    }, // setExperianData

    loadExperian: function (sDatumID) {
        if (!this.companyScore)
            return null;

        var oDataset = this.companyScore.get('dataset');

        if (!oDataset)
            return null;

        if (!oDataset[sDatumID])
            return null;

        var oDatum = oDataset[sDatumID].Data;

        if (!oDatum || !oDatum.length)
            return null;

        return oDatum;
    }, // loadExperian

    address: function (oRawAddress) {
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
            _.filter(aryRawAddress, function(s) { return $.trim(s) != ''; }),
            function(s) { return $.trim(s); }
        ).join('\n').toUpperCase();
    }, // address

    name: function (sRawName) {
        if (!sRawName)
            return '';

        return _.map(
            _.filter(
                $.trim(sRawName).split(/ |\t|\n|\r/), function(s) { return $.trim(s) != ''; }
            ),
            function(s) { return $.trim(s).toUpperCase(); }
        ).join(' ');
    }, // name

    events: {
        "click #recheck-targeting": "recheckTargeting"
    }, // events
    recheckTargeting: function (e) {
        var el = $(e.currentTarget),
            postcode = el.attr("data-postcode"),
            companyName = el.attr("data-company-name"),
            companyLegalStatus = el.attr('data-company-legal-status'),
            companyNum = el.attr('data-company-number'),
            that = this;

        if (el.hasClass("disabled"))
            return false;

        el.addClass("disabled");
        scrollTop();
        BlockUi("On");

    	$.get(window.gRootPath + "Account/CheckingCompany", { companyName: companyName, postcode: postcode, filter: companyLegalStatus, refNum: companyNum, customerId: this.model.customerId })
            .success(function(reqData) {
                if (reqData == undefined || reqData.success === false)
                    EzBob.ShowMessage("Targeting service is not responding", "Error", null, "OK");
                else {
                    switch (reqData.length) {
                    case 0:
                        EzBob.ShowMessage("Company was not found by name, post code, type, and company number (limited).", "Warning", null, "OK");
                        $("#recheck-targeting").removeClass("disabled");
                        break;
                    case 1:
                        that.saveTargetingData(reqData[0]);
                        break;
                    default:
                        var companyTargets = new EzBob.companyTargets({ model: reqData });

                        companyTargets.render();

                        companyTargets.on("BusRefNumGetted", function(targetingData) {
                            that.saveTargetingData(targetingData);
                        });

                        break;
                    } // switch
                } // if
            }).complete(function() {
                BlockUi("Off");
            });

        return false;
    }, // recheckTargeting

    saveTargetingData: function (targetingData) {
        var that = this;
        if (!targetingData || targetingData.BusRefNum == 'skip') {
            targetingData.BusRefNum = 'NotFound';
            targetingData.BusName = 'Company not found';
        }
        $.post(window.gRootPath + "Underwriter/CrossCheck/SaveTargetingData",
            {
                customerId: this.model.customerId,
                companyRefNum: targetingData.BusRefNum,
                companyName: targetingData.BusName,
                addr1: targetingData.AddrLine1,
                addr2: targetingData.AddrLine2,
                addr3: targetingData.AddrLine3,
                addr4: targetingData.AddrLine4,
                postcode: targetingData.PostCode,
            })
            .done(function() {
                EzBob.ShowMessage("Company Info was updated", "Updated successfully", null, "OK");
            })
            .complete(function() {
                $("#recheck-targeting").removeClass("disabled");
                that.render(that.model);
            });
    }, // saveTargetingData
}); // EzBob.Underwriter.CrossCheckView
