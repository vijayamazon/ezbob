﻿var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.CustomerRelationsModel = Backbone.Model.extend({
    url: function () {
        return window.gRootPath + "Underwriter/CustomerRelations/Index/" + this.customerId;
    }
});

EzBob.Underwriter.CustomerRelationsView = Backbone.Marionette.ItemView.extend({
    template: '#customerRelationsTemplate',
    initialize: function() {
        this.model.on("change reset sync", this.render, this);
    },
    serializeData: function () {
        return {
            vals: this.model.get("CustomerRelations"), 
            ranks: EzBob.CrmRanks,
            followUps: this.model.get("FollowUps"),
            lastFollowUp: this.model.get("LastFollowUp"),
            lastStatus: this.model.get("LastStatus")
        };
    },
    events: {
        "click .addNewCustomerRelationsEntry": "addNewCustomerRelationsEntry",
        "click .addFollowUp": "addFollowUp",
        "click .toggleSystemCrm": "toggleSystmeCrm",
        "change #Rank": "changeRank",
        "click #closeFollowUp": "closeFollowUp",
    },
    ui: {
        "toggleBtnIcon": ".toggleSystemCrm i",
        "toggleSystemBtn": ".toggleSystemCrm",
        "systemRows": ".crm-system",
        "closeFollowUpBtn": "#closeFollowUp",
        "rank": "#Rank",
        "followUp": 'label.followUp'
    },
    addNewCustomerRelationsEntry: function() {
        var view = new EzBob.Underwriter.AddCustomerRelationsEntry({ model: this.model });
        EzBob.App.jqmodal.show(view);
        return false;
    },

    addFollowUp: function() {
        var view = new EzBob.Underwriter.AddCustomerRelationsFollowUp({ model: this.model });
        EzBob.App.jqmodal.show(view);
        return false;
    },

    toggleSystmeCrm: function() {
        this.ui.toggleBtnIcon.toggleClass('fa-search-plus fa-search-minus');
        this.ui.systemRows.toggle("fast");
    },

    onRender: function() {
        var curRank = this.model.get('CurrentRank');
        if (curRank) {
            this.ui.rank.val(curRank.Id).blur();
        }
        
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
    
    closeFollowUp: function (event, state) {
        this.postChange(window.gRootPath + "Underwriter/CustomerRelations/CloseFollowUp", { customerId: this.model.customerId });
    },
    
    postChange: function(url, params) {
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

