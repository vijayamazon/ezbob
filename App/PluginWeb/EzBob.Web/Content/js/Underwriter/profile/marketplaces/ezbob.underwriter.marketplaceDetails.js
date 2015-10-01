var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.MarketPlaceDetailModel = Backbone.Model.extend({
	initialize: function() {
		this.on('change reset', this.recalculate, this);
		this.recalculate();
	},

	url: function() {
		return window.gRootPath + "Underwriter/MarketPlaces/Details/?umi=" + this.get('marketplaceId') + "&history=" + this.get('historyDate');
	},

	recalculate: function() {
		var age = EzBob.SeniorityFormat(this.get('AccountAge'), 0);
		this.set({age: age}, {silent: true});
	}
});

EzBob.Underwriter.MarketPlaceDetailsView = EzBob.MarionetteView.extend({
    initialize: function () {
        this.template = _.template($('#marketplace-values-template').html());
    },

    render: function () {
        var that = this;

        this.shop = this.model;

        if (!this.shop)
            return false;

        var data = {
            marketplaces: [],
            accounts: [],
            summary: null,
            customerId: this.options.customerId,
            personalInfoModel: this.options.personalInfoModel
        };

        var sTargetList = '';

        var cg = EzBob.CgVendors.all()[this.shop.get('Name')];
        if (cg)
            sTargetList = ((cg.Behaviour == 0) && !cg.HasExpenses) ? 'marketplaces' : 'accounts';
        else
            sTargetList = this.shop.get('IsPaymentAccount') ? 'accounts' : 'marketplaces';

        data[sTargetList].push(this.shop.toJSON());

        data.hideAccounts = data.accounts.length == 0;
        data.hideMarketplaces = data.marketplaces == 0;

        this.$el.html(this.template(data));

		this.renderHmrcSummary(data);

        this.$el.find('i[data-yodlee-calculated]').tooltip({ title: 'Calculated Field' });
        this.$el.find('.clear-filter').tooltip({ title: 'Clear all filters', placement: 'bottom' });

        if (this.shop.get('Name') == 'eBay') {
            //drawChart(this.shop.get('Id'));
        }
        
        if (this.shop.get('Name') == 'Yodlee') {
            that.renderYodlee();
        }

        $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
            if ($(e.target).text() == "Charts") {
                that.yodleeShowGraph();
            }
        });

        _.each(this.$el.find('[data-original-title]'), function(elem) {
        	$(elem).tooltip({ title: elem.getAttribute('data-original-title') });
        });
        return this;
    },

	renderHmrcSummary: function(data) {
		var oMp = data.marketplaces[0] || data.accounts[0];

		if (!oMp || !oMp.HmrcData || !oMp.HmrcData.VatReturnSummary || !oMp.HmrcData.VatReturnSummary.length) {
			this.$el.find('.vat-return-summary-display').hide();
			this.$el.find('.vat-return-no-summary').show();
			return;
		} // if

		this.$el.find('.vat-return-no-summary').hide();
		var oDisplay = this.$el.find('.vat-return-summary-display').show();

		var oBank = oMp.HmrcData.BankStatement || {};
		var oAnalBank = oMp.HmrcData.BankStatementAnnualized || {};

		for (var nIdx = 0; nIdx < oMp.HmrcData.VatReturnSummary.length; nIdx++) {
			var oTbl = this.$el.find('#vat-return-summary-template').find('.vat-return-summary').clone(true, true);
			oDisplay.append(oTbl);

			var oSummary = oMp.HmrcData.VatReturnSummary[nIdx];
			if (!isNaN(this.customSalariesMultiplier)) {
			    oSummary.SalariesMultiplier = this.customSalariesMultiplier / 100;
			} else {
			    var isInit = true;
			}
			var oColumns = { total: '.vrs-total', };

			for (var i = 0; i < 5; i++) {
				var oQuarter = oSummary.Quarters[i];
				var sQclass = '.vrs-q' + i;

				if (!oQuarter) {
					oTbl.find(sQclass).remove();
					continue;
				} // if

				oColumns[i] = sQclass;

				oTbl.find(sQclass + '.qtr-dates').text(
					EzBob.formatDateMY(moment(oQuarter.DateFrom).toDate()) + ' - ' +
						EzBob.formatDateMY(moment(oQuarter.DateTo).toDate())
				);

				if (oBank.Period)
					oTbl.find('.bank-period').text(oBank.Period);

				if (i > 0) {
					var oPrevQuarter = oSummary.Quarters[i - 1];

					if (!this.isJustBefore(oPrevQuarter.DateTo, oQuarter.DateFrom))
						oTbl.find(sQclass).addClass('not-just-after').attr('title', 'This period does not immediately follow the previous one.');
				} // if
			} // for

			if (oSummary.RegistrationNo)
				oTbl.find('.business-reg-no').text(oSummary.RegistrationNo);
			else
				oTbl.find('.business-reg-no').html('&ndash;');

			var aryDetails = [oSummary.BusinessName].concat((oSummary.BusinessAddress || '').split('\n'));
			var oDetails = oTbl.find('.business-details');

			_.each(aryDetails, function(s) {
				var t = $.trim(s);

				if (t)
					oDetails.append($('<div />').text(t));
			}); // for

			oTbl.find('tr[data-summary-field]').each(function() {
				var oTR = $(this);
				var sFieldName = oTR.attr('data-summary-field');
				var sFormat = oTR.attr('data-summary-format');

				for (var i in oColumns) {
					var sQclass = oColumns[i];

					var oValue = (i == 'total') ? oSummary[sFieldName] : oSummary.Quarters[i][sFieldName];

					var sDisplayValue = null;
					if (sFieldName == 'Salaries') {
					    oValue = (oValue || 0) * oSummary.SalariesMultiplier;
					}
					if (sFieldName == 'Ebida') {
					    oValue = (i == 'total') ? oSummary['TotalValueAdded'] - (oSummary['Salaries'] * oSummary.SalariesMultiplier) : oSummary.Quarters[i]['TotalValueAdded'] - (oSummary.Quarters[i]['Salaries'] * oSummary.SalariesMultiplier);
					}
					if (sFieldName == 'FreeCashFlow') {
					    oValue = (i == 'total') ? oSummary['TotalValueAdded'] - (oSummary['Salaries'] * oSummary.SalariesMultiplier) - oSummary['ActualLoanRepayment'] : oSummary.Quarters[i]['TotalValueAdded'] - (oSummary.Quarters[i]['Salaries'] * oSummary.SalariesMultiplier) - oSummary.Quarters[i]['ActualLoanRepayment'];
					}
				    
					if (oValue) {
						switch (sFormat) {
						case '%':
							sDisplayValue = EzBob.formatPercents(oValue);
							break;
						case '%0':
							sDisplayValue = EzBob.formatPercents0(oValue);
							break;
						case '£i':
							sDisplayValue = EzBob.formatPoundsAsInt(oValue);
							break;
						} // switch
					} // if

					// console.log(i, ':', sFieldName, '=', oValue, '->', sDisplayValue);

					if (sDisplayValue)
					    oTR.find(sQclass).text(sDisplayValue).addClass(NegativeNum(oValue));
					else
						oTR.find(sQclass).html('&ndash;');
				} // for each quarter/total column

				switch (sFieldName) {
				case 'Revenues':
				    oTR.find('.bank').text(EzBob.formatPoundsAsInt(oBank.Revenues)).addClass(NegativeNum(oBank.Revenues));
				    var revenues = oAnalBank.Revenues / oSummary.Revenues - 1;
				    oTR.find('.annualized').text(EzBob.formatPercents(revenues, 2)).addClass(NegativeNum(revenues));
					break;
				case 'Opex':
				    oTR.find('.bank').text(EzBob.formatPoundsAsInt(oBank.Opex)).addClass(NegativeNum(oBank.Opex));
				    var opex = oAnalBank.Opex / oSummary.Opex - 1;
				    oTR.find('.annualized').text(EzBob.formatPercents(opex, 2)).addClass(NegativeNum(opex));
					break;
				case 'TotalValueAdded':
				    oTR.find('.bank').text(EzBob.formatPoundsAsInt(oBank.TotalValueAdded)).addClass(NegativeNum(oBank.TotalValueAdded));
				    var tva = oAnalBank.TotalValueAdded / oSummary.TotalValueAdded - 1;
				    oTR.find('.annualized').text(EzBob.formatPercents(tva, 2)).addClass(NegativeNum(tva));
					break;
				case 'PctOfRevenues':
				    oTR.find('.bank').text(EzBob.formatPercents(oBank.PercentOfRevenues, 2)).addClass(NegativeNum(oBank.PercentOfRevenues));
					break;
				case 'Salaries':
				    var nCalcd = (oSummary.Salaries || 0) * oSummary.SalariesMultiplier;
				    if (isInit) {
				        oTR.find('.salaries-multiplier').percentFormat().autoNumeric('set', oSummary.SalariesMultiplier*100).blur();
				    } else {
				        oTR.find('.salaries-multiplier').percentFormat().autoNumeric('set', oSummary.SalariesMultiplier*100);
				    }
					oTR.find('.total').text(EzBob.formatPoundsAsInt(oSummary.Salaries || 0)).addClass(NegativeNum(oSummary.Salaries));
					oTR.find('.vrs-total').text(EzBob.formatPoundsAsInt(nCalcd)).addClass(NegativeNum(nCalcd));
					oTR.find('.bank').text(EzBob.formatPoundsAsInt(oBank.Salaries)).addClass(NegativeNum(oBank.Salaries));
				    var salaries = (nCalcd ? oAnalBank.Salaries / nCalcd : 0) - 1;
				    oTR.find('.annualized').text(EzBob.formatPercents(salaries, 2)).addClass(NegativeNum(salaries));
					break;
				case 'Tax':
				    oTR.find('.bank').text(EzBob.formatPoundsAsInt(oBank.Tax)).addClass(NegativeNum(oBank.Tax));
					break;
				case 'Ebida':
				    oTR.find('.bank').text(EzBob.formatPoundsAsInt(oBank.Ebida)).addClass(NegativeNum(oBank.Ebida));
				    var ebida = oAnalBank.Ebida / oSummary.Ebida - 1;
				    oTR.find('.annualized').text(EzBob.formatPercents(ebida, 2)).addClass(NegativeNum(ebida));
					break;
				case 'ActualLoanRepayment':
				    oTR.find('.bank').text(EzBob.formatPoundsAsInt(oBank.ActualLoansRepayment)).addClass(NegativeNum(oBank.ActualLoansRepayment));
					break;
				case 'FreeCashFlow':
				    oTR.find('.bank').text(EzBob.formatPoundsAsInt(oBank.FreeCashFlow)).addClass(NegativeNum(oBank.FreeCashFlow));
				    var fcf = oAnalBank.FreeCashFlow / oSummary.Ebida - 1;
				    oTR.find('.annualized').text(EzBob.formatPercents(fcf, 2)).addClass(NegativeNum(fcf));
					break;
				} // switch
			}); // for each row
		} // for each summary item
	}, // renderHmrcSummary
    hmrcSalariesMultiplierChanged : function(e) {
        var val = $(e.currentTarget).autoNumeric('get');
        if (this.customSalariesMultiplier != val) {
            this.customSalariesMultiplier = val;
            this.render();
        }
        
    },
    events: {
        "click .reCheckMP": "reCheck",
        "click .renew-token": "renewTokenClicked",
        "click .disable-shop": "diShop",
        "click .enable-shop": "enShop",
        "click .yodleeSearchWordsRow": "searchYodleeWordsRowClicked",
        "click .yodleeSearchWordsAdd": "searchYodleeWordsAddClicked",
        "click .yodleeSearchWordsDelete": "searchYodleeWordsDeleteClicked",
        "click .yodleeReplotGraph": "replotYodleeGraphClicked",
        "click .yodleeAccountsRow": "yodleeAccountsRowClicked",
        "click .yodleeShowTransactionsInRange": "yodleeShowTrnInRangeClicked",
        "click .yodleeRuleAdd": "yodleeRuleAddClicked",
        "change .salaries-multiplier": "hmrcSalariesMultiplierChanged"
    },
    renewTokenClicked: function (e) {
        var umi = $(e.currentTarget).data("umi");
        this.trigger("recheck-token", umi);
    },
    reCheck: function (e) {
        this.trigger("reCheck", e);
        return false;
    },
    enShop: function (e) {
        this.trigger("enable-shop", e);
        return false;
    },
    diShop: function (e) {
        this.trigger("disable-shop", e);
        return false;
    },
    recheckAskville: function (e) {
        var el = $(e.currentTarget);
        var guid = el.attr("data-guid");
        var marketplaceId = el.attr("data-marketplaceId");
        EzBob.ShowMessage(
            "", "Are you sure?",
            function () {
                BlockUi('on');
                $.post(window.gRootPath + "Customer/AmazonMarketPlaces/Askville", { askvilleGuid: guid, customerMarketPlaceId: marketplaceId })
                .done(function (askvilleStatus) {
                    $("#recheck-askville").closest("tr").find('.askvilleStatus').text(askvilleStatus);
                    EzBob.ShowMessage("Successfully", "The askville recheck was starting. ", null, "OK");
                })
                .done(function () {
                    BlockUi('off');
                });
            }, "Yes", null, "No");
        return false;
    },
    jqoptions: function () {
        return {
            modal: true,
            resizable: false,
            title: this.model.get('Name'),
            position: "center",
            draggable: false,
            width: "90%",
            height: Math.max(window.innerHeight * 0.9, 600),
            dialogClass: "marketplaceDetail"
        };
    },
    renderYodlee: function () {
        var oDataTableArgs = {
            aLengthMenu: [[10, 25, 50, 100, 200, -1], [10, 25, 50, 100, 200, "All"]],
            iDisplayLength: -1,
            asSorting: [],
            aoColumns: [{ sType: "numeric" }, { sType: "string" }, { sType: "date-uk" }, { sType: "formatted-num" }, { sType: "formatted-num" }, { sType: "string" }, { sType: "string" }, { sType: "string" }, { sType: "string" }, { sType: "string" }, { sType: "string" }],
            "fnFooterCallback": function (nRow, aaData, iStart, iEnd, aiDisplay) {
                //calculate totals per selection
                //var iTotalMarket = 0;
                //for (var i = 0 ; i < aaData.length ; i++) {
                //    iTotalMarket += EzBob.poundToInt(aaData[i][3]) * 1;
                //}

                //calculate totals per page
                var iPageAmountCredit = 0;
                var iPageAmountDebit = 0;
                var iPageCountCredit = 0;
                var iPageCountDebit = 0;
                for (var i = iStart ; i < iEnd ; i++) {
                    if (aaData[aiDisplay[i]][1] == 'credit') {
                        iPageAmountCredit += EzBob.poundToInt(aaData[aiDisplay[i]][3]) * 1;
                        iPageCountCredit++;
                    } else {
                        iPageAmountDebit += EzBob.poundToInt(aaData[aiDisplay[i]][3]) * 1;
                        iPageCountDebit++;
                    }
                }

                /* Modify the footer row to match what we want */
                var nCells = nRow.getElementsByTagName('th');
                nCells[0].innerHTML = "|Total #Credit: <i>" + parseInt(iPageCountCredit) + "</i> | Sum Credit: " + EzBob.formatPoundsAsInt(iPageAmountCredit) + " | Total #Debit: <i>" + parseInt(iPageCountDebit) + "</i> | Sum Debit: " + EzBob.formatPoundsAsInt(iPageAmountDebit);
            },
            "oLanguage": {
                "sSearch": "Filter all columns:"
            }
        };
        var that = this;
        var oTable = this.$el.find('.YodleeTransactionsTable').dataTable(oDataTableArgs);
        EzBob.DataTables.Helper.initCustomFiltering();
        this.$el.find("#DataTables_Table_1_length").after(this.$el.find('#range-filter'));
        this.$el.find("#date-range").keyup(function () { oTable.fnDraw(); });

        this.$el.find(".YodleeTransactionsTable tfoot input").click(function () {
            oTable.fnFilter(this.value, $("tfoot input").index(this));
        });
        this.$el.find(".YodleeTransactionsTable tfoot input").keyup(function () {
            oTable.fnFilter(this.value, $("tfoot input").index(this));
        });

        var asInitVals = new Array();
        this.$el.find(".YodleeTransactionsTable tfoot input").each(function (i) { asInitVals[i] = this.value; });

        this.$el.find(".YodleeTransactionsTable tfoot input").focus(function () {
            if (this.className == "search_init") { this.value = ""; }
        });

        this.$el.find(".YodleeTransactionsTable tfoot input").blur(function (i) {
            if (this.value == "") { this.value = asInitVals[$("tfoot input").index(this)]; }
        });

        this.$el.find("div.dataTables_filter input").focus(function () {
            that.$el.find(".YodleeTransactionsTable tfoot input").val("").keyup();
        });

        this.$el.find("button.clear-filter").click(function () {
            that.$el.find(".YodleeTransactionsTable tfoot input").val("").keyup();
            that.$el.find("#date-range").val("").keyup();
            that.$el.find("#YodleeTransactionsTable_filter input").val("").keyup();
        });
    },
    yodleeShowGraph: function () {
        var cashModel = this.shop.get("Yodlee").CashFlowReportModel;
        var runningBalanceModel = this.shop.get("Yodlee").RunningBalanceModel;
        var cashFlow = cashModel.YodleeCashFlowReportModelDict;
        var lowRunningBalance = runningBalanceModel.LowRunningBalanceDict;
        var highRunningBalance = runningBalanceModel.HighRunningBalanceDict;
        var runningBalance = runningBalanceModel.MergedDailyRunningBalanceDict;
        var bankFrame = runningBalanceModel.BankFrame;
        var formatedBankFrame = EzBob.formatPoundsAsInt(bankFrame);
        $.jqplot.config.enablePlugins = true;

        var lowBalanceLine = [];
        var highBalanceLine = [];
        var runningBalanceLine = [];
        var bankFrameLine = [];
        for (var monthYear1 in lowRunningBalance) {
            lowBalanceLine.push([new Date(moment(lowRunningBalance[monthYear1].Date)), parseInt(lowRunningBalance[monthYear1].Balance, 10)]);
        }

        for (var monthYear2 in highRunningBalance) {
            highBalanceLine.push([new Date(moment(highRunningBalance[monthYear2].Date)), parseInt(highRunningBalance[monthYear2].Balance, 10)]);

        }

        for (var r in runningBalance) {
            runningBalanceLine.push([new Date(moment(runningBalance[r].Date)), parseInt(runningBalance[r].Balance, 10)]);
            bankFrameLine.push([new Date(moment(runningBalance[r].Date)), parseInt(bankFrame, 10)]);
        }

        
        if (lowBalanceLine.length && highBalanceLine.length && !this.runningBalancePlot) {
            var firstDate = new Date(moment(runningBalance[0].Date));
            var lastDate = new Date(moment(runningBalance[runningBalance.length - 1].Date));

            this.runningBalancePlot = $.jqplot('yodleeRunningBalanceChart', [highBalanceLine, lowBalanceLine, runningBalanceLine, bankFrameLine], {
                animateReplot: true,
                drawIfHidden: true,
                cursor: {
                    show: true,
                    zoom: true,
                    looseZoom: true,
                    showTooltip: true
                },
                seriesColors: ['#008000', '#FF0000', '#A9A9A9', '#000000'],
                series: [
                    { label: 'High' },
                    { label: 'Low' },
                    {
                        label: 'Running Balance ' + formatedBankFrame,
                        trendline: {
                            show: false
                        },
                        markerOptions: {
                            show: false,
                            lineWidth: 1,
                        }
                    },
                    {
                        label: "Credit Limit",
                        trendline: {
                            show: false
                        },
                        markerOptions: {
                            show: false,
                            lineWidth: 1,
                        }
                    }
                ],
                axes: {
                    xaxis: {
                        renderer: $.jqplot.DateAxisRenderer,
                        label: 'Date',
                        labelRenderer: $.jqplot.CanvasAxisLabelRenderer,
                        tickRenderer: $.jqplot.CanvasAxisTickRenderer,
                        tickOptions: {
                            labelPosition: 'middle',
                            angle: 15,
                            formatString: '%d-%m-%y'
                        },
                        min: firstDate,
                        max: lastDate
                    },
                    yaxis: {
                        label: 'Balance',
                        labelRenderer: $.jqplot.CanvasAxisLabelRenderer,
                        tickOptions: {
                            formatString: "%'d £"
                        },
                        rendererOptions: { forceTickAt0: true, forceFit: true }
                    }
                },
                highlighter: {
                    show: true,
                    sizeAdjust: 1,
                    tooltipOffset: 0
                },
                legend: {
                    show: false,
                    //renderer: $.jqplot.EnhancedLegendRenderer,
                    //border: 'none',
                    //marginRight: '-5px',
                    //background: 'rgba(0,0,0,0)',
                    ////placement: 'outside',
                    //location: 'e',
                },
                grid: {
                    drawBorder: false,
                    shadow: false,
                    background: 'rgba(0,0,0,0)'
                },
                seriesDefaults: {
                    trendline: {
                        linePattern: '-.',
                        showMarker: false,
                        shadow: false,
                        rendererOptions: {
                            smooth: true
                        }
                    },
                    markerOptions: {
                        lineWidth: 2,
                    },
                    pointLabels: {
                        show: false
                    }
                },
                //canvasOverlay: {
                //    show: true,
                //    objects: [
                //      {
                //          horizontalLine: {
                //              name: 'bankFrame',
                //              y: bankFrame,
                //              lineWidth: 3,
                //              color: '#000000',
                //              shadow: false,
                //              lineCap: 'butt',
                //              xOffset: 0,
                //              showTooltip: true,
                //              tooltipFormatString: 'Credit Limit ' + formatedBankFrame,
                //          }
                //      }
                //    ]
                //}
            });

            $('.jqplot-highlighter-tooltip').addClass('ui-corner-all');
            //////////////////////////////////////////////////////////////////////////////////////////
            var minDay = cashModel.MinDateDict;
            var maxDay = cashModel.MaxDateDict;
            var income = cashFlow["0aTotal Income"];
            var expenses = cashFlow["0bTotal Expenses"];
            var numOfTransactionsIncome = cashFlow["cNum Of Transactions"];
            var numOfTransactionsExpenses = cashFlow["eNum Of Transactions"];
            var averageIncome = cashFlow["dAverage Income"];
            var averageExpenses = cashFlow["fAverage Expenses"];
            var ticks = [];
            var incomeBar = [];
            var expensesBar = [];
            var incomeLabels = [];
            var expensesLabels = [];

            for (var i in income) {
                if (!income.hasOwnProperty(i)) continue;
                if (i == '999999') continue; //skipping total

                ticks.push(i == '999999' ? 'Total' : minDay[i] + '-' + maxDay[i] + '/' + i.substring(4) + '/' + i.substring(0, 4));
                incomeBar.push(parseInt(income[i], 10));
                expensesBar.push(Math.abs(parseInt(expenses[i]), 10));
                incomeLabels.push("<p class='yodlee-cashflow-graph-labels'>" + EzBob.formatPoundsAsThousands(income[i], 10) + "<br>A " + EzBob.formatPoundsAsThousands(averageIncome[i], 10) + "<br># " + parseInt(numOfTransactionsIncome[i], 10) + "</p>");
                expensesLabels.push("<p class='yodlee-cashflow-graph-labels'>" + EzBob.formatPoundsAsThousands(expenses[i], 10) + "<br>A " + EzBob.formatPoundsAsThousands(averageExpenses[i], 10) + "<br># " + parseInt(numOfTransactionsExpenses[i], 10) + "</p>");
            }

            this.IncomeExpensesBarGraph = $.jqplot('yodleeBarGraph', [expensesBar, incomeBar], {
                seriesDefaults: {
                    renderer: $.jqplot.BarRenderer,
                    trendline: {
                        show: false
                    }
                },
                seriesColors: ['#FF0000', '#008000'],
                series: [
                    {
                        label: 'Expenses',
                        pointLabels: {
                            show: true,
                            labels: expensesLabels,
                            escapeHTML: false,
                            location: 'n',
                            ypadding: -8
                        }
                    },
                    {
                        label: 'Income',
                        pointLabels: {
                            show: true,
                            labels: incomeLabels,
                            escapeHTML: false,
                            location: 'n',
                            ypadding: -8
                        }
                    }
                ],
                legend: {
                    show: true,
                    placement: 'outsideGrid'
                },
                axes: {
                    xaxis: {
                        renderer: $.jqplot.CategoryAxisRenderer,
                        ticks: ticks
                    },
                    yaxis: {
                        pad: 1.05,
                        tickOptions: {
                            formatString: "%'d £"
                        },
                    }
                },
                grid: {
                    drawBorder: false,
                    shadow: false,
                    background: 'rgba(0,0,0,0)'
                },
                highlighter: {
                    show: false,
                },
                cursor: {
                    show: false,
                },
            });
            /*
            $('#yodleeBarGraph').bind('jqplotDataHighlight',
                    function (ev, seriesIndex, pointIndex, data) {
                        $('#yodleeBarInfo').html(data[1]);
                    }
                );
            */
        }
    },
    replotYodleeGraphClicked: function () {
        $.jqplot.config.enablePlugins = true;
        this.runningBalancePlot.replot({ resetAxes: true });
    },
    searchYodleeWordsRowClicked: function (el) {
        var searchWord = $(el.currentTarget).children("td:first").text();
        $('#yodleeTransactionsTabLink').click();
        $('#yodleetab4 .YodleeTransactionsTable [name="search_description"]').val(searchWord).change();
        $('.YodleeTransactionsTable').dataTable().fnFilter(searchWord, 9); //description id
    },
    yodleeAccountsRowClicked: function (el) {
        var accountId = $(el.currentTarget).index() + 1;
        $('#yodleeTransactionsTabLink').click();
        $('#yodleetab4 .YodleeTransactionsTable [name="search_acct"]').val(accountId).change();
        $('.YodleeTransactionsTable').dataTable().fnFilter(accountId, 0); //account num id
    },
    yodleeShowTrnInRangeClicked: function () {
        var minDate = moment.utc(this.runningBalancePlot.axes.xaxis.min).format('YYYY-MM-DD');
        var maxDate = moment.utc(this.runningBalancePlot.axes.xaxis.max).format('YYYY-MM-DD');
        $('#yodleeTransactionsTabLink').click();
        $('#yodleetab4 #date-range').val(minDate + ' - ' + maxDate).keyup();
    },
    yodleeRuleAddClicked: function () {
        var that = this;
        var group = this.$el.find("#yodleeGroup").val();
        var rule = this.$el.find("#yodleeRule").val();
        var literal = this.$el.find("#yodleeLiteral").val();
        if (!group || !rule) return false;
        if ((rule == 1 || rule == 5) && !literal) return false; //include/dont include literal rules

        EzBob.ShowMessage(
            "Add rule", "Are you sure you want to add rule: " + this.$el.find("#yodleeRule option:selected").text() + " to group: " + this.$el.find("#yodleeGroup option:selected").text() + "?",
            function () {
                BlockUi('on');

                $.post(window.gRootPath + "Underwriter/MarketPlaces/AddYodleeRule", { group: group, rule: rule, literal: literal })
                .done(function () {
                    that.model.fetch().done(function () {
                        that.render();
                        $('#yodleeRulesTab').click();
                        BlockUi('off');
                        EzBob.ShowMessage("Successfully Added", "The rule added successfully. ", null, "OK");
                    });
                });
            }, "Yes", null, "No");
        return false;
    },
    searchYodleeWordsAddClicked: function () {
        var that = this;
        var word = this.$el.find("#yodleeAddSearchWordTxt").val();
        if (!word) return false;
        EzBob.ShowMessage(
            "", "Are you sure you want to add word " + word + "?",
            function () {
                BlockUi('on');

                $.post(window.gRootPath + "Underwriter/MarketPlaces/AddSearchWord", { word: word })
                .done(function () {
                    that.model.fetch().done(function () {
                        that.render();
                        $('#yodleeSearchWordsTab').click();
                        BlockUi('off');
                        EzBob.ShowMessage("Successfully Added", "The word added successfully. ", null, "OK");
                    });
                });
            }, "Yes", null, "No");
        return false;
    },
    searchYodleeWordsDeleteClicked: function () {
        var word = this.$el.find("#yodleeSearchWordsDdl option:selected").text();
        var that = this;
        if (!word) return false;
        EzBob.ShowMessage(
            "", "Are you sure you want to remove " + word + "?",
            function () {
                BlockUi('on');
                $.post(window.gRootPath + "Underwriter/MarketPlaces/DeleteSearchWord", { word: word })
                .done(function () {
                    that.model.fetch().done(function () {
                        that.render();
                        $('#yodleeSearchWordsTab').click();
                        BlockUi('off');
                        EzBob.ShowMessage("Successfully Removed", "The word deleted successfully. ", null, "OK");
                    });
                });
            }, "Yes", null, "No");
        return false;
    }
});