var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.AffordabilityView = Backbone.Marionette.ItemView.extend({
	template: '#affordability-template',
    initialize: function () {
    	this.bindTo(this.model, 'change sync', this.render, this);
	    BlockUi('on', this.$el);
        return this;
    },
    serializeData: function () {
        return {
	    	affordability: this.model.toJSON()
        };
    },
    onRender: function() {
    	if (this.model.customerId) {
    		this.rotateTable();
    	}
    	BlockUi('off', this.$el);
    },
    rotateTable: function () {
        this.$el.find('#affordabilityTable').each(function () {
            var $this = $(this);
            var newrows = [];
            $this.find('tr').each(function () {
                var i = 0;
                $(this).find('td').each(function () {
                    i++;
                    if (newrows[i] === undefined) { newrows[i] = $('<tr></tr>'); }
                    newrows[i].append($(this));
                });
            });
            $this.find("tr").remove();
            $.each(newrows, function () {
                $this.append(this);
            });
        });

        this.$el.find('#affordabilityTable tr:first-child td').each(function () {
            $(this).replaceWith('<th>' + $(this).html() + '</th>');
        });

        $($('#affordabilityTable tr')[2]).addClass('green-row');
	    var that = this;
        
	    if (this.model.customerId) {
		    _.each(this.model.attributes, function(aford, i) {
				if (_.isFunction(aford)) {
				    return;
			    }
		    	var trendSorted = _.sortBy(aford.TurnoverTrend, function(turnover) {
				    return moment.utc(turnover.TheMonth);
		    	});
		    	
		    	var trendTurnover = _.pluck(trendSorted, 'Turnover').join(',');
		    	
		    	that.$el.find(".affordability-trend-" + i).attr('values', trendTurnover);
			    that.$el.find(".affordability-trend-" + i).sparkline("html", {
				    width: '100px',
				    lineWidth: 1,
				    spotRadius: 2,
				    lineColor: "#cfcfcf",
				    fillColor: "transparent",
				    spotColor: "#cfcfcf",
				    maxSpotColor: "#cfcfcf",
				    minSpotColor: "#cfcfcf",
				    valueSpots: {
					    ':': '#cfcfcf'
				    }
			    });
		    });
	    }
    }
});
