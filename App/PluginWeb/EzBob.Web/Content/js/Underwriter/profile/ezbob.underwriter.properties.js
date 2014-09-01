var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.Properties = Backbone.Model.extend({
    idAttribute: 'Id',
    url: function () {
        if (this.customerId != undefined) {
            return window.gRootPath + "Underwriter/Properties/Index/" + this.customerId;
        }

        return window.gRootPath + "Underwriter/Properties/Index/" + this.id;
    }
});

EzBob.Underwriter.PropertiesView = Backbone.Marionette.ItemView.extend({
	template: '#propertiesTemplate',
	initialize: function () {
		this.model.on("reset change sync", this.render, this);
	},
	onRender: function () {
		var that = this;
		$(document).ready(function () {
			$('#ownedPropertiesTable').dataTable({
				sDom: 't'
			});

			that.$el.find('.copy-buttons').one('mouseover', function () {
				that.$el.find(".btn-copy").each(function () {
					var element = $(this);

					if ((/[^\s,]/g).test(element.data('address'))) {
						element.zclip({
							path: window.gRootPath + "Content/flash/ZeroClipboard.swf",
							copy: function () {
								return element.data('address');
							}
						});
					} else
						element.addClass("disabled");
				});
			});
		});
	},
	serializeData: function () {
		return { model: this.model.toJSON() };
	},
	events: {
		"click .zooplaRecheck": "recheckZoopla",
		"click .btnEnquiry": "showLandRegistry",
		"click .btnNoLongerOwned": "markAsNoLongerOwned"
	},
	recheckZoopla: function () {
		BlockUi("On");
		var that = this;
		var xhr = $.get(window.gRootPath + "Underwriter/Properties/Zoopla/?customerId=" + this.customerId + "&recheck=true");
		xhr.done(function () {
			that.render(that.model);
		});
		xhr.always(function () {
			BlockUi("Off");
		});
	},
	showLandRegistry: function (el) {
		var address = $(el.currentTarget).data('address');
		var postcode = $(el.currentTarget).data('postcode');
		var addressId = $(el.currentTarget).data('addressid');

		var titles = null;
		if (addressId) {
			var properties = this.model.get('Properties');
			var prop = _.find(properties, function (currentProperty) { return currentProperty.AddressId == addressId; });
			titles = _.flatten(_.map(prop.LandRegistryEnquiries, function (enq) { return enq.Titles; }));
		}

		this.lrEnqView = new EzBob.LandRegistryEnquiryView({ model: { postcode: postcode, address: address, customerId: this.customerId, titles: titles } });
		EzBob.App.vent.on('landregistry:retrieved', this.landRegistryRetrieved, this);
		EzBob.App.jqmodal.show(this.lrEnqView);
	},
	landRegistryRetrieved: function () {
		BlockUi("Off");
		Backbone.history.loadUrl();
		return false;
	},
	markAsNoLongerOwned: function (el) {
	    var addressId = $(el.currentTarget).data('addressid');
	    var that = this;
	    
	    EzBob.ShowMessage(
			"Do you really want to remove this owned address?",
			"Please confirm",
			function () {
			    var xhr = $.post("" + window.gRootPath + "Underwriter/Properties/RemoveAddress?addressId=" + addressId);
			    xhr.done(function () {
			        EzBob.ShowMessageTimeout('Address was removed', 'Success');
			        that.model.fetch();
			    });
			},
			"Remove",
			null,
			"Cancel"
		);
	},
});

