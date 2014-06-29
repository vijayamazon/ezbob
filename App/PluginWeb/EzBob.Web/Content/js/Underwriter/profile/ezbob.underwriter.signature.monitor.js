﻿var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.SignatureMonitorView = Backbone.View.extend({
	initialize: function() {
		this.SignaturesList = null;
		this.SignersList = null;
	}, // initialize

	events: {
		'click .toggle-signers': 'toggleSigners',
		'click .esign-send-another': 'sendAnother',
		'click .esign-poll-status': 'pollStatus',
		'click .cancel-send': 'cancelSend',
		'click .do-send': 'doSend',
		'click .download-signed-document': 'downloadSignedDocument',
	}, // events

	pollStatus: function() {
		var nCustomerID = this.$el.find('.esign-poll-status').data('CustomerID');
		this.reload(nCustomerID, true);
	}, // pollStatus

	doSend: function() {
		var oPackage = this.prepareSendPackage();

		if (!oPackage)
			return;

		BlockUi();

		var oRequest = $.post(window.gRootPath + 'Underwriter/Esignatures/Send', { sPackage: JSON.stringify(oPackage), });

		var self = this;

		oRequest.done(function(oResponse) {
			UnBlockUi();

			if (oResponse.success) {
				self.cancelSend();
				self.reload(self.$el.find('.do-send').data('CustomerID'));
				EzBob.ShowMessageTimeout('Documents have been sent.', 'Success', 2);
				return;
			} // if

			if (oResponse.error)
				EzBob.ShowMessage(oResponse.error, 'Error while sending');
			else
				EzBob.ShowMessage('Could not send documents for signature.', 'Error while sending');
		});

		oRequest.fail(function() {
			UnBlockUi();
			EzBob.ShowMessage('Failed to send documents for signature.', 'Error while sending');
		});
	}, // doSend

	prepareSendPackage: function() {
		var oPackage = {};
		var bHasDocuments = false;

		// ReSharper disable DuplicatingLocalDeclaration
		function echoSignEnvelope(nCustomerID, nTemplateID) {
			this.CustomerID = nCustomerID;
			this.Directors = [];
			this.TemplateID = nTemplateID;
			this.SendToCustomer = false;
		}
		// ReSharper restore DuplicatingLocalDeclaration

		echoSignEnvelope.prototype.IsReadyToSend = function() {
			if (this.CustomerID < 1)
				return false;

			if (this.TemplateID < 1)
				return false;

			if (!this.SendToCustomer && (this.Directors.length < 1))
				return false;

			for (var i; i < this.Directors.length; i++)
				if (this.Directors[i] < 1)
					return false;

			return true;
		};

		var nCustomerID = this.$el.find('.do-send').data('CustomerID');

		this.$el.find('.document-type').each(function() {
			var oChk = $(this);

			if (!oChk.attr('checked'))
				return;

			bHasDocuments = true;

			var nTemplateID = parseInt(oChk.data('template-id'), 10);

			oPackage[nTemplateID] = new echoSignEnvelope(nCustomerID, nTemplateID);
		});

		if (!bHasDocuments) {
			EzBob.ShowMessage('No documents to sign selected.', 'Cannot send');
			return null;
		} // if

		this.$el.find('.selected-signer').each(function() {
			var oChk = $(this);

			if (!oChk.attr('checked'))
				return;

			var oSigner = oChk.data();

			if (oSigner.IsDirector && oPackage[1]) {
				if (oSigner.DirectorID)
					oPackage[1].Directors.push(oSigner.DirectorID);
				else
					oPackage[1].SendToCustomer = true;
			} // if

			if (oSigner.IsShareholder && oPackage[2]) {
				if (oSigner.DirectorID)
					oPackage[2].Directors.push(oSigner.DirectorID);
				else
					oPackage[2].SendToCustomer = true;
			} // if
		});

		oPackage = _.filter(oPackage, function(e) { return e.IsReadyToSend(); });

		if (!oPackage.length) {
			EzBob.ShowMessage('No signer specified.', 'Cannot send');
			return null;
		} // if

		return oPackage;
	}, // prepareSendPackage

	sendAnother: function() {
		this.$el.find('#esigners-list-container').removeClass('hide');
		this.$el.find('#esignature-list-container').addClass('hide');
		this.$el.find('.initially-checked').attr('checked', 'checked');
	}, // sendAnother

	cancelSend: function() {
		this.$el.find('#esigners-list-container').addClass('hide');
		this.$el.find('#esignature-list-container').removeClass('hide');
	}, // cancelSend

	toggleSigners: function() {
		var nSignatureID = $(event.target).data('SignatureID');
		this.$el.find('.signature' + nSignatureID).toggleClass('hide');
	}, // toggleSigners

	reload: function(nCustomerID, bPollStatus) {
		if (!EzBob.Config.EchoSignEnabledUnderwriter) {
			this.$el.hide();
			return;
		} // if

		bPollStatus = bPollStatus ? true : false;

		if (this.SignaturesList) {
			this.SignaturesList.fnClearTable();
			this.SignaturesList = null;
		} // if

		if (this.SignersList) {
			this.SignersList.fnClearTable();
			this.SignersList = null;
		} // if

		var self = this;

		if (bPollStatus)
			BlockUi();

		var oRequest = $.getJSON(window.gRootPath + 'Underwriter/Esignatures/Load', { nCustomerID: nCustomerID, bPollStatus: bPollStatus, });
		
		oRequest.done(function(oResponse) {
			var oSignatures = self.prepareSignatures(oResponse.signatures);

			var oSignatureListOpts = self.getDataTableOpts(oSignatures, 'ID,Name,@Date,Status,HasDocument');
			oSignatureListOpts.fnRowCallback = _.bind(self.signatureListRowCallback, self);

			self.SignaturesList = self.$el.find('#esignature-list').dataTable(oSignatureListOpts);
			self.$el.find('#esignature-list_wrapper .dataTables_top_left')
				.html('Sent documents ')
				.append(self.fromTemplate('.esign-send-another'))
				.append(self.fromTemplate('.esign-poll-status').data('CustomerID', nCustomerID));

			var oSignersListOpts = self.getDataTableOpts(oResponse.signers, 'FirstName,LastName,Email');
			oSignersListOpts.fnRowCallback = _.bind(self.signersListRowCallback, self);
			oSignersListOpts.aoColumns.push.apply(oSignersListOpts.aoColumns, [
				{ sClass: ['grid-item-IsDirector center'], mData: null, },
				{ sClass: ['grid-item-IsShareholder center'], mData: null, },
				{ sClass: ['grid-item-IsSelected center'], mData: null, },
			]);

			self.SignersList = self.$el.find('#esigners-list').dataTable(oSignersListOpts);
			self.$el.find('#esigners-list_wrapper .dataTables_top_left')
				.html('Send documents to');
		}); // on getJSON done

		oRequest.always(function() {
			if (bPollStatus)
				UnBlockUi();
		});
	}, // reload

	fromTemplate: function(sSelector) {
		return this.$el.find('.templates').find(sSelector).clone(true);
	}, // fromTemplate

	signersListRowCallback: function(oTR, oData, iDisplayIndex, iDisplayIndexFull) {
		var oRow = $(oTR);

		this.$el.find('.do-send').data('CustomerID', oData.CustomerID);

		if (oData.IsDirector)
			oRow.find('.grid-item-IsDirector').empty().append($('<i class="fa fa-check-square-o"/>'));

		if (oData.IsShareholder)
			oRow.find('.grid-item-IsShareholder').empty().append($('<i class="fa fa-check-square-o"/>'));

		var oSelected = $('<input type=checkbox class="initially-checked selected-signer">').data({
			DirectorID: oData.DirectorID,
			IsDirector: oData.IsDirector,
			IsShareholder: oData.IsShareholder,
		});

		oRow.find('.grid-item-IsSelected').empty().append(oSelected);
	}, // signersListRowCallback

	downloadSignedDocument: function() {
		var nSignatureID = $(event.target).data('SignatureID');
		window.open(window.gRootPath + 'Underwriter/Esignatures/Download?nEsignatureID=' + nSignatureID);
	}, // downloadSignedDocument

	signatureListRowCallback: function(oTR, oData, iDisplayIndex, iDisplayIndexFull) {
		var oRow = $(oTR);

		if (oData.Type === 'signer')
			oRow.addClass('hide signature' + oData.SignatureID);

		var oIDCell = oRow.find('.grid-item-ID').empty();

		if (oData.Type === 'signature')
			oIDCell.append(this.fromTemplate('.toggle-signers').data('SignatureID', oData.SignatureID));

		var oDownloadCell = oRow.find('.grid-item-HasDocument').empty();

		if (oData.HasDocument)
			oDownloadCell.append(this.fromTemplate('.download-signed-document').data('SignatureID', oData.SignatureID));
	}, // signatureListRowCallback

	prepareSignatures: function(oRawSignatures) {
		oRawSignatures.sort(function(a, b) {
			var oA = moment.utc(a.SendDate);
			var oB = moment.utc(b.SendDate);
			return oB.diff(oA, 'seconds');
		});

		var oSignatures = [];

		for (var i = 0; i < oRawSignatures.length; i++) {
			var oItem = oRawSignatures[i];

			oSignatures.push({
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

				oSignatures.push({
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

		return oSignatures;
	}, // prepareSignatures

	getDataTableOpts: function(aaData, sFieldNames) {
		return {
			bDestroy: true,
			bProcessing: true,
			aoColumns: EzBob.DataTables.Helper.extractColumns(sFieldNames),

			aaData: aaData,

			aaSorting: [],
			bSort: false,

			aLengthMenu: [[-1], ['all']],
			iDisplayLength: -1,

			sPaginationType: 'bootstrap',
			bJQueryUI: false,

			bAutoWidth: true,
			sDom: '<"top"<"box"<"box-title"<"dataTables_top_right"f><"dataTables_top_left">>>>tr<"clear">',
		};
	}, // getDataTableOpts
}); // EzBob.Underwriter.SignatureMonitorView
