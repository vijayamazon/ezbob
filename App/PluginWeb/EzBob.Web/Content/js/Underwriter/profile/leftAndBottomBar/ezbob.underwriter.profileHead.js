var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ProfileHeadView = Backbone.Marionette.ItemView.extend({
    template: "#profile-head-template",
    initialize: function (options) {
        this.loanModel = options.loanModel;
        this.personalModel = options.personalModel;
        this.medalModel = options.medalModel;
        this.bindTo(this.model, "change sync", this.render, this);
        this.bindTo(this.loanModel, "change sync", this.render, this);
        this.bindTo(this.medalModel, "change sync", this.render, this);
        this.bindTo(this.personalModel, "change sync", this.personalModelChanged, this);
    },
    serializeData: function () {
        return {
            m: this.model.toJSON(),
            loan: this.loanModel.toJSON(),
            medal: this.medalModel.get('Score'),
        };
    },
    events: {
        'click a[data-action="collapse"]': "boxToolClick",
        'click a[data-action="close"]': "boxToolClick"
    },
    boxToolClick: function (e) {
        var action, btn, obj;
        obj = e.currentTarget;
        if ($(obj).data("action") === undefined) {
            false;
        }
        action = $(obj).data("action");
        btn = $(obj);
        switch (action) {
            case "collapse":
                $(btn).children("i").addClass("anim-turn180");
                $(obj).parents(".box").children(".box-content").slideToggle(500, function () {
                    if ($(this).is(":hidden")) {
                        $(btn).children("i").attr("class", "fa fa-chevron-down");
                    } else {
                        $(btn).children("i").attr("class", "fa fa-chevron-up");
                    }
                    return false;
                });
                break;
            case "close":
                $(obj).parents(".box").fadeOut(500, function () {
                    $(this).parent().remove();
                    return false;
                });
                break;
            case "config":
                $("#" + $(obj).data("modal")).modal("show");
        }
        return false;
    },
    personalModelChanged: function (e, a) {
        if (e && a && this.model) {
            this.model.fetch();
        }
    },
    onRender: function () {
        if (this.model.get('Alerts') !== void 0) {
            if (this.model.get('Alerts').length === 0) {
                $('#customer-label-span').removeClass('label-warning').removeClass('label-important').addClass('label-success');
            } else {
                if (_.some(this.model.get('Alerts'), function (alert) {
                  return alert.AlertType === 'danger';
                })) {
                    $('#customer-label-span').removeClass('label-success').removeClass('label-warning').addClass('label-important');
                } else {
                    $('#customer-label-span').removeClass('label-success').removeClass('label-important').addClass('label-warning');
                }
            }
        }
        this.$el.find('[data-toggle="tooltip"]').tooltip({
            'placement': 'bottom'
        });

        var medalHistory = this.medalModel.get('History');
        if (medalHistory) {
            var histData = [];
            for (var hist in medalHistory.MedalHistories) {
                histData.push([parseInt(hist), medalHistory.MedalHistories[hist].Result]);
            }
            var mhPlot = $.jqplot('medalHistory', [histData],
                {
                    title: '',
                    series: [{ markerOptions: { style: "circle" } }],

                    axes: {
                        yaxis: { min: 0, max: 1 },
                        xaxis: { min: 0, tickOptions: { show: false} }
                    },
                    grid: {
                        drawGridLines: false,
                        background: 'transparent',
                        borderWidth: 0,
                        borderColor: 'transparent',
                        shadow: false
                    },
                    seriesDefaults: {
                        shadow: false
                    }
                });
        }
    }
});
