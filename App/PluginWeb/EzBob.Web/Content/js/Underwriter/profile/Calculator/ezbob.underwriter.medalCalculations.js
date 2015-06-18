var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.MedalCalculationModel = Backbone.Model.extend({
    idAttribute: "Id",
    urlRoot: window.gRootPath + "Underwriter/Medal/Index/"
});

EzBob.Underwriter.MedalCalculationView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#medal-calculation-template').html());
        this.model.on('change reset sync', this.render, this);
        this.currentMedalId = 0;
    },

    events: {
        'click .export-to-exel': 'exportExcel',
        'change #MedalDetailsHistory': 'changeHistory'
    },

    changeHistory: function(el) {
        this.currentMedalId = $(el.currentTarget).val();
        this.render();
    },
    
    exportExcel: function () {
        location.href = window.gRootPath + 'Underwriter/Medal/ExportToExel?id=' + this.model.get('Id');
    },
    
    render: function() {
		
    	var medals = this.model.toJSON();
	    if (!medals.DetailedHistory)
	    	return false;

        var that = this;
        var currentMedal = _.find(medals.DetailedHistory.MedalDetailsHistories, function(medalDetails) {
             return medalDetails.Score.Id == that.currentMedalId;
        });
        
        this.$el.html(this.template({
            medals: medals,
            currentMedal: currentMedal,
            currentMedalId: this.currentMedalId
        }));

        EzBob.handleUserLayoutSetting();
        return this;
    }
});


