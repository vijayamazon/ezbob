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
        var that = this;
        switch (action) {
            case "collapse":
                btn = this.$el.find('a[data-action="collapse"]');
                btn.children("i").addClass("anim-turn180");
                btn.parents(".box").children(".box-content").slideToggle(500, function () {
                    if ($(this).is(":hidden")) {
                        btn.children("i").attr("class", "fa fa-chevron-down");
                        console.log('a', btn.parents(".box").children(".box-title-collapse"));
                        that.$el.find(".box-title-collapse").show();
                    } else {
                        btn.children("i").attr("class", "fa fa-chevron-up");
                        that.$el.find(".box-title-collapse").hide();
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
        var controlButtons = this.$el.find("#controlButtons");
        this.controlButtonsView = new EzBob.Underwriter.ControlButtonsView(
            {
                el: controlButtons,
                model: new Backbone.Model({ customerId: this.model.get("Id") })
            });
        this.controlButtonsView.render();
        
        if (this.personalModel) {
            this.changeDecisionButtonsState(this.personalModel.get("Editable"));
        }

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
                histData.push([parseInt(hist), medalHistory.MedalHistories[hist].Result * 100]);
            }

            var mhPlot = $.jqplot('medalHistory', [histData],
                {
                    title: '',
                    axes: {
                        yaxis: {
                            labelRenderer: $.jqplot.CanvasAxisLabelRenderer,
                            min: 0,
                            max: 100,
                            tickOptions: {
                                show: false,
                                formatString: '%.1f %'
                            },
                            rendererOptions: { drawBaseline: false }
                        },
                        xaxis: {
                            min: 0,
                            tickOptions: { show: false },
                            rendererOptions: { drawBaseline: false }
                        }
                    },
                    grid: {
                        drawGridLines: false,
                        background: 'transparent',
                        borderWidth: 0,
                        borderColor: 'transparent',
                        shadow: false
                    },
                    seriesDefaults: {
                        shadow: false,
                        color: "#a7a7a7",
                        markerOptions: {
                            color: "#a7a7a7",
                            style: "circle"
                        }
                    },
                    highlighter: {
                        show: true,
                        showTooltip: true,
                        tooltipAxes: 'y',
                        useAxesFormatters: true,
                        tooltipFormatString: '%.1f %'
                    },
                    canvasOverlay: {
                        show: true,
                        objects: [
                            { horizontalLine: { name: '', y: 0, lineWidth: 1, xOffset: 0, color: '#a7a7a7', shadow: false } },
                            { horizontalLine: { name: 'silver', y: 40, lineWidth: 1, xOffset: 0, color: '#a7a7a7', shadow: false } },
                            { horizontalLine: { name: 'gold', y: 62, lineWidth: 1, xOffset: 0, color: '#a7a7a7', shadow: false } },
                            { horizontalLine: { name: 'platinum', y: 84, lineWidth: 1, xOffset: 0, color: '#a7a7a7', shadow: false } },
                            { horizontalLine: { name: 'diamond', y: 100, lineWidth: 1, xOffset: 0, color: '#a7a7a7', shadow: false } }
                        ]
                    }
                });


            var medalBar = $.jqplot('medalBar', [[40], [22], [22], [16]],
                {
                    // Tell the plot to stack the bars.
                    stackSeries: true,
                    captureRightClick: true,
                    seriesDefaults: {
                        renderer: $.jqplot.BarRenderer,
                        rendererOptions: {
                            // Put a 30 pixel margin between bars.
                            barMargin: 30,
                        },
                        shadow: false,
                    },
                    seriesColors: ['silver', '#D4AF37', '#E5E4E2', '#39A3E1'],
                    axes: {
                        xaxis: {
                            renderer: $.jqplot.CategoryAxisRenderer,
                            ticks: [''],
                            tickOptions: { show: false },
                            rendererOptions: { drawBaseline: false }
                        },
                        yaxis: {
                            min: 0,
                            max: 100,
                            padMin: 0,
                            tickOptions: { show: false },
                            rendererOptions: { drawBaseline: false }
                        }
                    },
                    grid: {
                        drawGridLines: false,
                        background: 'transparent',
                        borderWidth: 0,
                        borderColor: 'transparent',
                        shadow: false
                    },
                });
        }

        var medal = this.medalModel.get('Score');
        if (medal) {
            var fillColor = 'black';
            switch (medal.Medal) {
                case 'Silver':
                    fillColor = 'silver'; break;
                case 'Gold':
                    fillColor = '#D4AF37'; break;
                case 'Platinum':
                    fillColor = '#E5E4E2'; break;
                case 'Diamond':
                    fillColor = '#39A3E1'; break;
            }
            this.drawDonut('medalCanvas', fillColor, medal.Result);
        }
    },
    
    changeDecisionButtonsState: function(isHideAll) {
        var creditResult, disabled, userStatus;
        disabled = !this.personalModel.get("IsCustomerInEnabledStatus");
        creditResult = this.personalModel.get("CreditResult");
        this.$el.find("#SuspendBtn, #RejectBtn, #ApproveBtn, #EscalateBtn, #ReturnBtn").toggleClass("disabled", disabled);
        if (isHideAll) {
            this.$el.find("#SuspendBtn, #RejectBtn, #ApproveBtn, #EscalateBtn, #ReturnBtn").hide();
        }
        switch (creditResult) {
            case "WaitingForDecision":
                this.$el.find("#ReturnBtn").hide();
                this.$el.find("#RejectBtn").show();
                this.$el.find("#ApproveBtn").show();
                this.$el.find("#SuspendBtn").show();
                //if (!escalatedFlag) {this.$el.find("#EscalateBtn").show();}
                break;
            case "Rejected":
            case "Approved":
            case "Late":
                this.$el.find("#ReturnBtn").hide();
                this.$el.find("#RejectBtn").hide();
                this.$el.find("#ApproveBtn").hide();
                this.$el.find("#SuspendBtn").hide();
                this.$el.find("#EscalateBtn").hide();
                break;
            case "Escalated":
                this.$el.find("#ReturnBtn").hide();
                this.$el.find("#RejectBtn").show();
                this.$el.find("#ApproveBtn").show();
                this.$el.find("#SuspendBtn").show();
                this.$el.find("#EscalateBtn").hide();
                break;
            case "ApprovedPending":
                this.$el.find("#ReturnBtn").show();
                this.$el.find("#RejectBtn").hide();
                this.$el.find("#ApproveBtn").hide();
                this.$el.find("#SuspendBtn").hide();
                this.$el.find("#EscalateBtn").hide();
        }
        userStatus = this.personalModel.get("UserStatus");
        if (userStatus === 'Registered') {
            this.$el.find("#ReturnBtn").hide();
            this.$el.find("#RejectBtn").hide();
            this.$el.find("#ApproveBtn").hide();
            this.$el.find("#SuspendBtn").hide();
            return this.$el.find("#EscalateBtn").hide();
        }
    },
    drawDonut: function (canvasId, fillColor, fillPercent) {
        var canvas = document.getElementById(canvasId);
        var context = canvas.getContext('2d');
        var x = canvas.width / 2;
        var y = canvas.height / 2;
        var radius = 40;
        var startAngle = 1 * Math.PI;
        var endAngle = 4 * Math.PI;
        var lineWidth = 15;
        context.beginPath();
        context.arc(x, y, radius, startAngle, endAngle, false);
        context.lineWidth = lineWidth;
        context.strokeStyle = '#ebebeb';
        context.stroke();
        context.beginPath();
        context.arc(x, y, radius, startAngle, Math.PI * (1 + fillPercent * 2), false);
        context.strokeStyle = fillColor;
        context.stroke();
    }
});
