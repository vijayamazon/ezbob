var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.AddCustomerRelationsEntry = EzBob.BoundItemView.extend({
    template: '#add-customer-relations-entry-template',

    events: {
        'keyup #Comment': 'commentKeyup',
        'change #Action': 'determinePhoneNumbersState',
        'change input[name="Type"]': 'determinePhoneNumbersState'
    }, // events

    isUnderwriter: document.location.href.indexOf("Underwriter") > -1,

    jqoptions: function() {
        return {
            modal: true,
            resizable: this.isUnderwriter,
            title: 'CRM - add entry',
            position: 'center',
            draggable: true,
            dialogClass: 'customer-relations-popup',
            width: 600
        };
    }, // jqoptions

    initialize: function (options) {
        this.onsave = options.onsave;
        this.onbeforesave = options.onbeforesave;
        this.customerId = this.model.customerId;
        this.url = options.url;
	    this.isBroker = options.isBroker;

        EzBob.Underwriter.AddCustomerRelationsEntry.__super__.initialize.apply(this, arguments);
    }, // initialize

    onRender: function () {
        EzBob.Underwriter.AddCustomerRelationsEntry.__super__.onRender.apply(this, arguments);

        this.ui.Form.get(0).reset();
        this.$el.find("input#Type_Out").prop('checked', true);
        this.ui.Action.prop('selectedIndex', 1);
        
        var rank = this.model.get('CurrentRank');
        if (rank) {
            this.ui.Rank.val(rank.Id);
            this.ui.RankSpan.addClass('active attardi-has-data');
        }
        
        if(this.model.get('isBroker')) {
        	this.ui.RankDiv.hide();
        	this.ui.PhoneNumberDiv.hide();
        }
	    var self = this;
        _.each(EzBob.CrmActions, function(action) {
		    self.ui.Action.append($('<option value="' + action.Id + '">' + action.Name + '</option>'));
        });

        _.each(this.model.get("PhoneNumbers"), function(phone) {
		var phoneVerificationState = phone.IsVerified ? '(Verified)' : '';
		self.ui.PhoneNumber.append($('<option value="' + phone.Number + '">' + phone.Type + ':' + phone.Number + phoneVerificationState + '</option>'));
        });

        var bIsBroker = this.isBroker;
        var statuses = _.filter(EzBob.CrmStatuses, function(s) { return s.IsBroker === bIsBroker; });
	    
        _.each(statuses, function(sGroup) {
        	var optGroup = $('<optgroup label="' + sGroup.Name + '"></optgroup>');
        	_.each(sGroup.Statuses, function(status) { optGroup.append($('<option value="' + status.Id + '">' + status.Name + '</option>')) });
        	self.ui.Status.append(optGroup);
	    });
	    
    }, // onRender
	
    serializeData: function () {
	    var bIsBroker = this.isBroker;

        return {
            actions: EzBob.CrmActions,
            statuses: _.filter(EzBob.CrmStatuses, function(s) { return s.IsBroker === bIsBroker; }),
            ranks: EzBob.CrmRanks,
            phoneNumbers: this.model.get("PhoneNumbers")
        };
    }, // serializeData

    commentKeyup: function (el) {
        return this.ui.Comment.val(this.ui.Comment.val().replace(/\r\n|\r|\n/g, '\r\n').slice(0, 1000));
    }, // commentKeyup
    
    determinePhoneNumbersState: function () {
        var isTypeOutgoing = document.getElementById("Type_Out").checked;
        var actionElement = document.getElementById("Action");
        var isActionCall = actionElement.options[actionElement.selectedIndex].text;

        var phoneNumbersSectionJqElement = $(".phoneNumbersSection");
        if (isTypeOutgoing && isActionCall == 'Call') {
            phoneNumbersSectionJqElement.removeClass('hide');
        } else {
            phoneNumbersSectionJqElement.addClass('hide');
        }
    },
    
    ui: {
        Type: 'input[name="Type"]:checked',
        Status: '#Status',
        Action: '#Action',
        Rank: '#Rank',
        RankSpan: '.rank-span',
        RankDiv: '.rank-div',
        Comment: '#Comment',
        PhoneNumber: '#PhoneNumber',
        PhoneNumberDiv: '.phoneNumbersSection',
        Form: 'form#customer-relations-form'
    }, // ui

    onSave: function() {

        if (this.ui.Status[0].selectedIndex === 0)
            return false;

        if (this.ui.Action[0].selectedIndex === 0)
            return false;

        var phoneNumber = '';
        if (this.ui.PhoneNumber.length > 0) {
            phoneNumber = this.ui.PhoneNumber[0].value;
        }

        BlockUi();
        var opts = {
            type: this.$el.find('input[name="Type"]:checked').data("value"),
            action: this.ui.Action[0].value,
            status: this.ui.Status[0].value,
            rank: this.ui.Rank[0].value,
            comment: this.ui.Comment.val(),
            customerId: this.customerId,
            isBroker: this.isBroker,
            phoneNumber: phoneNumber
        };

        if (this.onbeforesave)
            this.onbeforesave(opts);

        var self = this;

        var xhr = $.post(this.url, opts);

        xhr.done(function (r) {
            if (r.success && !self.model.get('isBroker')) {
                self.model.fetch();
            } else if (r.success && self.onsave) {
            	$('body').removeClass('stop-scroll');
                self.onsave();
            } else {
                if (r.error) {
                    EzBob.ShowMessage(r.error, 'Error');
                }
            } // if

            self.close();
        });

        xhr.always(function() {
            return UnBlockUi();
        });

        return false;
    }, // onSave
    
}); // EzBob.Underwriter.AddCustomerRelationsEntry
