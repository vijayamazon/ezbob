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
	initialize: function() {
		this.model.on("reset change sync", this.render, this);
	},
	onRender: function() {
		var that = this;

		$(document).ready(function() {
			$('#ownedPropertiesTable').dataTable({
				sDom: 't'
			});

			that.$el.find('.copy-buttons').one('mouseover', function() {
				that.$el.find(".btn-copy").each(function() {
					var element = $(this);

					if ((/[^\s,]/g).test(element.data('address'))) {
						element.zclip({
							path: window.gRootPath + "Content/flash/ZeroClipboard.swf",
							copy: function() {
								return element.data('address');
							}
						});
					} else
						element.addClass("disabled");
				});
			});
		});
	},
	serializeData: function() {
		return { model: this.model.toJSON() };
	},
	events: {
		"click .zooplaRecheck": "recheckZoopla",
		"click .btnEnquiry": "showLandRegistry",
		"click .btnNoLongerOwned": "markAsNoLongerOwned",
		"click #addOwnedAddress": "addOwnedAddress",
		'click .accordion-toggle': "accordionClicked",
		'click .btn-street-view-static': 'staticStreetViewClicked'
	},
	addOwnedAddress: function () {
	    var popUp = new EzBob.Popup({
	        postcode: $.trim($('#addOwnedAddressPostcode').val()),
	        uiEventControlIdPrefix: 'underwriter-add-property',
	        callback: this.addAddressCallback,
	        caller: this
	    });
	    
	    popUp.render();
	    popUp.SearchByPostcode();
	},
	addAddressCallback: function (address, caller) {
	    var that = this;
	    this.addingAddress = address;
	    address.fetch().done(function () {
	        var xhr = $.post(window.gRootPath + "Underwriter/Properties/AddAddress?customerId=" + caller.model.id
			    + "&addressId=" + that.addingAddress.get('Id')
			    + "&organisation=" + that.addingAddress.get('Organisation')
			    + "&line1=" + that.addingAddress.get('Line1')
			    + "&line2=" + that.addingAddress.get('Line2')
			    + "&line3=" + that.addingAddress.get('Line3')
			    + "&town=" + that.addingAddress.get('Town')
			    + "&county=" + that.addingAddress.get('County')
			    + "&postcode=" + that.addingAddress.get('Postcode')
			    + "&country=" + that.addingAddress.get('Country')
			    + "&rawpostcode=" + that.addingAddress.get('Rawpostcode')
			    + "&deliverypointsuffix=" + that.addingAddress.get('Deliverypointsuffix')
			    + "&nohouseholds=" + that.addingAddress.get('Nohouseholds')
			    + "&smallorg=" + that.addingAddress.get('Smallorg')
			    + "&pobox=" + that.addingAddress.get('Pobox')
			    + "&mailsortcode=" + that.addingAddress.get('Mailsortcode')
			    + "&udprn=" + that.addingAddress.get('Udprn')
			);
	        xhr.done(function () {
	            caller.model.fetch();
	        });
	    });
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
	staticStreetViewClicked: function() {
		this.$el.find('.street-view-static').toggleClass('hide');
	},
	accordionClicked: function (el) {
		if (!$(el.currentTarget).hasClass('collapsed') && $(el.currentTarget).attr('href').indexOf('zoopla') > -1) {
			this.googleStreetView();
			this.$el.find('img.zoppla-graph').each(function () {
				$(this).attr('src', $(this).attr('data-src'));
			});
		}
	},

	googleStreetView: function () {
		$('div.street-view').each(function (i, el) {
			
			var $el = $(this);
			var geocoder = new google.maps.Geocoder();
			geocoder.geocode({ address: $el.data('address') },
				function (results, status) {
					if (status == 'OK') {
						var latlng = results[0].geometry.location;
						var addr = new google.maps.LatLng(latlng.lat(), latlng.lng());
						var mapOptions = { center: addr, zoom: 14 };
						var map = new google.maps.Map(el, mapOptions);
						var panoramaOptions = {
							position: addr,
							pov: {
								heading: 34,
								pitch: 10
							}
						};
						var panorama = new google.maps.StreetViewPanorama(el, panoramaOptions);
						map.setStreetView(panorama);
					}
				}
			);
		});
	},
});

