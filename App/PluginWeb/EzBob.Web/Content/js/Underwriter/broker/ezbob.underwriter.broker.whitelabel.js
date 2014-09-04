var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.BrokerWhiteLabelModel = Backbone.Model.extend({
	idAttribute: "Id",
	url: function() {
		return window.gRootPath + "Underwriter/Brokers/LoadWhiteLabel/?nBrokerID=" + this.get("brokerID");
	},
});

EzBob.Underwriter.BrokerWhiteLabelView = EzBob.ItemView.extend({
	template: "#broker-white-label-template",
	initialize: function(options) {
		this.brokerID = options.brokerID;
		this.model.on("change reset", this.render, this);
		this.model.fetch();
	}, 

	events: {
		
	}, 

	serializeData: function() {
		return {
			whiteLabel: this.model.toJSON()
		};
	},

	onRender: function() {
	
	}, 

}); 
