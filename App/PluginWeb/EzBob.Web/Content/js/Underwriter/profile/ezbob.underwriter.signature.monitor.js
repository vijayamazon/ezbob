var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.SignatureMonitorView = Backbone.View.extend({
	initialize: function() {
		this.SignaturesList = null;
		this.SignersList = null;

		this.personalInfoModel = this.options.personalInfoModel;

		this.boardResolutionTemplateID = 1;
		this.personalGuaranteeTemplateID = 2;
	}, // initialize

	events: {
		'click .toggle-signers': 'toggleShowSigners',
		'click .esign-send-another': 'sendAnother',
		'click .esign-poll-status': 'pollStatus',
		'click .cancel-send': 'cancelSend',
		'click .do-send': 'doSend',
		'click .download-signed-document': 'downloadSignedDocument',
		'click .add-director': 'addDirectorClicked',
		'click .toggle-all-signers': 'toggleAllSigners',
		'click .btn-edit-director': 'startEditDirector',
		'click .btn-delete-director': 'deleteDirector',
	}, // events

	deleteDirector: function(event) {
		var oRow = $(event.target).closest('TR.experian-director');

		if (oRow.length !== 1)
			return;

		var self = this;

		var doDeleteDirector = function() {
			BlockUi('on', this.$el);

			var oData = oRow.data('for-edit');

			var oRequest = $.post(
				window.gRootPath + 'Underwriter/Esignatures/DeleteExperianDirector',
				{ nDirectorID: oData.directorID, }
			);

			oRequest.done(function(oResponse) {
				if (oResponse.success) {
					EzBob.App.trigger('clear');
					self.reloadCurrent();
					return;
				} // if

				if (oResponse.error)
					EzBob.App.trigger('error', oResponse.error);
				else
					EzBob.App.trigger('error', 'Error deleting director.');
			});

			oRequest.fail(function() {
				EzBob.App.trigger('error', 'Failed to delete director.');
			});

			oRequest.always(function() {
				BlockUi('off', self.$el);
			});
		}; // doDeleteDirector

		var sTitle = $.trim(
			$.trim(oRow.find('.grid-item-FirstName').text()) + ' ' +
			$.trim(oRow.find('.grid-item-LastName').text())
		);

		EzBob.ShowMessage('Confirm deleting director', sTitle, doDeleteDirector, 'Delete', null, 'Keep');
	}, // deleteDirector

	startEditDirector: function(event) {
		var oRow = $(event.target).closest('TR.experian-director');

		if (oRow.length !== 1)
			return;

		var oView = new EzBob.EditExperianDirectorView({
			data: oRow.data('for-edit'),

			saveUrl: window.gRootPath + 'Underwriter/Esignatures/SaveExperianDirector',

			row: oRow,

			editBtn: oRow.find('.edit-and-delete'),
			saveBtn: oRow.find('.btn-save-director'),
			cancelBtn: oRow.find('.btn-cancel-edit'),

			emailCell: oRow.find('.grid-item-Email'),
			mobilePhoneCell: oRow.find('.grid-item-MobilePhone'),
			addressCell: oRow.find('.grid-item-Address'),
		});

		oView.render();
	}, // startEditDirector

	toggleAllSigners: function() {
		var oChk = this.$el.find('.toggle-all-signers');

		var bChecked = !(oChk.data('checked') ? true : false);

		oChk.data('checked', bChecked);

		if (bChecked)
			this.$el.find('.selected-signer').attr('checked', 'checked');
		else
			this.$el.find('.selected-signer').removeAttr('checked');
	}, // toggleAllSigners

	addDirectorClicked: function(event) {
		event.stopPropagation();
		event.preventDefault();

		this.$el.find('.add-director').hide();

		var customerInfo = {
			FirstName: this.personalInfoModel.get('FirstName'),
			Surname: this.personalInfoModel.get('Surname'),
			DateOfBirth: this.personalInfoModel.get('DateOfBirth'),
			Gender: this.personalInfoModel.get('Gender'),
			PostCode: this.personalInfoModel.get('PostCode'),
			Directors: this.personalInfoModel.get('Directors')
		};

		var directorEl = this.$el.find('.add-director-container');

		var addDirectorView = new EzBob.AddDirectorInfoView({
			model: new EzBob.DirectorModel(),
			el: directorEl,
			backButtonCaption: 'Cancel',
			failOnDuplicate: false,
			customerInfo: customerInfo,
		});

		var nCustomerID = this.personalInfoModel.get('Id');

		var self = this;

		addDirectorView.setBackHandler(function() {
			return self.onDirectorAddCanceled();
		});

		addDirectorView.setSuccessHandler(function() {
			return self.onDirectorAdded(nCustomerID);
		});

		addDirectorView.setDupCheckCompleteHandler(function(bDupFound) {
			return self.onDuplicateCheckComplete(bDupFound);
		});

		addDirectorView.render();

		addDirectorView.setCustomerID(nCustomerID);

		directorEl.show();

		return false;
	}, // addDirectorClicked

	onDirectorAddCanceled: function() {
		this.$el.find('.add-director-container').hide().empty();
		this.$el.find('.add-director').show();
	}, // onDirectorAddCanceled

	onDirectorAdded: function(nCustomerID) {
		this.onDirectorAddCanceled();
		this.reload(nCustomerID);
	}, // onDirectorAdded

	onDuplicateCheckComplete: function(bDupFound) {
		if (bDupFound)
			this.$el.find('.duplicate-director-detected').show();	
		else
			this.$el.find('.duplicate-director-detected').hide();
	}, // onDuplicateCheckComplete

	pollStatus: function() {
		var nCustomerID = this.$el.find('.esign-poll-status').data('CustomerID');
		this.reload(nCustomerID, true);
	}, // pollStatus

	reloadCurrent: function() {
		this.reload(this.$el.find('.do-send').data('CustomerID'));
	}, // reloadCurrent

	doSend: function() {
		var oPackage = this.prepareSendPackage();

		if (!oPackage)
			return;

		BlockUi('on', this.$el);

		var oRequest = $.post(window.gRootPath + 'Underwriter/Esignatures/Send', { sPackage: JSON.stringify(oPackage), });

		var self = this;

		oRequest.done(function(oResponse) {
			if (oResponse.success) {
				self.cancelSend();
				self.reloadCurrent();
				EzBob.ShowMessageTimeout('Documents have been sent.', 'Success', 2);
				return;
			} // if

			if (oResponse.error)
				EzBob.ShowMessage(oResponse.error, 'Error while sending');
			else
				EzBob.ShowMessage('Could not send documents for signature.', 'Error while sending');
		});

		oRequest.fail(function() {
			EzBob.ShowMessage('Failed to send documents for signature.', 'Error while sending');
		});

		oRequest.always(function() {
			BlockUi('off', self.$el);
		});
	}, // doSend

	prepareSendPackage: function() {
		var oPackage = {};
		var bHasDocuments = false;

		// ReSharper disable DuplicatingLocalDeclaration
		function echoSignEnvelope(nCustomerID, nTemplateID) {
			this.CustomerID = nCustomerID;
			this.Directors = [];
			this.ExperianDirectors = [];
			this.TemplateID = nTemplateID;
			this.SendToCustomer = false;
		}
		// ReSharper restore DuplicatingLocalDeclaration

		echoSignEnvelope.prototype.IsReadyToSend = function() {
			if (this.CustomerID < 1)
				return false;

			if (this.TemplateID < 1)
				return false;

			if (!this.SendToCustomer && ((this.Directors.length < 1) && (this.ExperianDirectors.length < 1)))
				return false;

			var i;

			for (i = 0; i < this.Directors.length; i++)
				if (this.Directors[i] < 1)
					return false;

			for (i = 0; i < this.ExperianDirectors.length; i++)
				if (this.ExperianDirectors[i] < 1)
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

		var self = this;

		this.$el.find('.selected-signer').each(function() {
			var oChk = $(this);

			if (!oChk.attr('checked'))
				return;

			var oSigner = oChk.data();

			if (oSigner.IsDirector && oPackage[self.boardResolutionTemplateID]) {
				if (oSigner.DirectorID) {
					if (oSigner.Type === 'experian')
						oPackage[self.boardResolutionTemplateID].ExperianDirectors.push(oSigner.DirectorID);
					else
						oPackage[self.boardResolutionTemplateID].Directors.push(oSigner.DirectorID);
				}
				else
					oPackage[self.boardResolutionTemplateID].SendToCustomer = true;
			} // if

			if (oSigner.IsShareholder && oPackage[self.personalGuaranteeTemplateID]) {
				if (oSigner.DirectorID) {
					if (oSigner.Type === 'experian')
						oPackage[self.personalGuaranteeTemplateID].ExperianDirectors.push(oSigner.DirectorID);
					else
						oPackage[self.personalGuaranteeTemplateID].Directors.push(oSigner.DirectorID);
				} // if
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

	toggleShowSigners: function() {
		var nSignatureID = $(event.target).closest('button').data('SignatureID');
		this.$el.find('.signature' + nSignatureID).toggleClass('hide');
	}, // toggleShowSigners

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
			BlockUi('on', this.$el);

		var oRequest = $.getJSON(
			window.gRootPath + 'Underwriter/Esignatures/Load',
			{ nCustomerID: nCustomerID, bPollStatus: bPollStatus, }
		);
		
		oRequest.done(function(oResponse) {
			var oSignatures = self.prepareSignatures(oResponse.signatures);

			var oSignatureListOpts = self.getDataTableOpts(oSignatures, 'ID,Name,@Date,Status,HasDocument');
			oSignatureListOpts.fnRowCallback = _.bind(self.signatureListRowCallback, self);

			var oTbl = self.$el.find('#esignature-list');
			self.SignaturesList = oTbl.dataTable(oSignatureListOpts);
			self.$el.find('#esignature-list_wrapper .dataTables_top_left')
				.html('Sent documents ')
				.append(self.fromTemplate('.esign-send-another'))
				.append(self.fromTemplate('.esign-poll-status').data('CustomerID', nCustomerID));
			oTbl.css('width', '');

			var oSignersListOpts = self.getDataTableOpts(oResponse.signers, 'FirstName,LastName,Email,MobilePhone');
			oSignersListOpts.fnRowCallback = _.bind(self.signersListRowCallback, self);
			oSignersListOpts.aoColumns.unshift(
				{ sClass: 'grid-item-IsSelected center narrow-as-possible', mData: null, },
				{ sClass: 'grid-item-Source center narrow-as-possible', mData: 'Type', mRender: function(sType) {
					var sClass = 'fa-circle-o';
					var bHide = false;
					var sSource = '';

					switch (sType) {
					case 'customer':
						sClass = 'fa-user';
						sSource = 'This is the customer.';
						break;
					case 'director':
						sClass = 'fa-users';
						sSource = 'This director/shareholder has been entered by the customer/underwriter.';
						break;
					case 'experian':
						sClass = 'fa-institution';
						sSource = 'This director/shareholder has been extracted from Experian data.';
						break;
					default:
						bHide = true;
						break;
					} // sType

					var sHtml = '<i class="fa ' + sClass + '" title="' + sSource + '" ' +
						(bHide ? 'style="visibility: hidden;"' : '') + '></i>';

					return sHtml +
						'<i class="fa fa-legal IsDirector" title="Company director"></i>' +
						'<i class="fa fa-money IsShareholder" title="Company shareholder"></i>';
				}}
			);

			oSignersListOpts.aoColumns.push(
				{ sClass: 'grid-item-Address', mData: null, mRender: function(ignored, sType, oData) {
					return _.filter(
						[oData.Line1, oData.Line2, oData.Line3, oData.Town, oData.County, oData.Postcode],
						$.trim
					).join(', ');
				}},
				{ sClass: 'grid-item-Controls narrow-as-possible', mData: null, }
			);

			oTbl = self.$el.find('#esigners-list');
			self.SignersList = oTbl.dataTable(oSignersListOpts);
			self.$el.find('#esigners-list_wrapper .dataTables_top_left')
				.html('Send documents to');
			oTbl.css('width', '');

			self.$el.find('.initially-checked').attr('checked', 'checked');

			self.$el.find('.grid-item-Source').find('i').tooltip();
		}); // on getJSON done

		oRequest.always(function() {
			if (bPollStatus)
				BlockUi('off', self.$el);

			self.boardResolutionTemplateID = self.personalInfoModel.get('BoardResolutionTemplateID');
			self.personalGuaranteeTemplateID = self.personalInfoModel.get('PersonalGuaranteeTemplateID');

			self.$el.find('#esign-board-resolution').data('template-id', self.boardResolutionTemplateID);
			self.$el.find('#esign-personal-guarantee').data('template-id', self.personalGuaranteeTemplateID);

			self.$el.find('#add-director-from-esign').toggleClass(
				'hide',
				self.personalInfoModel.get('CompanyType') === 'Entrepreneur'
			);
		});
	}, // reload

	fromTemplate: function(sSelector) {
		return this.$el.find('.templates').find(sSelector).clone(true);
	}, // fromTemplate

	// ReSharper disable UnusedParameter
	signersListRowCallback: function(oTR, oData, iDisplayIndex, iDisplayIndexFull) {
		// ReSharper restore UnusedParameter
		var oRow = $(oTR);

		oRow.data('for-edit', new EzBob.EditExperianDirectorData({
			directorID: oData.DirectorID,
			email: oData.Email,
			mobilePhone: oData.MobilePhone,
			line1: oData.Line1,
			line2: oData.Line2,
			line3: oData.Line3,
			town: oData.Town,
			county: oData.County,
			postcode: oData.Postcode,
		}));

		this.$el.find('.do-send').data('CustomerID', oData.CustomerID);

		if (!oData.IsDirector)
			oRow.find('.grid-item-Source .IsDirector').css('visibility', 'hidden');

		if (!oData.IsShareholder || (oData.Type === 'customer'))
			oRow.find('.grid-item-Source .IsShareholder').css('visibility', 'hidden');

		var oSelected = $('<input type=checkbox class="initially-checked selected-signer">').data({
			DirectorID: oData.DirectorID,
			IsDirector: oData.IsDirector,
			IsShareholder: oData.IsShareholder,
			Type: oData.Type,
		});

		oRow.find('.grid-item-IsSelected').empty().append(oSelected);

		var oControls = oRow.find('.grid-item-Controls').empty();

		if (oData.Type === 'experian') {
			oControls
				.append($(
					'<div class=edit-and-delete>' +
					'<button class="btn btn-primary btn-edit-director"><i class="fa fa-edit"></i> Edit</button>' +
					'<button class="btn btn-primary btn-delete-director" title="Delete this director">' +
						'<i class="fa fa-times"></i>' +
					'</button>' +
					'</div>' +

					'<button class="btn btn-primary btn-save-director hide"><i class="fa fa-save"></i> Save</button>' +
					'<button class="btn btn-primary btn-cancel-edit hide"><i class="fa fa-undo"></i> Cancel</button>'
				));

			oRow.addClass('experian-director');
		} // if
	}, // signersListRowCallback

	downloadSignedDocument: function() {
		var nSignatureID = $(event.target).closest('button').data('SignatureID');
		window.open(window.gRootPath + 'Underwriter/Esignatures/Download?nEsignatureID=' + nSignatureID);
	}, // downloadSignedDocument

	// ReSharper disable UnusedParameter
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
	// ReSharper restore UnusedParameter

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
