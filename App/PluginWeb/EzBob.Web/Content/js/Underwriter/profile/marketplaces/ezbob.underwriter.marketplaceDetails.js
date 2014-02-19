var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.MarketPlaceDetailModel = Backbone.Model.extend({
});

EzBob.Underwriter.MarketPlaceDetails = Backbone.Collection.extend({
    model: EzBob.Underwriter.MarketPlaceDetailModel,
    url: function () {
        return window.gRootPath + "Underwriter/MarketPlaces/Details/" + this.makertplaceId;
    }
});

EzBob.Underwriter.MarketPlaceYodleeDetailModel = Backbone.Model.extend({
    url: function () {
        return window.gRootPath + "Underwriter/MarketPlaces/YodleeDetails/" + this.get("makertplaceId");
    }
});


EzBob.Underwriter.MarketPlaceDetailsView = Backbone.Marionette.View.extend({
    initialize: function () {
        this.template = _.template($('#marketplace-values-template').html());
    },

    render: function () {
        var that = this;
        var aryCGAccounts = $.parseJSON($('div#cg-account-list').text());

        this.shop = this.model.get(this.options.currentId);

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

        var cg = aryCGAccounts[this.shop.get('Name')];
        if (cg)
            sTargetList = ((cg.Behaviour == 0) && !cg.HasExpenses) ? 'marketplaces' : 'accounts';
        else
            sTargetList = this.shop.get('IsPaymentAccount') ? 'accounts' : 'marketplaces';

        data[sTargetList].push(this.shop.toJSON());

        data.hideAccounts = data.accounts.length == 0;
        data.hideMarketplaces = data.marketplaces == 0;

        this.$el.html(this.template(data));
        this.$el.find('a[data-bug-type]').tooltip({ title: 'Report bug' });
        this.$el.find('i[data-yodlee-calculated]').tooltip({ title: 'Calculated Field' });

        if (this.shop.get('Name') == 'Yodlee') {

            that.renderYodlee();
        }

        $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
            if ($(e.target).text() == "Charts") {
                that.yodleeShowGraph();
            }
        });
        return this;
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
        "click .yodleeRuleAdd": "yodleeRuleAddClicked"
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
            aoColumns: [{ sType: "numeric" }, { sType: "string" }, { sType: "date-uk" }, { sType: "formatted-num" }, { sType: "formatted-num" }, { sType: "string" },{ sType: "string" }, { sType: "string" }, { sType: "string" }, { sType: "string" }, { sType: "string" }],
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

        if ($.fn.dataTableExt.afnFiltering.length == 0) {
            $.fn.dataTableExt.afnFiltering.push(
                function(oSettings, aData, iDataIndex) {
                    if (oSettings.nTable !== document.getElementById("YodleeTransactionsTable")) {
                        // if not table should be ignored
                        return true;
                    }
                    var dateRange = that.$el.find('#date-range').attr("value");
                    if (dateRange == null) return true;

                    var dateMin = dateRange.substring(0, 4) + dateRange.substring(5, 7) + dateRange.substring(8, 10);
                    var dateMax = dateRange.substring(13, 17) + dateRange.substring(18, 20) + dateRange.substring(21, 23);
                    var date = aData[2];
                    if (date == null) {
                        return true;
                    }
                    date = date.substring(0, 10);
                    date = date.substring(6, 10) + date.substring(3, 5) + date.substring(0, 2);
                    if (dateMin == "" && date <= dateMax) {
                        return true;
                    } else if (dateMin == "" && date <= dateMax) {
                        return true;
                    } else if (dateMin <= date && "" == dateMax) {
                        return true;
                    } else if (dateMin <= date && date <= dateMax) {
                        return true;
                    }
                    return false;
                }
            );
        }        
        

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
            lowBalanceLine.push([new Date(Date.parse(lowRunningBalance[monthYear1].Date)), parseInt(lowRunningBalance[monthYear1].Balance, 10)]);
        }

        for (var monthYear2 in highRunningBalance) {
            highBalanceLine.push([new Date(Date.parse(highRunningBalance[monthYear2].Date)), parseInt(highRunningBalance[monthYear2].Balance, 10)]);

        }

        for (var date in runningBalance) {
            runningBalanceLine.push([new Date(Date.parse(date)), parseInt(runningBalance[date], 10)]);
            bankFrameLine.push([new Date(Date.parse(date)), parseInt(bankFrame, 10)]);
        }

        if (lowBalanceLine.length && highBalanceLine.length && !this.runningBalancePlot) {
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
                expensesBar.push(parseInt(expenses[i], 10));
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
        $('.YodleeTransactionsTable').dataTable().fnFilter(searchWord, 7); //description id
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