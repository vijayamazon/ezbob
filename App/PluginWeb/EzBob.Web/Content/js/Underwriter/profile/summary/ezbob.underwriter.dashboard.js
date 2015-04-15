var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.DashboardView = Backbone.Marionette.ItemView.extend({
    template: '#dashboard-template',
    initialize: function (options) {
        this.crmModel = options.crmModel;
        this.personalModel = options.personalModel;
        this.experianModel = options.experianModel;
        this.propertiesModel = options.propertiesModel;
        this.loanModel = options.loanModel;
        this.companyModel = options.companyModel;
        this.affordability = options.affordability;

        this.bindTo(this.model, 'change sync', this.render, this);
        this.bindTo(this.personalModel, 'change sync', this.personalModelChanged, this);
        this.bindTo(this.experianModel, 'change sync', this.render, this);
        this.bindTo(this.companyModel, 'change sync', this.render, this);
        this.bindTo(this.propertiesModel, 'change sync', this.render, this);
        this.bindTo(this.loanModel, 'change sync', this.render, this);
        this.bindTo(this.affordability, 'change sync', this.renderAffordability, this);
        this.bindTo(this.crmModel, 'change sync', this.render, this);

        this.expCompany = [];
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

        return {
            m: this.model.toJSON(),
            experian: this.experianModel.toJSON(),
            company: this.expCompany,
            properties: this.propertiesModel.toJSON(),
            loan: this.loanModel.toJSON(),
            personal: this.personalModel.toJSON()
        };
    },

    events: {
        'click a[href^="#companyExperian"]': 'companyChanged',
        'click a[href="#customerExperian"]': 'consumerChanged',
        'click a[href^="#director"]': 'consumerChanged',
        'change label.journal-filter input': 'journalFilter'
    },
    companyChanged: function (e) {
	    var obj = e.currentTarget;
        this.$el.find('.company-name').text($(obj).data('companyname') + ' ' + $(obj).data('companyref'));
        this.drawSparklineGraphs();
    },
    consumerChanged: function (e) {
        var obj = e.currentTarget;
        this.$el.find('.applicant-name').text($(obj).data('applicantname'));
        this.drawSparklineGraphs();
    },
    personalModelChanged: function (e, a) {
        if (e && a && this.model) {
            this.model.fetch();
        }
    },
    journalFilter: function(e) {
	    this.journalView.journalFilter(e);
    },
    renderAffordability: function () {
        this.affordabilityView = new EzBob.Underwriter.AffordabilityView({
            el: this.$el.find('#affordability'),
            model: this.affordability
        });
        this.affordabilityView.render();
    },
    onRender: function () {
        this.experianSpark();
        this.drawGraphs();
        this.$el.find('a[data-bug-type]').tooltip({
            title: 'Report bug'
        });
        this.$el.find('[data-toggle="tooltip"]').tooltip({
            'placement': 'bottom'
        });

        var that = this;
        this.$el.find('a[data-toggle="tab"]').on('shown.bs.tab', function () {
            that.drawSparklineGraphs();
        });

       

        if (this.crmModel.customerId) {
        	this.journalView = new EzBob.Underwriter.JournalView({
        		el: this.$el.find('#journal'),
        		model: this.model,
        		crmModel: this.crmModel
        	});
        
        	this.journalView.render();
        }
        this.renderAffordability();
	    EzBob.handleUserLayoutSetting();
    },
    
    experianSpark: function () {
        var that = this;
        if (this.experianModel && this.experianModel.get('Consumer')) {
            var historyConsumerSorted = _.sortBy(this.experianModel.get('Consumer').ConsumerHistory, function (history) {
                return history.Date;
            });
            var consumerHistoryScores = _.pluck(historyConsumerSorted, 'Score').join(',');
            var consumerHistoryCIIs = _.pluck(historyConsumerSorted, 'CII').join(',');
            var consumerHistoryCais = _.pluck(historyConsumerSorted, 'Balance').join(',');
            this.$el.find(".consumerScoreGraph").attr('values', consumerHistoryScores);
            this.$el.find(".consumerCIIGraph").attr('values', consumerHistoryCIIs);
            this.$el.find(".consumerBalanceGraph").attr('values', consumerHistoryCais);
        }

        if (this.experianModel && this.experianModel.get('Directors')) {
            _.each(this.experianModel.get('Directors'), function (director, i) {
                var historyDirectorSorted = _.sortBy(director.ConsumerHistory, function (history) {
                    return history.Date;
                });
                var directorHistoryScores = _.pluck(historyDirectorSorted, 'Score').join(',');
                var directorHistoryCIIs = _.pluck(historyDirectorSorted, 'CII').join(',');
                var directorHistoryCais = _.pluck(historyDirectorSorted, 'Balance').join(',');
                that.$el.find(".directorScoreGraph" + i).attr('values', directorHistoryScores);
                that.$el.find(".directorCIIGraph" + i).attr('values', directorHistoryCIIs);
                that.$el.find(".directorBalanceGraph" + i).attr('values', directorHistoryCais);
            });
        }

        if (this.expCompany) {
            _.each(this.expCompany, function(c, i) {
                if (c.CompanyHistories && c.CompanyHistories.length > 0) {
                    var historyCompanyScoresSorted = _.sortBy(c.CompanyHistories, function (history) {
                        return history.Date;
                    });
                    var companyHistoryScores = _.pluck(historyCompanyScoresSorted, 'Score').join(',');
                    var companyHistoryCais = _.pluck(historyCompanyScoresSorted, 'Balance').join(',');
                    that.$el.find(".companyScoreGraph" + i).attr('values', companyHistoryScores);
                    that.$el.find(".companyCaisBalanceGraph" + i).attr('values', companyHistoryCais);
                }
            });
        }
	    
        if (this.expCompany && this.expCompany.length > 0 && !this.expCompany[0].Error) {
        	_.each(this.expCompany, function (c, j) {
        		var compC = that.$el.find("#companyScoreCanvas" + j);
        		that.halfDonut(compC, compC.data('color'), compC.data('percent'));
        		if (c.IsLimited) {
        			var profit = _.pluck(c.FinDataHistories, 'AdjustedProfit').reverse().join(',');
        			that.$el.find("#companyProfit" + j).attr('values', profit);
        			var equity = _.pluck(c.FinDataHistories, 'TangibleEquity').reverse().join(',');
        			that.$el.find("#companyEquity" + j).attr('values', equity);
        		}
        	});
        }

        this.drawSparklineGraphs();
    },
    drawSparklineGraphs: function() {
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
	    
        this.$el.find('.bar-sparkline').sparkline("html", {
        	type: 'bar',
        	barColor: '#cfcfcf',
        	height: "50px",
        	barWidth: 6
        });
    },
    drawGraphs: function() {
        var properties = this.propertiesModel.toJSON();
        if (properties && properties.NetWorth) {
            this.drawDonut(this.$el.find("#assets-donut"), "#00ab5d", properties.NetWorth / (properties.NetWorth + properties.SumOfMortgages));
        }
        var cc = this.$el.find("#consumerScoreCanvas");
        this.halfDonut(cc, cc.data('color'), cc.data('percent'));
        var cii = this.$el.find("#consumerCIICanvas");
        this.halfDonut(cii, cii.data('color'), cii.data('percent'));

        if (this.experianModel && this.experianModel.get('Directors')) {
            var directors = this.experianModel.get('Directors').length;
            var i = 0;
            while (i < directors) {
                cc = this.$el.find("#directorScoreCanvas" + i);
                this.halfDonut(cc, cc.data('color'), cc.data('percent'));
                cii = this.$el.find("#directorCIICanvas" + i);
                this.halfDonut(cii, cii.data('color'), cii.data('percent'));
                i++;
            }
        }
    },
    halfDonut: function (el, fillColor, fillPercent) {
	    var canvas = el[0];
        if (!canvas) return false;
        var context = canvas.getContext('2d');
        var x = canvas.width / 2;
        var y = canvas.height / 2;
        var radius = 40;
        var startAngle = 1 * Math.PI;
        var endAngle = 2 * Math.PI;
        var lineWidth = 15;
        context.beginPath();
        context.arc(x, y, radius, startAngle, endAngle, false);
        context.lineWidth = lineWidth;
        context.strokeStyle = '#ebebeb';
        context.stroke();
        context.beginPath();
        context.arc(x, y, radius, startAngle, Math.PI * (1 + fillPercent), false);
        context.strokeStyle = fillColor;
        context.stroke();
	    return false;
    },
    drawDonut: function (el, fillColor, fillPercent) {
	    var canvas = el[0];
        if (!canvas) return false;
        var context = canvas.getContext('2d');
        var x = canvas.width / 2;
        var y = canvas.height / 2;
        var radius = 70;
        var startAngle = 1 * Math.PI;
        var endAngle = 4 * Math.PI;
        var lineWidth = 25;
        context.beginPath();
        context.arc(x, y, radius, startAngle, endAngle, false);
        context.lineWidth = lineWidth;
        context.strokeStyle = '#ebebeb';
        context.stroke();
        context.beginPath();
        context.arc(x, y, radius, startAngle, Math.PI * (1 + fillPercent * 2), false);
        context.strokeStyle = fillColor;
        context.stroke();
	    return false;
    }
});
