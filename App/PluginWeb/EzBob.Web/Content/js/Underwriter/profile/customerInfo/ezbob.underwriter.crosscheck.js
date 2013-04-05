var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.CrossCheckView = Backbone.View.extend({
    initialize: function () {
    },
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
                    } else {
                        element.addClass("disabled");
                    }
                    
                });
            });
        });
        jx.error(function () { view.$el.html("Failed to get cross check data."); });
    },
    events: {
        "click #recheck-targeting": "recheckTargeting"
    },
    recheckTargeting: function (e) {
        var el = $(e.currentTarget),
            postcode = el.attr("data-postcode"),
            companyName = el.attr("data-company-name"),
            that = this;

        if (el.hasClass("disabled")) {
            return false;
        }
        el.addClass("disabled");
        scrollTop();
        BlockUi("On");

        $.get(window.gRootPath + "Account/CheckingCompany", { companyName: companyName, postcode: postcode }).success(function (reqData) {
            if (reqData == undefined || reqData.success === false) {
                EzBob.ShowMessage("Targeting service is not responding", "Error", null, "OK");
            } else {
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
                }
            }

        }).complete(function () {
            BlockUi("Off");
        });;

        return false;
    },
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
    }
});