var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.SignatureMonitorView = Backbone.View.extend({
	initialize: function() {
		this.theList = null;
	}, // initialize

	events: {
		'click .toggle-signers': 'toggleSigners',
	}, // events

	toggleSigners: function() {
		console.log(event.target);

		var nSignatureID = $(event.target).data('signature-id');

		this.$el.find('.signature' + nSignatureID).toggleClass('hide');
	}, // toggleSigners

	reload: function(nCustomerID) {
		if (this.theList) {
			this.theList.fnClearTable();
			this.theList = null;
		} // if

		console.log('reload for customer', nCustomerID);

		var self = this;

		$.getJSON(window.gRootPath + 'Underwriter/Esignatures/Load', { nCustomerID: nCustomerID }).done(function(oResponse) {
			oResponse.aaData.sort(function(a, b) {
				console.log(a, b);
				var oA = moment.utc(a.SendDate);
				var oB = moment.utc(b.SendDate);
				console.log(oB.diff(oA, 'seconds'));
				return oB.diff(oA, 'seconds');
			});

			var data = [];

			for (var i = 0; i < oResponse.aaData.length; i++) {
				var oItem = oResponse.aaData[i];

				data.push({
					ID: oItem.ID,
					SignatureID: oItem.ID,
					Name: oItem.TemplateName,
					Date: oItem.SendDate,
					Status: oItem.Status,
					HasDocument: oItem.HasDocument,
					Type: 'signature',
				});

				for (var j = 0; j < oItem.Signers.length; j++) {
					var oSigner = oItem.Signers[j];

					data.push({
						ID: oSigner.ID,
						SignatureID: oItem.ID,
						Name: $.trim(oSigner.FirstName) + ' ' + $.trim(oSigner.LastName) + ', ' + oSigner.Email,
						Date: oSigner.SignDate,
						Status: oSigner.Status,
						HasDocument: false,
						Type: 'signer',
					});
				} // for j
			} // for i

			var opts = {
				bDestroy: true,
				bProcessing: true,
				aoColumns: EzBob.DataTables.Helper.extractColumns('ID,Name,@Date,Status,HasDocument'),

				aaData: data,

				aaSorting: [],
				bSort: false,

				aLengthMenu: [[-1], ['all']],
				iDisplayLength: -1,

				sPaginationType: 'bootstrap',
				bJQueryUI: false,

				bAutoWidth: true,
				sDom: '<"top"<"box"<"box-title"<"dataTables_top_right"f><"dataTables_top_left"i>>>>tr<"clear">',

				fnRowCallback: function(oTR, oData, iDisplayIndex, iDisplayIndexFull) {
					console.log(oTR, oData);

					var oRow = $(oTR);

					if (oData.Type === 'signer')
						oRow.addClass('hide signature' + oData.SignatureID);

					if (oData.Type === 'signature') {
						var oBtn = $('<button title="Toggle signers display" />')
							.addClass('toggle-signers')
							.data('signature-id', oData.SignatureID)
							.text('*');

						oRow.find('.grid-item-Name').empty().append(oBtn).append(oData.Name);
					} // if

					if (oData.HasDocument) {
						var sLink = window.gRootPath + 'Underwriter/Esignatures/Download?nEsignatureID=' + oData.SignatureID;
						oRow.find('.grid-item-HasDocument').html('<a href="' + sLink + '" target=_blank title="Downloand the document.">Download</a>');
					}
					else
						oRow.find('.grid-item-HasDocument').empty();
				}, // fnRowCallback
			};

			console.log('opts', opts);

			self.theList = self.$el.find('#esignature-list').dataTable(opts);
		}); // on getJSON done
	}, // reload
}); // EzBob.Underwriter.SignatureMonitorView
