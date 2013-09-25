﻿var EzBob = EzBob || {};
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
        var aryCGAccounts = $.parseJSON($('div#cg-account-list').text());

        var shop = this.model.get(this.options.currentId);
        // drawChart(shop.get("Id"));
        var data = { marketplaces: [], accounts: [], summary: null, customerId: this.options.customerId };

        var sTargetList = '';

        var cg = aryCGAccounts[shop.get('Name')];
        if (cg)
            sTargetList = ((cg.Behaviour == 0) && !cg.HasExpenses) ? 'marketplaces' : 'accounts';
        else
            sTargetList = shop.get('IsPaymentAccount') ? 'accounts' : 'marketplaces';

        data[sTargetList].push(shop.toJSON());

        data.hideAccounts = data.accounts.length == 0;
        data.hideMarketplaces = data.marketplaces == 0;

        this.$el.html(this.template(data));
        this.$el.find('a[data-bug-type]').tooltip({ title: 'Report bug' });

        if (shop.get('Name') == 'Yodlee') {
            this.renderYodlee(shop);
        }

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
        "click .yodleeSearchWordsDelete": "searchYodleeWordsDeleteClicked"
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
    renderYodlee: function (shop) {
        var oDataTableArgs = {
            aLengthMenu: [[10, 25, 50, 100, 200, -1], [10, 25, 50, 100, 200, "All"]],
            iDisplayLength: -1,
            asSorting: [],
            aoColumns: [{ sType: "string" }, { sType: "date" }, { sType: "formatted-num" }, { sType: "string" }, { sType: "string" }, { sType: "string" }]
        };
        this.$el.find('.YodleeTransactionsTable').dataTable(oDataTableArgs);


        var cashModel = shop.get("Yodlee").CashFlowReportModel;
        var cashFlow = cashModel.YodleeCashFlowReportModelDict;
        var minDay = cashModel.MinDateDict;
        var maxDay = cashModel.MaxDateDict;

        var income = cashFlow["2Total Income"];
        var expenses = cashFlow["7Total Expenses"];
        var numOfTransactionsIncome = cashFlow["3Num Of Transactions"];
        var numOfTransactionsExpenses = cashFlow["8Num Of Transactions"];
        var averageIncome = cashFlow["4Average Income"];
        var averageExpenses = cashFlow["9Average Expenses"];

        var arrayOfData = [];

        for (var i in income) {
            if (!income.hasOwnProperty(i)) continue;
            arrayOfData.push(
            [
                [
                    [parseInt(expenses[i], 10), parseInt(averageExpenses[i], 10), parseInt(numOfTransactionsExpenses[i], 10)],
                    [parseInt(income[i], 10), parseInt(averageIncome[i], 10), parseInt(numOfTransactionsIncome[i], 10)]
                ], i == '999999' ? 'Total' : minDay[i] + '-' + maxDay[i] + '/' + i.substring(4) + '/' + i.substring(0, 4)
            ]);
            console.log(i, income[i], expenses[i]);
        }
        console.log(arrayOfData);

        $("#yodleeBarGraph").jqBarGraph({
            data: arrayOfData,
            colors: ['#FF0000', '#008000'],
            legends: ['Expenses', 'Income'],
            legend: true,
            width: 800,
            //color: '#ffffff',
            type: 'multi',
            postfix: '£',
            title: '<h3>Cash Flow<br /><small>monthly income/expenses</small></h3>',
            showValues: true,
            showValuesColor: "#000000",
        });

    },
    searchYodleeWordsRowClicked: function (el) {
        var searchWord = $(el.currentTarget).children("td:first").text();
        $('#yodleeTabLink5').click();
        $('#yodleetab5 .dataTables_filter input').val(searchWord).change();
        $('.YodleeTransactionsTable').dataTable().fnFilter(searchWord, null);
    },

    searchYodleeWordsAddClicked: function () {
        var that = this;
        var word = this.$el.find("#yodleeAddSearchWordTxt").val();
        if (!word) return false;
        EzBob.ShowMessage(
            "", "Are you sure you whant to add word " + word + "?",
            function () {
                BlockUi('on');

                $.post(window.gRootPath + "Underwriter/MarketPlaces/AddSearchWord", { word: word })
                .done(function () {
                    EzBob.ShowMessage("Successfully Added", "The word added successfully. ", null, "OK");
                })
                .done(function () {
                    BlockUi('off');
                    $("#yodleeSearchWordsDdl").append("<option>" + word + "</option>");
                    return false;
                });
            }, "Yes", null, "No");
        return false;
    },

    searchYodleeWordsDeleteClicked: function () {
        var word = this.$el.find("#yodleeSearchWordsDdl option:selected").text();
        var that = this;
        if (!word) return false;
        EzBob.ShowMessage(
            "", "Are you sure you want to remove"+ word +"?",
            function () {
                BlockUi('on');
                $.post(window.gRootPath + "Underwriter/MarketPlaces/DeleteSearchWord", { word: word })
                .done(function () {
                    EzBob.ShowMessage("Successfully Removed", "The word deleted successfully. ", null, "OK");
                })
                .done(function () {
                    BlockUi('off');
                    $("#yodleeSearchWordsDdl option:selected").remove();
                    return false;
                });
            }, "Yes", null, "No");
        return false;
    }
});