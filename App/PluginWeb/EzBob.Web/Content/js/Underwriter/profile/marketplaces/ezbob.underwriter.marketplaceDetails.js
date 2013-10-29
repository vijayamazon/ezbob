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

EzBob.Underwriter.MarketPlaceDetailsView = Backbone.Marionette.View.extend({
    initialize: function () {
        this.template = _.template($('#marketplace-values-template').html());
    },

    render: function () {
        var that = this;
        var aryCGAccounts = $.parseJSON($('div#cg-account-list').text());

        this.shop = this.model.get(this.options.currentId);
        if (!this.shop) return false;
        var data = { marketplaces: [], accounts: [], summary: null, customerId: this.options.customerId };

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
            this.renderYodlee();
        }

        $('a[data-toggle="tab"]').on('shown', function (e) {
            if ($(e.target).text() == "Charts") {
                that.yodleeShowGraph();
            }
        });
        return this;
    },

    events: {
        "click .reCheckMP": "reCheck",
        "click .reCheck-paypal": "reCheckPayPal",
        "click .renew-token": "renewTokenClicked",
        "click .disable-shop": "diShop",
        "click .enable-shop": "enShop",
        "click .yodleeSearchWordsRow": "searchYodleeWordsRowClicked",
        "click .yodleeSearchWordsAdd": "searchYodleeWordsAddClicked",
        "click .yodleeSearchWordsDelete": "searchYodleeWordsDeleteClicked",
        "click .yodleeReplotGraph": "replotYodleeGraphClicked",
        "click .yodleeAccountsRow": "yodleeAccountsRowClicked"
    },
    renewTokenClicked: function (e) {
        var umi = $(e.currentTarget).data("umi");
        this.trigger("recheck-token", umi);
    },
    reCheck: function (e) {
        this.trigger("reCheck", e);
        return false;
    },
    reCheckPayPal: function (e) {
        this.trigger("reCheck-PayPal", e);
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
            width: "73%",
            height: Math.max(window.innerHeight * 0.9, 600),
            dialogClass: "marketplaceDetail"
        };
    },
    renderYodlee: function () {
        var oDataTableArgs = {
            aLengthMenu: [[10, 25, 50, 100, 200, -1], [10, 25, 50, 100, 200, "All"]],
            iDisplayLength: -1,
            asSorting: [],
            aoColumns: [{ sType: "numeric" }, { sType: "string" }, { sType: "date" }, { sType: "formatted-num" }, { sType: "formatted-num" }, { sType: "string" }, { sType: "string" }, { sType: "string" }, { sType: "string" }],
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
                nCells[0].innerHTML = "|Total #Credit: <i>" + parseInt(iPageCountCredit) + "</i> | Sum Credit: " + EzBob.formatPoundsAsInt(iPageAmountCredit) + " | Total #Debit: <i>" + parseInt(iPageCountDebit) + "</i> | Sum Debit: "+ EzBob.formatPoundsAsInt(iPageAmountDebit);
            },
            "oLanguage": {
                "sSearch": "Filter all columns:"
            }
        };
        var oTable = this.$el.find('.YodleeTransactionsTable').dataTable(oDataTableArgs);

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
        var that = this;
        this.$el.find("div.dataTables_filter input").focus(function() {
            that.$el.find(".YodleeTransactionsTable tfoot input").val("").keyup();
        });

        var cashModel = this.shop.get("Yodlee").CashFlowReportModel;
        var cashFlow = cashModel.YodleeCashFlowReportModelDict;
        var minDay = cashModel.MinDateDict;
        var maxDay = cashModel.MaxDateDict;

        var income = cashFlow["0Total Income"];
        var expenses = cashFlow["1Total Expenses"];
        var numOfTransactionsIncome = cashFlow["5Num Of Transactions"];
        var numOfTransactionsExpenses = cashFlow["9Num Of Transactions"];
        var averageIncome = cashFlow["6Average Income"];
        var averageExpenses = cashFlow["aAverage Expenses"];

        var arrayOfData = [];

        for (var i in income) {
            if (!income.hasOwnProperty(i)) continue;
            if (i == '999999') continue; //skipping total
            arrayOfData.push(
            [
                [
                    [parseInt(expenses[i], 10), parseInt(averageExpenses[i], 10), parseInt(numOfTransactionsExpenses[i], 10)],
                    [parseInt(income[i], 10), parseInt(averageIncome[i], 10), parseInt(numOfTransactionsIncome[i], 10)]
                ], i == '999999' ? 'Total' : minDay[i] + '-' + maxDay[i] + '/' + i.substring(4) + '/' + i.substring(0, 4)
            ]);
        }

        if (arrayOfData.length) {
            this.$el.find("#yodleeBarGraph").jqBarGraph({
                data: arrayOfData,
                colors: ['#FF0000', '#008000'],
                legends: ['Expenses', 'Income'],
                legend: true,
                width: 800,
                //color: '#ffffff',
                type: 'multi',
                postfix: '£',
                title: '<h3>Cash Flow<br /><small>monthly income/expenses as of date ' + EzBob.formatDate(new Date(Date.parse(cashModel.AsOfDate))) + '</small></h3>',
                showValues: true,
                showValuesColor: "#000000",
            });

            this.$el.find("#yodleeBarGraph").css({ height: '340px' });
        }
    },
    yodleeShowGraph: function () {
        var cashModel = this.shop.get("Yodlee").CashFlowReportModel;
        var lowRunningBalance = cashModel.LowRunningBalanceDict;
        var highRunningBalance = cashModel.HighRunningBalanceDict;
        var bankFrame = cashModel.BankFrame;
        var formatedBankFrame = EzBob.formatPoundsAsInt(bankFrame);
        $.jqplot.config.enablePlugins = true;

        var lowBalanceLine = [];
        var highBalanceLine = [];
        for (var monthYear1 in lowRunningBalance) {
            lowBalanceLine.push([new Date(Date.parse(lowRunningBalance[monthYear1].Date)), parseInt(lowRunningBalance[monthYear1].Balance, 10)]);
        }

        for (var monthYear2 in highRunningBalance) {
            highBalanceLine.push([new Date(Date.parse(highRunningBalance[monthYear2].Date)), parseInt(highRunningBalance[monthYear2].Balance, 10)]);
        }
        if (lowBalanceLine.length && highBalanceLine.length) {
            this.runningBalancePlot = $.jqplot('yodleeRunningBalanceChart', [highBalanceLine, lowBalanceLine], {
                animateReplot: true,
                drawIfHidden: true,
                cursor: {
                    show: true,
                    zoom: true,
                    looseZoom: true,
                    showTooltip: true
                },
                seriesColors: ['#008000', '#FF0000'],
                series: [{ label: 'High' }, { label: 'Low' }],
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
                        rendererOptions: { forceTickAt0: true }
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
                    }
                },
                canvasOverlay: {
                    show: true,
                    objects: [
                      {
                          horizontalLine: {
                              name: 'bankFrame',
                              y: bankFrame,
                              lineWidth: 3,
                              color: '#000000',
                              shadow: false,
                              lineCap: 'butt',
                              xOffset: 0,
                              showTooltip: true,
                              tooltipFormatString: 'Credit Limit ' + formatedBankFrame
                          }
                      }
                    ]
                }
            });
            
            $('.jqplot-highlighter-tooltip').addClass('ui-corner-all');
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
        var accountId = $(el.currentTarget).index()+1;
        $('#yodleeTransactionsTabLink').click();
        $('#yodleetab4 .YodleeTransactionsTable [name="search_acct"]').val(accountId).change();
        $('.YodleeTransactionsTable').dataTable().fnFilter(accountId, 0); //account num id
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