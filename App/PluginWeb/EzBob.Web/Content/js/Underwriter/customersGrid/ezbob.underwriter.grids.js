﻿var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.GridTools = {
	showMedalIcon: function(cellval) {
		var text;
		text = cellval.text || cellval;
		return '<i data-toggle=tooltip title="' + text + '" class="' + (text.toLowerCase().replace(/\s/g, '')) + '"></i>';
	}, // showMedalIcon

	profileLink: function(nCustomerID, sLinkText) {
		return EzBob.DataTables.Helper.withScrollbar('<a class=profileLink title="Open customer profile" href="#profile/' + nCustomerID + '">' + (sLinkText || nCustomerID) + '</a>');
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

		    if (oData.hasOwnProperty('MP_List')) {
		        $('.grid-item-MP_List', oTR).empty().html(EzBob.DataTables.Helper.showNewMPsIcon(oData.MP_List, oData.SegmentType));
		    }

		    if (oData.hasOwnProperty('Email')) {
		        $('.grid-item-Email', oTR).empty().html(EzBob.DataTables.Helper.showEmail(oData.Email));
		    }
		    
		    if (oData.hasOwnProperty('Broker')) {
		        $('.grid-item-Broker', oTR).empty().html(EzBob.DataTables.Helper.showBroker(oData.Broker, oData.FirstSale));
		    }

		    if (oData.hasOwnProperty('Id')) {
				$('.grid-item-Id', oTR).empty().html(EzBob.Underwriter.GridTools.profileLink(oData.Id));

				if (oData.hasOwnProperty('Name')) {
					if (oData.Name)
						$('.grid-item-Name', oTR).empty().html(EzBob.Underwriter.GridTools.profileLink(oData.Id, oData.Name));
				} // if has name

				$(oTR).dblclick(function() {
					location.assign($(oTR).find('.profileLink').first().attr('href'));
				});
			} // if has id

			if (oData.IsWasLate) {
				//$(oTR).addClass(oData.IsWasLate);
				$(oTR).addClass("table-flag-red");
			} // if
		    
			$(oTR).find('[data-toggle="tooltip"]').tooltip({ html: true, placement: "bottom" });
		}; // fnRowCallback

		this.gridProperties = {
			waiting: new GridProperties({
				icon: 'envelope-o',
				title: 'Waiting for decision',
				action: 'GridWaiting',
				columns: '#Id,Cart,MP_List,Name,Email,^ApplyDate,^RegDate,CurrentStatus,$CalcAmount,$OSBalance,LastStatus,CRMcomment,Broker,~SegmentType',
			}), // waiting
			escalated: new GridProperties({
				icon: 'arrow-up',
				action: 'GridEscalated',
				columns: '#Id,Cart,MP_List,Name,Email,^ApplyDate,^RegDate,CurrentStatus,$CalcAmount,$OSBalance,^EscalationDate,Underwriter,Reason',
			}), // escalated
			pending: new GridProperties({
				icon: 'clock-o',
				action: 'GridPending',
				columns: '#Id,Cart,MP_List,Name,Email,^ApplyDate,^RegDate,CurrentStatus,$CalcAmount,$OSBalance,Pending,LastStatus,CRMcomment,Broker,~SegmentType',
			}), // pending
			approved: new GridProperties({
				icon: 'thumbs-o-up',
				action: 'GridApproved',
				columns: '#Id,Cart,MP_List,Name,Email,^ApplyDate,^ApproveDate,^RegDate,$CalcAmount,$ApprovedSum,$AmountTaken,#ApprovesNum,#RejectsNum,LastStatus,CRMcomment,Broker,~SegmentType'
			}), // approved
			loans: new GridProperties({
				icon: 'gbp',
				action: 'GridLoans',
				columns: '#Id,Cart,MP_List,Name,Email,^RegDate,^FirstLoanDate,^LastLoanDate,$LastLoanAmount,$AmountTaken,$TotalPrincipalRepaid,$OSBalance,^NextRepaymentDate,LastStatus,CRMcomment,Broker,~SegmentType',
			}), // loans
			sales: new GridProperties({
				icon: 'phone',
				action: 'GridSales',
				columns: '#Id,Email,Name,MobilePhone,DaytimePhone,$ApprovedSum,$AmountTaken,^OfferDate,$OSBalance,CRMstatus,CRMcomment,#Interactions,SegmentType',
			}), // sales
			collection: new GridProperties({
				icon: 'rocket',
				action: 'GridCollection',
				columns: '#Id,Email,Name,MobilePhone,DaytimePhone,$AmountTaken,$OSBalance,CRMstatus,CRMcomment,CollectionStatus,SegmentType',
			}), // collection
			late: new GridProperties({
				icon: 'flag',
				action: 'GridLate',
				columns: '#Id,Cart,MP_List,Name,Email,^ApplyDate,^ApproveDate,^RegDate,$CalcAmount,$ApprovedSum,$AmountTaken,#ApprovesNum,#RejectsNum,SegmentType,$OSBalance,^LatePaymentDate,$LatePaymentAmount,#Delinquency,CRMstatus,CRMcomment'
			}), // late
			rejected: new GridProperties({
				icon: 'thumbs-o-down',
				action: 'GridRejected',
				columns: '#Id,Cart,MP_List,Name,Email,^ApplyDate,^RegDate,^DateRejected,Reason,#RejectsNum,#ApprovesNum,$OSBalance,LastStatus,CRMcomment,Broker,~SegmentType',
			}), // rejected
			all: new GridProperties({
				icon: 'female',
				action: 'GridAll',
				columns: '#Id,Cart,MP_List,Name,Email,^RegDate,^ApplyDate,CustomerStatus,$CalcAmount,$ApprovedSum,$OSBalance',
			}), // all
			registered: new GridProperties({
				icon: 'bars',
				action: 'GridRegistered',
				columns: '#UserId,Email,UserStatus,^RegDate,MP_Statuses,WizardStep,SegmentType',
				fnRowCallback: function (oTR, oData, iDisplayIndex, iDisplayIndexFull) {
					$('.grid-item-UserId', oTR).empty().html(EzBob.Underwriter.GridTools.profileLink(oData.UserId));

					$(oTR).dblclick(function() {
						location.assign($(oTR).find('.profileLink').first().attr('href'));
					});

					if (oData.IsWasLate)
						$(oTR).addClass(oData.IsWasLate);
				}, // fnRowCallback
			}), // registered
			logbook: new GridProperties({
				icon: 'file-text-o',
				action: 'GridLogbook',
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
				fnRowCallback: function (oTR, oData, iDisplayIndex, iDisplayIndexFull) {
					$('.grid-item-EntryContent', oTR).empty().html(oData.EntryContent.replace(/\n/g, '<br>'));
				}, // fnRowCallback
			}), // logbook
		}; // gridProperties
	}, // initialize

	events: {
		'click #include-test-customers': 'reloadActive',
		'click #include-all-customers': 'reloadActive',
		'click .reload-button': 'reloadActive',
		'click #logbook-show-new-entry-form': 'showNewLogbookEntryForm',
		'click #logbook-new-entry-cancel': 'hideNewLogbookEntryForm',
		'click #logbook-new-entry-save': 'submitNewLogbookEntryForm',
	}, // events

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

		$.post(window.gRootPath + 'Underwriter/Customers/AddLogbookEntry', { type: nEntryType, content: sMsg }, 'json')
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
			window.gRootPath + 'Underwriter/Customers/LoadLogbookEntryTypeList',
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

		this.tabLinks().on('shown.bs.tab', function (e) { self.handleTabSwitch(e); });

		return this;
	}, // render

	handleTabSwitch: function (evt) {
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

			aaSorting: [[ 0, 'desc' ]],

			bAutoWidth: true,
			sDom: '<"top"<"box"<"box-title"<"dataTables_top_right"if><"dataTables_top_left">>>>tr<"bottom"<"col-md-6"l><"col-md-6 dataTables_bottom_right"p>><"clear">',

			bStateSave: true,

			fnStateSave: function (oSettings, oData) {
				var sKey = 'uw-grid-state-' + $('#uw-name-and-icon').attr('data-uw-id') + '-' + sGridName;
				localStorage.setItem(sKey, JSON.stringify(oData));
			}, // fnStateSave

			fnStateLoad: function (oSettings) {
				var sKey = 'uw-grid-state-' + $('#uw-name-and-icon').attr('data-uw-id') + '-' + sGridName;
				var sData = localStorage.getItem(sKey);
				var oData = sData ? JSON.parse(sData) : null;
				return oData;
			}, // fnStateLoad
			
			fnInitComplete: function (oSettings, json) {
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

		return window.gRootPath + 'Underwriter/Customers/' + oGridProperties.action +
			'?includeTestCustomers=' + (bIncludeTest ? 'true' : 'false') +
			'&includeAllCustomers=' + (bIncludeAll ? 'true' : 'false');
	}, // gridSrcUrl

	tabLinks: function () {
		return this.$el.find('a[data-toggle="tab"]');
	}, // tabLinks

	tabLinkTo: function (sTarget) {
		return this.tabLinks().filter('[href="#' + (sTarget || '') + '-grid"]');
	}, // tabLinkTo

	/// id argument is not used here. It was added to be consistent with
	/// previously created views.
	show: function (id, type) {
		this.$el.show();
		this.loadGrid(this.getValidType(type));
	}, // show

	typeFromHref: function (sHref) {
		if (!sHref)
			sHref = this.tabLinks().first().attr('href');

		var aryMatch = sHref.match(/^#(.+)-grid$/);

		if (aryMatch && aryMatch[1])
			return aryMatch[1];

		return this.typeFromHref(this.tabLinks().first().attr('href'));
	}, // typeFromHref

	getValidType: function (sType) {
		if (this.isValidType(sType))
			return sType;

		return this.typeFromHref();
	}, // getValidType

	isValidType: function(sType) {
		return this.tabLinkTo(sType).length === 1;
	}, // isValidType

	hide: function() {
		this.$el.hide();
	}, // hide
}); // EzBob.Underwriter.GridsView
