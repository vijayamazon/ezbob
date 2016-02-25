var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ManageInvestorsModel = Backbone.Model.extend({
    url: '' + gRootPath + 'Underwriter/Investor/GetInvestors'
});


EzBob.Underwriter.ManageInvestorsView = Backbone.Marionette.ItemView.extend({
    template: "#investors-list-template",
    initialize: function() {
        this.model = new EzBob.Underwriter.ManageInvestorsModel();
        this.model.on("change reset", this.render, this);
        this.includeNonActiveInvestors = true;

        return this;
    }, //initialize
    events: {
        'click .manage-investor-row': 'manageInvestorClick',
        'click .edit-investor-details': 'EditInvestorDetails'

    }, //events
    serializeData: function() {
        return {
            data: this.model.get("Investors") //take the list from the model after fatch
        };
    }, //serializeData

    onRender: function() {
        if (this.currentInvestorID) {
            this.$el.find(".manage-investor-row[data-id='" + this.currentInvestorID + "']").click();
            this.$el.find(".manage-investor-row[data-id='" + this.currentInvestorID + "'] .edit-investor-details").click();
        }

    }, //onRender

    show: function() {
        var self = this;
      
        this.model.fetch().done(function() {
            self.render();
        });
        return this.$el.show();
    }, //show

    hide: function() {
        return this.$el.hide();
    }, //hide
    manageInvestorClick: function (el, preventClosingRow) {

        
        var tr = $(el.currentTarget).closest('tr');
        var id = tr.data('id');
        $('.manage-investor-row').removeClass("active");
        this.$el.find('.manage-investor-view').remove();
        this.$el.find('.edit-details-view-area').remove();
        if (this.view && this.view.$el.data('id') === id && !preventClosingRow) {
            this.details = null;
            this.view = null;
            $(tr).removeClass("active");
        } else {


            var newRow = $('<tr class="manage-investor-view"><td class="manage-investor-column" data-id="' + id + '"colspan="5"></td></tr>');

            tr.after(newRow);

            var rowel = this.$el.find('tr.manage-investor-view td');
            this.view = new EzBob.Underwriter.ManageInvestorView({ el: rowel, });
            this.view.show(id);
            $(tr).addClass("active");
        }
       
    }, //manageInvestorClick
    EditInvestorDetails: function(el) {
        
        var tr = $(el.currentTarget).closest('tr');
        var id = tr.data('id');
        this.$el.find('.edit-details-view-area').remove();
        if (!this.view || this.view.$el.data('id') !== id) {
            tr.click();
            this.OpenInvestorDetails(tr, id);
        
        } else if (this.view && this.details && this.view.$el.data('id') === id) {
            this.details = null;
        } else {
            this.OpenInvestorDetails(tr, id);

        }
       
        return false;

    }, //EditInvestorDetails
    OpenInvestorDetails: function(tr, id) {
        var newRow = $('<tr class="edit-details-view-area"><td  colspan="5"></td></tr>');

        var toSave = this.model.get('Investors').find(function (item) {
            return Number(item['InvestorID']) === id;
        });
        $(tr).closest('tr').next('tr').before(newRow);
        var newRowEl = this.$el.find('tr.edit-details-view-area td');
        this.details = new EzBob.Underwriter.ManageInvestorDetails({
            el: newRowEl,
            model: new EzBob.Underwriter.InvestorModel(),
            InvestorID: id,
            Details: toSave
        });
       
        this.details.render();
        this.details.on('submitDetails', this.submitDetails, this);
    },
    submitDetails: function(investorID) {

        this.currentInvestorID = investorID;
       
        this.model.fetch();
        //
      
        return false;
    }
});





