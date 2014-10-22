var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.CustomerRelationsModel = Backbone.Model.extend({
    url: function () {
        return window.gRootPath + "Underwriter/CustomerRelations/Index/" + this.customerId;
    }
});

EzBob.Underwriter.CustomerRelationsView = Backbone.Marionette.ItemView.extend({
    template: '#customerRelationsTemplate',

    initialize: function () {
        this.model.on("change reset sync", this.render, this);
        this.isBroker = this.options.isBroker;
    },

    serializeData: function () {
        var actionItems = this.model.get("ActionItems");
        var checkedActionItemsCounter = 0;
        if (this.model.get("CostumeActionItem").IsChecked) {
            checkedActionItemsCounter = 1;
        }
        for (var i = 0; i < actionItems.length; i++) {
            var ai = actionItems[i];
            if (ai.IsChecked) {
                checkedActionItemsCounter++;
            }
        }
        
        return {
            vals: this.model.get("CustomerRelations"),
            ranks: EzBob.CrmRanks,
            followUps: this.model.get("FollowUps"),
            lastFollowUp: this.model.get("LastFollowUp"),
            lastStatus: this.model.get("LastStatus"),
            isPhoneVerified: this.model.get("IsPhoneVerified"),
            creditResult: this.model.get("CreditResult"),
            checkedActionItemsCounter: checkedActionItemsCounter
        };
    },

    events: {
        "click .addNewCustomerRelationsEntry": "addNewCustomerRelationsEntry",
        "click .addFollowUp": "addFollowUp",
        "click .markAsPending": "markAsPending",
        "click .toggleSystemCrm": "toggleSystmeCrm",
        "change #Rank": "changeRank",
        "click #closeFollowUp": "closeLastFollowUp",
        "click .btnCloseFollowUp": "closeFollowUp",
        "click #sendSms": "sendSms",
    },

    ui: {
        "toggleBtnIcon": ".toggleSystemCrm i",
        "toggleSystemBtn": ".toggleSystemCrm",
        "systemRows": ".crm-system",
        "closeFollowUpBtn": "#closeFollowUp",
        "rank": "#Rank",
        "followUp": 'label.followUp'
    },
    sendSms: function () {
        var view = new EzBob.Underwriter.SendSms({
            model: this.model,
            isBroker: this.isBroker,
        });
        EzBob.App.jqmodal.show(view);
        return false;
    },

    addNewCustomerRelationsEntry: function () {
        var view = new EzBob.Underwriter.AddCustomerRelationsEntry({
            model: this.model,
            url: window.gRootPath + 'Underwriter/CustomerRelations/SaveEntry/',
            isBroker: this.isBroker,
        });
        EzBob.App.jqmodal.show(view);
        return false;
    },

    addFollowUp: function () {
        var view = new EzBob.Underwriter.AddCustomerRelationsFollowUp({
            model: this.model,
            isBroker: this.isBroker,
        });
        EzBob.App.jqmodal.show(view);
        return false;
    },

    markAsPending: function () {
        var view = new EzBob.Underwriter.MarkAsPending({
            model: this.model,
            actionItemsSavedCallback: this.actionItemsSavedCallback
        });
        EzBob.App.jqmodal.show(view);
        return false;
    },
    
    actionItemsSavedCallback: function () {
        this.model.fetch();
    },

    toggleSystmeCrm: function () {
        this.ui.toggleBtnIcon.toggleClass('fa-search-plus fa-search-minus');
        this.ui.systemRows.toggle("fast");
    },

    onRender: function () {
        var curRank = this.model.get('CurrentRank');
        if (curRank)
            this.ui.rank.val(curRank.Id).blur();

        var isFollowed = _.some(this.model.get("FollowUps"), function (f) {
            return !f.IsClosed;
        });

        $('.crm-tab').toggleClass('followed-up', isFollowed);

        this.ui.toggleSystemBtn.tooltip({ placement: "right" });
        this.ui.followUp.tooltip({ placement: "bottom" });
    },

    changeRank: function () {
        if (!this.ui.rank.val()) {
            this.ui.rank.val(this.model.get('CurrentRank').Id).blur();
            return;
        }
        this.postChange(window.gRootPath + "Underwriter/CustomerRelations/ChangeRank", { customerId: this.model.customerId, rankId: this.ui.rank.val() });
    },

    closeLastFollowUp: function (event, state) {
        this.postChange(window.gRootPath + "Underwriter/CustomerRelations/CloseFollowUp", { customerId: this.model.customerId });
    },

    closeFollowUp: function (event, state) {
        this.postChange(window.gRootPath + "Underwriter/CustomerRelations/CloseFollowUp", { customerId: this.model.customerId, followUpId: $(event.currentTarget).data("id") });
    },

    postChange: function (url, params) {
        BlockUi("on");
        var that = this;
        var xhr = $.post(url, params);
        xhr.done(function (res) {
            if (res.error) {
                EzBob.ShowMessage(res.error, 'Error');
            }
            that.model.fetch();
        });

        xhr.always(function () {
            BlockUi("off");
        });

    }
});

