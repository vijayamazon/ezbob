var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.GridTools = {
	showMedalIcon: function(cellval) {
		var text;
		text = cellval.text || cellval;
		return '<i data-toggle=tooltip title="' + text + '" class="' + (text.toLowerCase().replace(/\s/g, '')) + '"></i>';
	}, // showMedalIcon

	profileLink: function(nCustomerID, sLinkText) {
		return EzBob.DataTables.Helper.withScrollbar('<button class="profileLink btn btn-link" title="Open customer profile" data-href="#profile/' + nCustomerID + '">' + (sLinkText || nCustomerID) + '</button>');
	}, // profileLink

	brokerProfileLink: function(nBrokerID, sLinkText) {
		return EzBob.DataTables.Helper.withScrollbar('<button class="profileLink btn btn-link" title="Open broker profile" data-href="#broker/' + nBrokerID + '">' + (sLinkText || nBrokerID) + '</button>');
	}, // brokerProfileLink
	investorManageLink: function (nInvestorID, sLinkText) {
		return EzBob.DataTables.Helper.withScrollbar('<button class="profileLink btn btn-link" title="Manage investor" data-href="#manageInvestor/' + nInvestorID + '">' + (sLinkText || nInvestorID) + '</button>');
	}, // investorManageLink

	investorButton: function(buttonName, buttonClass, customerID, cashRequestID, link) {
		return EzBob.DataTables.Helper.withScrollbar('<a href="#" class="btn btn-primary ' + buttonClass + '" data-id="' + customerID + '" data-crid="' + cashRequestID + '" title="' + buttonName + '" data-href="'+ link +'">' + buttonName + '</a>');
	}, // profileLink
}; // EzBob.Underwriter.GridTools

EzBob.Underwriter.GridsView = Backbone.View.extend({
	initialize: function(options) {
		this.router = null;

		this.loadLogbookEntryTypeList();

		function GridProperties(opts) {
			_.extend(this, opts);
		} // GridProperties

		GridProperties.prototype.AddToTitle = function() { return null; };

		GridProperties.prototype.OnAfterRender = function() { };

		GridProperties.prototype.fnRowCallback = function(oTR, oData, iDisplayIndex, iDisplayIndexFull) {
			if (oData.hasOwnProperty('Cart'))
				$('.grid-item-Cart', oTR).empty().html(EzBob.Underwriter.GridTools.showMedalIcon(oData.Cart));

			if (oData.hasOwnProperty('MP_List'))
				$('.grid-item-MP_List', oTR).empty().html(EzBob.DataTables.Helper.showNewMPsIcon(oData.MP_List, oData.SegmentType));

			if (oData.hasOwnProperty('Email'))
				$('.grid-item-Email', oTR).empty().html(EzBob.DataTables.Helper.showEmail(oData.Email));

			if (oData.hasOwnProperty('Broker'))
				$('.grid-item-Broker', oTR).empty().html(EzBob.DataTables.Helper.showBroker(oData.Broker, oData.FirstSale));

			if (oData.hasOwnProperty('Id')) {
				$('.grid-item-Id', oTR).empty().html(EzBob.Underwriter.GridTools.profileLink(oData.Id));

				if (oData.hasOwnProperty('Name')) {
					if (oData.Name)
						$('.grid-item-Name', oTR).empty().html(EzBob.Underwriter.GridTools.profileLink(oData.Id, oData.Name));
				} // if has name

				$(oTR).dblclick(function() {
					location.assign($(oTR).find('.profileLink').first().data('href'));
				});
			} // if has id

			if (oData.IsWasLate)
				$(oTR).addClass("table-flag-red");

			$(oTR).find('[data-toggle="tooltip"]').tooltip({ html: true, placement: "bottom" });
		}; // fnRowCallback

		this.gridProperties = {
			waiting: new GridProperties({
				icon: 'envelope-o',
				title: 'Waiting for decision',
				action: 'UwGridWaiting',
				columns: '#Id,Cart,MP_List,Name,Email,^ApplyDate,^RegDate,CustomerStatus,$CalcAmount,$OSBalance,LastStatus,Broker,CRMcomment,~SegmentType,~FirstSale',
			}), // waiting
			escalated: new GridProperties({
				icon: 'arrow-up',
				action: 'UwGridEscalated',
				columns: '#Id,Cart,MP_List,Name,Email,^ApplyDate,^RegDate,CurrentStatus,$CalcAmount,$OSBalance,^EscalationDate,Underwriter,Reason',
			}), // escalated
			pending: new GridProperties({
				icon: 'clock-o',
				action: 'UwGridPending',
				columns: '#Id,Cart,MP_List,Name,Email,^ApplyDate,^RegDate,CurrentStatus,$CalcAmount,$OSBalance,Pending,LastStatus,Broker,CRMcomment,~SegmentType,~FirstSale',
			}), // pending
			signature: new GridProperties({
				icon: 'pencil-square-o',
				action: 'UwGridSignature',
				columns: '#Id,Cart,MP_List,Name,Email,^ApplyDate,^RegDate,CurrentStatus,$CalcAmount,$OSBalance,Pending,LastStatus,Broker,CRMcomment,~SegmentType,~FirstSale',
			}), // signature
			approved: new GridProperties({
				icon: 'thumbs-o-up',
				action: 'UwGridApproved',
				columns: '#Id,Cart,MP_List,Name,Email,^ApplyDate,^ApproveDate,^RegDate,$CalcAmount,$ApprovedSum,$AmountTaken,#ApprovesNum,#RejectsNum,LastStatus,Broker,CRMcomment,~SegmentType,~FirstSale'
			}), // approved


			pendingInvestor: new GridProperties({ 
				icon: 'hourglass-half',
				title: 'Pending Investor',
				action: 'UwGridPendingInvestor',
				columns: '#Id,Name,Grade,ApplicantScore,$ApprovedAmount,!Term,*RequestApprovedAt,&TimeLimitUntilAutoreject,FindInvestor,EditOffer,ChooseInvestor,SubmitChosenInvestor,ManageChosenInvestor',
				fnRowCallback: function(oTR, oData, iDisplayIndex, iDisplayIndexFull) {
					if (oData.hasOwnProperty('ChooseInvestor')) {
						$('.grid-item-ChooseInvestor', oTR).addClass('grid-btn').html('<select class="form-control choose-investor" id="DataID' +
							oData.Id + '"></select>');
					}

					var rowID = '#DataID' + oData.Id.toString();
					var oSelect = $(oTR).find(rowID);

					$.each(oData.ChooseInvestor, function(idx, investorData) {
						oSelect.append(
							$('<option></option>').val(investorData.InvestorID).text(investorData.InvestorName + ' (£' + investorData.InvestorFunds + ')').attr('funds', investorData.InvestorFunds)
						);
					}); // for each
					
					if (oData.hasOwnProperty('Id')) {
						$('.grid-item-Id', oTR).empty().html(EzBob.Underwriter.GridTools.profileLink(oData.Id));

						if (oData.hasOwnProperty('Name')) {
							if (oData.Name)
								$('.grid-item-Name', oTR).empty().html(EzBob.Underwriter.GridTools.profileLink(oData.Id, oData.Name));
						} // if has name

						$(oTR).dblclick(function() {
							return false;
						});
					} // if has id

					var link;

					if (oData.hasOwnProperty('FindInvestor'))
						$('.grid-item-FindInvestor', oTR).empty().addClass('grid-btn').html(EzBob.Underwriter.GridTools.investorButton(oData.FindInvestor, 'find-investor', oData.Id, oData.CashRequestID));

					if (oData.hasOwnProperty('EditOffer')) {

						 link ='#profile/' + oData.Id;

						 $('.grid-item-EditOffer', oTR).empty().addClass('grid-btn').html(EzBob.Underwriter.GridTools.investorButton(oData.EditOffer, 'edit-offer', oData.Id, oData.CashRequestID, link));
					}

					if (oData.hasOwnProperty('SubmitChosenInvestor'))
						$('.grid-item-SubmitChosenInvestor', oTR).empty().addClass('grid-btn').html(EzBob.Underwriter.GridTools.investorButton(oData.SubmitChosenInvestor, 'submit-choosen-investor', oData.Id, oData.CashRequestID));

					if (oData.hasOwnProperty('ManageChosenInvestor')) {

						link = '#configInvestor';

						$('.grid-item-ManageChosenInvestor', oTR).empty().addClass('grid-btn').html(EzBob.Underwriter.GridTools.investorButton(oData.ManageChosenInvestor, 'manage-choosen-investor', oData.Id, oData.CashRequestID, link));
					}

					if (oData.IsWasLate)
						$(oTR).addClass("table-flag-red");

				}
			}), // pendingInvestor


			loans: new GridProperties({
				icon: 'gbp',
				action: 'UwGridLoans',
				columns: '#Id,Cart,MP_List,Name,Email,^RegDate,^FirstLoanDate,^LastLoanDate,$LastLoanAmount,$AmountTaken,$TotalPrincipalRepaid,$OSBalance,^NextRepaymentDate,LastStatus,Broker,CRMcomment,~SegmentType,~FirstSale',
			}), // loans
			sales: new GridProperties({
				icon: 'phone',
				action: 'UwGridSales',
				columns: '#Id,Email,Name,MobilePhone,DaytimePhone,$ApprovedSum,$AmountTaken,^OfferDate,$OSBalance,CRMstatus,CRMcomment,#Interactions,SegmentType',
			}), // sales
			collection: new GridProperties({
				icon: 'rocket',
				action: 'UwGridCollection',
				columns: '#Id,Email,Name,MobilePhone,DaytimePhone,$AmountTaken,$OSBalance,CRMstatus,CRMcomment,CollectionStatus,SegmentType',
			}), // collection
			late: new GridProperties({
				icon: 'flag',
				action: 'UwGridLate',
				columns: '#Id,Cart,MP_List,Name,Email,^ApplyDate,^ApproveDate,^RegDate,$CalcAmount,$ApprovedSum,$AmountTaken,#ApprovesNum,#RejectsNum,SegmentType,$OSBalance,^LatePaymentDate,$LatePaymentAmount,#Delinquency,CRMstatus,CRMcomment'
			}), // late
			rejected: new GridProperties({
				icon: 'thumbs-o-down',
				action: 'UwGridRejected',
				columns: '#Id,Cart,MP_List,Name,Email,^ApplyDate,^RegDate,^DateRejected,Reason,#RejectsNum,#ApprovesNum,$OSBalance,LastStatus,Broker,CRMcomment,~SegmentType,~FirstSale',
			}), // rejected
			all: new GridProperties({
				icon: 'female',
				action: 'UwGridAll',
				columns: '#Id,Cart,MP_List,Name,Email,^RegDate,^ApplyDate,CustomerStatus,$CalcAmount,$ApprovedSum,$OSBalance',
			}), // all
			registered: new GridProperties({
				icon: 'bars',
				action: 'UwGridRegistered',
				columns: '#UserId,Email,UserStatus,^RegDate,MP_Statuses,WizardStep,SegmentType',
				fnRowCallback: function(oTR, oData, iDisplayIndex, iDisplayIndexFull) {
					$('.grid-item-UserId', oTR).empty().html(EzBob.Underwriter.GridTools.profileLink(oData.UserId));

					$(oTR).dblclick(function() {
						location.assign($(oTR).find('.profileLink').first().data('href'));
					});

					if (oData.IsWasLate)
						$(oTR).addClass(oData.IsWasLate);
				}, // fnRowCallback
			}), // registered
			logbook: new GridProperties({
				icon: 'file-text-o',
				action: 'UwGridLogbook',
				columns: '#EntryID,LogbookEntryTypeDescription,FullName,^EntryTime,EntryContent',
				AddToTitle: function() {
					return $('#reload-button-template')
						.clone()
						.text('Add new entry')
						.attr('id', 'logbook-show-new-entry-form')
						.removeClass('hide');
				}, // AddToTitle
				OnAfterRender: function(oView, sTableContainer) {
					oView.$el.find(sTableContainer + ' #logbook-show-new-entry-form')
						.toggleClass(
							'hide',
							!oView.$el.find(sTableContainer + ' #logbook-new-entry-form').hasClass('hide')
						);
				}, // OnAfterRender
				fnRowCallback: function(oTR, oData, iDisplayIndex, iDisplayIndexFull) {
					$('.grid-item-EntryContent', oTR).empty().html(oData.EntryContent.replace(/\n/g, '<br>'));
				}, // fnRowCallback
			}), // logbook
			brokers: new GridProperties({
				icon: 'male',
				action: 'UwGridBrokers',
				columns: '#BrokerID,FirmName,ContactName,ContactEmail,ContactMobile,ContactOtherPhone,FirmWebSiteUrl',
				fnRowCallback: function(oTR, oData, iDisplayIndex, iDisplayIndexFull) {
					$('.grid-item-BrokerID', oTR).empty().html(EzBob.Underwriter.GridTools.brokerProfileLink(oData.BrokerID));

					$(oTR).dblclick(function() {
						location.assign($(oTR).find('.profileLink').first().data('href'));
					});
				}, // fnRowCallback
			}), // brokers

			investors: new GridProperties({
				icon: 'male',
				action: 'UwGridInvestors',
				columns: '#InvestorID,InvestorType,CompanyName,^Timestamp',
				fnRowCallback: function (oTR, oData, iDisplayIndex, iDisplayIndexFull) {
					$('.grid-item-InvestorID', oTR).empty().html(EzBob.Underwriter.GridTools.investorManageLink(oData.InvestorID));

					$(oTR).dblclick(function () {
						location.assign($(oTR).find('.profileLink').first().data('href'));
					});
				}, // fnRowCallback
			}), // investors
		}; // gridProperties
	}, // initialize

	events: {
		'click #include-test-customers': 'reloadActive',
		'click #include-all-customers': 'reloadActive',
		'click .reload-button': 'reloadActive',
		'click #logbook-show-new-entry-form': 'showNewLogbookEntryForm',
		'click #logbook-new-entry-cancel': 'hideNewLogbookEntryForm',
		'click #logbook-new-entry-save': 'submitNewLogbookEntryForm',
		'mouseup .profileLink': 'profileLinkClicked',
		'click .find-investor': 'findInvestor',
		'click .edit-offer': 'profileLinkClicked',
		'click .submit-choosen-investor': 'submitChosenInvestor',
		'click .manage-choosen-investor': 'manageChosenInvestor'
	}, // events

	findInvestor: function(evt) {
		var customerID = $(evt.currentTarget).attr('data-id');
		var cashRequestID = $(evt.currentTarget).attr('data-crid');
		var self = this;
		BlockUi();
		$.post(window.gRootPath + 'Underwriter/Investor/FindInvestor', { customerID: customerID, cashRequestID: cashRequestID })
			.done(function (data) {
				if (data.error || data.warning) {
					EzBob.ShowMessage(data.error + ' ' + data.warning, 'Failed to link offer to investor', null, 'Close');
				}
				self.reloadActive();
			}) // on success
			.fail(function(error) {
				EzBob.ShowMessage(error, 'Failed to find investor', null, 'Close');
			}) // on fail
			.always(function () {
				UnBlockUi();
			}); // on always
	},

	submitChosenInvestor: function(evt) {
		var customerID = $(evt.currentTarget).attr('data-id');
		var cashRequestID = $(evt.currentTarget).attr('data-crid');
		var chooseInvestorElem = '.choose-investor#DataID' + customerID;
		var chosenInvestorID = $(chooseInvestorElem).val();
		var self = this;
		

		if (chosenInvestorID === '') {
			$(chooseInvestorElem).focus();
			return;
		} // if

		BlockUi();
		$.post(window.gRootPath + 'Underwriter/Investor/SubmitInvestor', { customerID: customerID, investorID: chosenInvestorID, cashRequestID: cashRequestID })
			.done(function (data) {
				if (data.error || data.warning) {
					EzBob.ShowMessage(data.error + ' ' + data.warning, 'Failed to link offer to investor', null, 'Close');
				}
				self.reloadActive();
			}) // on success
			.fail(function() {
				EzBob.ShowMessage(error, 'Failed to link offer to investor', null, 'Close');
			}) // on fail
			.always(function () {
				UnBlockUi();
			}); // on always
	},

	manageChosenInvestor: function(evt) {
		event.preventDefault();
		event.stopPropagation();
		var customerID = $(evt.currentTarget).attr('data-id');
		var chooseInvestorElem = '.choose-investor#DataID' + customerID;
		var chosenInvestorID = $(chooseInvestorElem).val();
		location.assign($(evt.currentTarget).data('href') + '/' + chosenInvestorID);
	}, // manageChosenInvestor

	profileLinkClicked: function(evt) {
		event.preventDefault();
		event.stopPropagation();

		if (evt.button === 1)
			window.open($(evt.currentTarget).data('href'));
		else if (evt.button === 0) {
			if (evt.shiftKey)
				window.open($(evt.currentTarget).data('href'));
			else 
				location.assign($(evt.currentTarget).data('href'));
		} // if

		return false;
	}, // profileLinkClicked

	submitNewLogbookEntryForm: function() {
		var nEntryType = $('#logbook-new-entry-type').val();

		if (nEntryType === '') {
			$('#logbook-new-entry-type').focus();
			return;
		} // if

		var sMsg = $.trim($('#logbook-new-entry').val());

		if (sMsg === '') {
			$('#logbook-new-entry').focus();
			return;
		} // if

		BlockUi();

		var self = this;

		$.post(window.gRootPath + 'Underwriter/LogBook/Add', { type: nEntryType, content: sMsg }, 'json')
			.done(function(data) {
				self.handleNewLogbookSaved(data.success, data.msg);
			}) // on success
			.fail(function() {
				self.handleNewLogbookSaved(false, 'Failed to save an entry.');
			}); // on fail
	}, // submitNewLogbookEntryForm

	handleNewLogbookSaved: function(bSuccess, sMsg) {
		UnBlockUi();

		if (bSuccess) {
			EzBob.ShowMessageTimeout(sMsg || 'New entry has been logged successfully.', 'Success', 2);

			this.hideNewLogbookEntryForm();
			this.reloadActive();
		}
		else
			EzBob.ShowMessage(sMsg || 'Failed to save an entry.', 'Error');
	}, // handleNewLogbookSaved

	showNewLogbookEntryForm: function() {
		$('#logbook-new-entry-form').slideDown('slow').removeClass('hide');
		$('#logbook-show-new-entry-form').addClass('hide');
		$('#logbook-new-entry-type').focus();
	}, // showNewLogbookEntryForm

	hideNewLogbookEntryForm: function() {
		$('#logbook-new-entry-form').slideUp('slow').addClass('hide');
		$('#logbook-show-new-entry-form').removeClass('hide');

		$('#logbook-new-entry-type').val('');
		$('#logbook-new-entry').val('');
	}, // hideNewLogbookEntryForm

	loadLogbookEntryTypeList: function() {
		$.getJSON(
			window.gRootPath + 'Underwriter/LogBook/Index',
			function(data) {
				var oSelect = $('#logbook-new-entry-type');

				$.each(data, function(idx, oEntryType) {
					oSelect.append(
						$('<option></option>').val(oEntryType.ID).text(oEntryType.Description)
					);
				}); // for each
			} // onsuccess
		);
	}, // loadLogbookEntryTypeList

	render: function() {
		var self = this;

		this.tabLinks().on('shown.bs.tab', function(e) { self.handleTabSwitch(e); });

		return this;
	}, // render

	handleTabSwitch: function(evt) {
		this.loadGrid(this.typeFromHref($(evt.target).attr('href')));
	}, // handleTabSwitch

	toggleAllCustomers: function(sCurTabType) {
		this.$el.find('.all-customers').toggleClass('hide', sCurTabType !== 'registered');
	}, // toggleAllCustomers

	reloadActive: function() {
		this.loadGrid(this.activeGridType());
	}, // reloadActive

	activeGridType: function() {
		return this.typeFromHref(
			this.tabLinks().closest('li').filter('.active').find('a[data-toggle]').attr('href')
		);
	}, // activeGridType

	loadGrid: function(sGridName) {
		if (this.router)
			this.router.navigate('#customers/' + sGridName);

		localStorage.setItem('uw_grids_last_shown-' + $('#uw-name-and-icon').attr('data-uw-id'), sGridName);

		this.toggleAllCustomers(sGridName);

		this.tabLinks().closest('li').filter('.active').removeClass('active');

		this.tabLinkTo(sGridName).closest('li').addClass('active');

		this.$el.find('.tab-pane').hide().filter('#' + sGridName + '-grid').show();

		var oGridProperties = this.gridProperties[sGridName];

		if (!oGridProperties)
			return;

		if (!oGridProperties.name)
			oGridProperties.name = sGridName;

		if (!oGridProperties.title)
			oGridProperties.title = sGridName.charAt(0).toUpperCase() + sGridName.slice(1);

		var sTableContainer = '#' + sGridName + '-grid';

		this.$el.find(sTableContainer + ' .grid-data').dataTable({
			bDestroy: true,
			bProcessing: true,
			sAjaxSource: this.gridSrcUrl(oGridProperties),
			aoColumns: EzBob.DataTables.Helper.extractColumns(oGridProperties.columns),

			bDeferRender: true,

			aLengthMenu: [[-1, 10, 20, 50, 100], ['all', 10, 20, 50, 100]],
			iDisplayLength: 20,

			sPaginationType: 'bootstrap',
			bJQueryUI: false,

			fnRowCallback: oGridProperties.fnRowCallback,

			aaSorting: [[0, 'desc']],

			bAutoWidth: false,
			sDom: '<"top"<"box"<"box-title"<"dataTables_top_right"if><"dataTables_top_left">><"box-content"tr<"row"<"col-md-6"l><"col-md-6 dataTables_bottom_right"p>>>>>',

			bStateSave: true,

			fnStateSave: function(oSettings, oData) {
				var sKey = 'uw-grid-state-' + $('#uw-name-and-icon').attr('data-uw-id') + '-' + sGridName;
				localStorage.setItem(sKey, JSON.stringify(oData));
			}, // fnStateSave

			fnStateLoad: function(oSettings) {
				var sKey = 'uw-grid-state-' + $('#uw-name-and-icon').attr('data-uw-id') + '-' + sGridName;
				var sData = localStorage.getItem(sKey);
				var oData = sData ? JSON.parse(sData) : null;
				return oData;
			}, // fnStateLoad

			fnInitComplete: function(oSettings, json) {
				$('[data-toggle="tooltip"]').tooltip({ html: true, placement: "bottom" });
			},
		}); // create data table

		var oTableTitle = this.$el.find(sTableContainer + ' .dataTables_top_left');

		if ($('h3', oTableTitle).length < 1) {
			this.$el.find(sTableContainer + ' .dataTables_top_right,' + sTableContainer + ' .dataTables_bottom_right').prepend(
				$('#reload-button-template').clone().attr('id', '').removeClass('hide').addClass('reload-button')
			);

			oTableTitle.append('<h3><i class="fa fa-' + oGridProperties.icon + '"></i>' + oGridProperties.title + '</h3>');

			var oAdditional = oGridProperties.AddToTitle();

			if (oAdditional)
				oTableTitle.append(oAdditional);
		} // if

		oGridProperties.OnAfterRender(this, sTableContainer);
	}, // loadGrid

	gridSrcUrl: function(oGridProperties) {
		var bIncludeTest = this.$el.find('#include-test-customers:checked').length > 0;
		var bIncludeAll = this.$el.find('#include-all-customers:visible:checked').length > 0;

		var oTabPane = this.$el.find('#' + oGridProperties.name + '-grid');

		var oShowingTest = oTabPane.find('.showing-test');

		if (bIncludeTest) {
			if (oShowingTest.length !== 1)
				oTabPane.find('h2').append('<span class=showing-test> (including test customers)</span>');
		}
		else
			oShowingTest.remove();

		return window.gRootPath + 'Underwriter/Grids/Load' +
			'?grid=' + oGridProperties.action +
			'&includeTestCustomers=' + (bIncludeTest ? 'true' : 'false') +
			'&includeAllCustomers=' + (bIncludeAll ? 'true' : 'false');
	}, // gridSrcUrl

	tabLinks: function() {
		return this.$el.find('a[data-toggle="tab"]');
	}, // tabLinks

	tabLinkTo: function(sTarget) {
		return this.tabLinks().filter('[href="#' + (sTarget || '') + '-grid"]');
	}, // tabLinkTo

	/// id argument is not used here. It was added to be consistent with
	/// previously created views.
	show: function(id, type) {
		this.$el.show();
		this.loadGrid(this.getValidType(type));
	}, // show

	typeFromHref: function(sHref) {
		if (!sHref)
			sHref = this.tabLinks().first().attr('href');

		var aryMatch = sHref.match(/^#(.+)-grid$/);

		if (aryMatch && aryMatch[1])
			return aryMatch[1];

		return this.typeFromHref(this.tabLinks().first().attr('href'));
	}, // typeFromHref

	getValidType: function(sType) {
		if (this.isValidType(sType))
			return sType;

		return this.typeFromHref();
	}, // getValidType

	isValidType: function(sType) {
		return this.tabLinkTo(sType).length === 1 || sType === 'investors';
	}, // isValidType

	hide: function() {
		this.$el.hide();
	}, // hide
}); // EzBob.Underwriter.GridsView
