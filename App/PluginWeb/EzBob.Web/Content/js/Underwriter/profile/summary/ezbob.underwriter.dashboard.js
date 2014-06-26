﻿var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.DashboardView = Backbone.Marionette.ItemView.extend({
    template: "#dashboard-template",
    initialize: function (options) {
        this.crmModel = options.crmModel;
        this.personalModel = options.personalModel;
        this.experianModel = options.experianModel;
        this.propertiesModel = options.propertiesModel;
        this.mpsModel = options.mpsModel;
        this.loanModel = options.loanModel;
        this.companyModel = options.companyModel;
        this.affordability = options.affordability;
        this.bindTo(this.model, "change sync", this.render, this);
        this.bindTo(this.crmModel, "change sync", this.render, this);
        this.bindTo(this.personalModel, "change sync", this.personalModelChanged, this);
        this.bindTo(this.experianModel, "change sync", this.render, this);
        this.bindTo(this.companyModel, "change sync", this.render, this);
        this.bindTo(this.propertiesModel, "change sync", this.render, this);
        this.bindTo(this.mpsModel, "change sync", this.render, this);
        this.bindTo(this.loanModel, "change sync", this.render, this);
        this.bindTo(this.affordability, "change sync", this.render, this);
        
        this.expCompany = [];
        this.journal = [];
        this.journalTable = null;
        this.isCrm = false;
        this.isDecisionHistory = false;
        return this;
    },
    serializeData: function () {
        this.expCompany = [];
        if (this.companyModel.get('DashboardModel')) {
            this.expCompany.push(this.companyModel.get('DashboardModel'));
        }
        if (this.companyModel.get('Owners')) {
            _.each(this.companyModel.get('Owners'), (function (_this) {
                return function (owner) {
                    return _this.expCompany.push(owner.DashboardModel);
                };
            })(this));
        }

        console.log('aff', this.affordability);
        return {
            m: this.model.toJSON(),
            experian: this.experianModel.toJSON(),
            company: this.expCompany,
            properties: this.propertiesModel.toJSON(),
            //mps: this.mpsModel.toJSON(),
            loan: this.loanModel.toJSON(),
            affordability: this.affordability.toJSON()
        };
    },
    rotateTable: function() {
        this.$el.find("#affordabilityTable").each(function () {
            var $this = $(this);
            var newrows = [];
            $this.find("tr").each(function () {
                var i = 0;
                $(this).find("td").each(function () {
                    i++;
                    if (newrows[i] === undefined) { newrows[i] = $("<tr></tr>"); }
                    newrows[i].append($(this));
                });
            });
            $this.find("tr").remove();
            $.each(newrows, function () {
                $this.append(this);
            });
        });
        
        this.$el.find("#affordabilityTable tr:first-child td").each(function () {
            $(this).replaceWith('<th>' + $(this).text() + '</th>');
        });

        $($("#affordabilityTable tr")[2]).addClass("green-row");
    },
    buildJournal: function () {
        if (!this.isCrm && this.crmModel && this.crmModel.get("CustomerRelations")) {
            this.isCrm = true;
            
            _.each(this.crmModel.get("CustomerRelations"), (function (_this) {
                return function (crm) {
                    return _this.journal.push({
                        Action: crm.Action,
                        Adate: new Date(moment.utc(crm.DateTime)),
                        Type: crm.Type,
                        Status: crm.Status,
                        ApprovedSum: null,
                        Interest: null,
                        RepaymentPeriod: null,
                        LoanType: null,
                        SetupFee: null,
                        DiscountPlan: null,
                        LoanSource: null,
                        CustomerSelection: null,
                        UW: crm.User,
                        Comment: crm.Comment,
                        IsCrm: true
                    });
                };
            })(this));

        }
        if (!this.isDecisionHistory && this.model && this.model.get('DecisionHistory')) {
            this.isDecisionHistory = true;
            _.each(this.model.get('DecisionHistory'), (function (_this) {
                return function (dh) {
                    return _this.journal.push({
                        Action: dh.Action,
                        Adate: new Date(moment.utc(dh.Date)),
                        Type: null,
                        Status: null,
                        ApprovedSum: dh.ApprovedSum,
                        Interest: dh.InterestRate,
                        RepaymentPeriod: dh.RepaymentPeriod,
                        LoanType: dh.LoanType.split(" ")[0],
                        SetupFee: (dh.UseSetupFee || dh.UseBrokerSetupFee ? "yes (todo %)" : "-"),
                        DiscountPlan: dh.DiscountPlan,
                        LoanSource: (dh.LoanSourceName === "EU" ? "EU" : ""),
                        CustomerSelection: (dh.IsLoanTypeSelectionAllowed === 1 ? "Yes" : "No"),
                        UW: dh.UnderwriterName,
                        Comment: dh.Comment,
                        IsCrm: false
                    });
                };
            })(this));
        }

        
        if (this.journal.length > 0 && this.isCrm && this.isDecisionHistory) {
           // console.log('table');
            this.journalTable = this.$el.find("#journalTable").dataTable({
                aLengthMenu: [[-1, 10, 20, 50, 100], ['all', 10, 20, 50, 100]],
                iDisplayLength: 20,
                sPaginationType: 'bootstrap',
                bJQueryUI: false,
                aaSorting: [[1, 'desc']],
                bAutoWidth: true,
                aaData: this.journal,
                aoColumns: EzBob.DataTables.Helper.extractColumns('Action,^Adate,Type,Status,$ApprovedSum,%Interest,RepaymentPeriod,LoanType,SetupFee,DiscountPlan,LoanSource,CustomerSelection,UW,Comment,~IsCrm'),
                aoColumnDefs: [
                    {
                        "aTargets": [1],
                        "sType": 'date'
                    }
                ]
            });
            EzBob.DataTables.Helper.initCustomFiltering();
        }
    },
    events: {
        'click a[href^="#companyExperian"]': "companyChanged",
        'change label.journal-filter input': 'journalFilter'
    },
    journalFilter: function (e) {
        var allJournal = this.$el.find("#allJournal");
        if ($(e.currentTarget).attr("id") != "allJournal" && allJournal.is(":checked")) {
            allJournal.parent().click();
            return false;
        }
        this.journalTable.fnDraw();
    },
    companyChanged: function (e) {
        var obj;
        obj = e.currentTarget;
        this.$el.find('.company-name').text($(obj).data('companyname') + ' ' + $(obj).data('companyref'));
    },
    personalModelChanged: function (e, a) {
        if (e && a && this.model) {
            this.model.fetch();
        }
    },
    onRender: function () {
        var cc, cii, companyHistoryScores, consumerHistoryCIIs, consumerHistoryScores, directors, historyScoresSorted, i, properties;
        
        this.$el.find('a[data-bug-type]').tooltip({
            title: 'Report bug'
        });
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
        if (this.experianModel && this.experianModel.get('ConsumerHistory')) {
            historyScoresSorted = _.sortBy(this.experianModel.get('ConsumerHistory'), function (history) {
                return history.Date;
            });
            consumerHistoryScores = _.pluck(historyScoresSorted, 'Score').join(',');
            this.$el.find(".consumerScoreGraph").attr('values', consumerHistoryScores);
            consumerHistoryCIIs = _.pluck(historyScoresSorted, 'CII').join(',');
            this.$el.find(".consumerCIIGraph").attr('values', consumerHistoryCIIs);
        }
        if (this.experianModel && this.experianModel.get('CompanyHistory')) {
            historyScoresSorted = _.sortBy(this.experianModel.get('CompanyHistory'), function (history) {
                return history.Date;
            });
            companyHistoryScores = _.pluck(historyScoresSorted, 'Score').join(',');
            this.$el.find(".companyScoreGraph0").attr('values', companyHistoryScores);
        }
        this.$el.find(".inline-sparkline").sparkline("html", {
            width: "100%",
            height: "100%",
            lineWidth: 2,
            spotRadius: 3.5,
            lineColor: "#cfcfcf",
            fillColor: "transparent",
            spotColor: "#cfcfcf",
            maxSpotColor: "#cfcfcf",
            minSpotColor: "#cfcfcf",
            valueSpots: {
                ':': '#cfcfcf'
            }
        });
        properties = this.propertiesModel.toJSON();
        if (properties && properties.NetWorth) {
            this.drawDonut(this.$el.find("#assets-donut"), "#00ab5d", properties.NetWorth / (properties.NetWorth + properties.SumOfMortgages));
        }
        this.$el.find('[data-toggle="tooltip"]').tooltip({
            'placement': 'bottom'
        });
        cc = this.$el.find("#consumerScoreCanvas");
        this.halfDonut(cc, cc.data('color'), cc.data('percent'));
        cii = this.$el.find("#consumerCIICanvas");
        this.halfDonut(cii, cii.data('color'), cii.data('percent'));
        if (this.expCompany && this.expCompany.length > 0 && !this.expCompany[0].Error) {
            _.each(this.expCompany, (function (_this) {
                return function (c, i) {
                    var compC, equity, profit;
                    compC = _this.$el.find("#companyScoreCanvas" + i);
                    _this.halfDonut(compC, compC.data('color'), compC.data('percent'));
                    if (c.IsLimited) {
                        profit = _.pluck(c.FinDataHistories, 'AdjustedProfit').reverse().join(',');
                        _this.$el.find("#companyProfit" + i).attr('values', profit);
                        equity = _.pluck(c.FinDataHistories, 'TangibleEquity').reverse().join(',');
                        return _this.$el.find("#companyEquity" + i).attr('values', equity);
                    }
                };
            })(this));
        }
        this.$el.find('.bar-sparkline').sparkline("html", {
            type: 'bar',
            barColor: '#cfcfcf',
            height: "50px"
        });
        if (this.experianModel && this.experianModel.get('directorsModels')) {
            directors = this.experianModel.get('directorsModels').length;
            i = 0;
            while (i < directors) {
                cc = this.$el.find("#directorScoreCanvas" + i);
                this.halfDonut(cc, cc.data('color'), cc.data('percent'));
                cii = this.$el.find("#directorCIICanvas" + i);
                this.halfDonut(cii, cii.data('color'), cii.data('percent'));
                i++;
            }
        }
        this.buildJournal();
        this.rotateTable();
        EzBob.handleUserLayoutSetting();
    },
    halfDonut: function (el, fillColor, fillPercent) {
        var canvas, context, counterClockwise, endAngle, lineWidth, radius, startAngle, x, y;
        canvas = el[0];
        context = canvas.getContext('2d');
        x = canvas.width / 2;
        y = canvas.height / 2;
        radius = 40;
        startAngle = 1 * Math.PI;
        endAngle = 2 * Math.PI;
        counterClockwise = false;
        lineWidth = 15;
        context.beginPath();
        context.arc(x, y, radius, startAngle, endAngle, counterClockwise);
        context.lineWidth = lineWidth;
        context.strokeStyle = '#ebebeb';
        context.stroke();
        context.beginPath();
        context.arc(x, y, radius, startAngle, Math.PI * (1 + fillPercent), counterClockwise);
        context.strokeStyle = fillColor;
        context.stroke();
    },
    drawDonut: function (el, fillColor, fillPercent) {
        var canvas, context, endAngle, lineWidth, radius, startAngle, x, y;
        canvas = el[0];
        context = canvas.getContext("2d");
        x = canvas.width / 2;
        y = canvas.height / 2;
        radius = 70;
        startAngle = 1 * Math.PI;
        endAngle = 4 * Math.PI;
        lineWidth = 25;
        context.beginPath();
        context.arc(x, y, radius, startAngle, endAngle, false);
        context.lineWidth = lineWidth;
        context.strokeStyle = "#ebebeb";
        context.stroke();
        context.beginPath();
        context.arc(x, y, radius, startAngle, Math.PI * (1 + fillPercent * 2), false);
        context.strokeStyle = fillColor;
        context.stroke();
    }
});
