var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.JournalView = Backbone.Marionette.ItemView.extend({
	template: '#journal-template',
	initialize: function(options) {
		this.crmModel = options.crmModel;

    	this.bindTo(this.model, 'change sync', this.render, this);
    	this.bindTo(this.crmModel, 'change sync', this.render, this);
    	BlockUi('on', this.$el);

        return this;
    },

	onRender: function() {
		this.journal = [];
		this.journalTable = null;
	    this.buildJournal();
    	BlockUi('off', this.$el);
    },

    buildJournal: function() {
	    var self = this;
    	this.journal = [];
    	if (this.crmModel.get('CustomerRelations')) {
		    _.each(this.crmModel.get('CustomerRelations'), function(crm) {
			    self.journal.push({
				    Action: crm.Action,
				    Adate: new Date(moment.utc(crm.DateTime)),
				    Type: crm.Type,
				    Status: crm.Status,
				    ApprovedSum: null,
				    Interest: null,
				    RepaymentPeriod: null,
				    LoanType: null,
				    SetupFee: null,
				    DiscountPlan: null,
				    LoanSource: null,
				    CustomerSelection: null,
				    UW: crm.User,
				    Comment: crm.Comment,
				    IsCrm: true,
				    IsOpenPlatform: null
				});
		    });
    	}

    	if (this.model.get('DecisionHistory')) {
		    _.each(this.model.get('DecisionHistory'), function(dh) {
			    self.journal.push({
				    Action: dh.Action,
				    Adate: new Date(moment.utc(dh.Date)),
				    Type: null,
				    Status: null,
				    ApprovedSum: dh.ApprovedSum,
				    Interest: dh.InterestRate,
				    RepaymentPeriod: dh.RepaymentPeriod,
				    LoanType: dh.LoanType.split(" ")[0],
				    SetupFee: dh.TotalSetupFee ? EzBob.formatPounds(dh.TotalSetupFee) : '-',
				    DiscountPlan: dh.DiscountPlan,
				    LoanSource: (dh.LoanSourceName === 'Standard' ? '' : dh.LoanSourceName),
				    CustomerSelection: (dh.IsLoanTypeSelectionAllowed === 1 ? 'Yes' : 'No'),
				    UW: dh.UnderwriterName,
				    Comment: dh.Comment,
				    IsCrm: false,
					IsOpenPlatform: dh.IsOpenPlatform
			    });
		    });
	    }

    	if (this.journal.length > 0 && this.model && this.crmModel) {
    		try {
    			if (this.journalTable) {
			    	this.journalTable.fnClearTable();
				    this.journalTable.fnAddData(this.journal);
				    this.journalTable.fnDraw();
				    return;
			    }

    			this.journalTable = this.$el.find("#journalTable").dataTable({
    				aLengthMenu: [[-1, 10, 20, 50, 100], ['all', 10, 20, 50, 100]],
    				iDisplayLength: 20,
    				sPaginationType: 'bootstrap',
    				bJQueryUI: false,
    				aaSorting: [[1, 'desc']],
    				bAutoWidth: true,
    				aaData: this.journal,
					aoData: [],
    				aoColumns: EzBob.DataTables.Helper.extractColumns('Action,^Adate,Type,Status,$ApprovedSum,%Interest,RepaymentPeriod,LoanType,SetupFee,DiscountPlan,LoanSource,CustomerSelection,IsOpenPlatform,UW,Comment,~IsCrm'),
    				aoColumnDefs: [
                        {
                        	"aTargets": [1],
                        	"sType": 'date'
                        }
    				],
    				//bDestroy: true,
    				bDeferRender: true
    			});
    			EzBob.DataTables.Helper.initCustomFiltering();
    		} catch (ex) {
    			console.log('journal table exception', ex);
    		}
    	}
    },

    journalFilter: function(e) {
    	var allJournal = this.$el.find('#allJournal');
    	if ($(e.currentTarget).attr('id') !== 'allJournal' && allJournal.is(':checked')) {
    		allJournal.parent().click();
    		return false;
    	}
    	this.journalTable.fnDraw();
    	return false;
    }
});
