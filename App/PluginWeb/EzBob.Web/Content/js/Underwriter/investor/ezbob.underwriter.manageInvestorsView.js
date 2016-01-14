var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ManageInvestorsModel = Backbone.Model.extend({
    url: '' + gRootPath + 'Underwriter/Investor/GetInvestors'
});


EzBob.Underwriter.ManageInvestorsView = Backbone.Marionette.ItemView.extend({
    template: "#investors-list-template",
    initialize: function () {
        this.model = new EzBob.Underwriter.ManageInvestorsModel();
        this.model.on("change reset", this.render, this);
        this.includeNonActiveInvestors = true;

        return this;
    },
    events: {
        'click .manage-investor-row': 'manageInvestorClick'
     
    },
    serializeData: function () {
        return {
            data: this.model.get("Investors") //take the list from the model after fatch
            };
    },
    displayInvestorsData: function () {
        this.model.fetch();

    },
    onRender: function() {
        this.displayInvestorsData();
    },
    

    show: function () {

        return this.$el.show();
    },

    hide: function () {
        return this.$el.hide();
    },
    manageInvestorClick: function (el) {
        var tr = $(el.currentTarget).closest('tr');
        var id = tr.data('id');
        this.$el.find('.manage-investor-view').remove();
        console.log(id);

        var newRow = $('<tr class="manage-investor-view"><td colspan="6"></td></tr>');
        tr.after(newRow);
        var newRowEl = this.$el.find('tr.manage-investor-view td');
        var investorView = new EzBob.Underwriter.ManageInvestorView({ el: newRowEl, });
        investorView.show(id);

    }

});





