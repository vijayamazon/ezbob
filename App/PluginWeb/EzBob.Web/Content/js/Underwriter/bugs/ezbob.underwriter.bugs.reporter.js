var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.BugModel = Backbone.Model.extend({
	url: function() {
		return window.gRootPath + 'Underwriter/Bugs/Report';
	}, // url

	idAttribute: 'Id',

	defaults: {
		TextOpened: '',
		Type: 'Other',
		DateOpened: new Date(),
		State: 'Opened',
		MarketPlaceId: null,
	}, // defaults

	save: function() {
		if (this.isNew()) {
			return EzBob.Underwriter.BugModel.__super__.save.call(this, {}, {
				url: '' + window.gRootPath + 'Underwriter/Bugs/CreateBug'
			});
		} else {
			this.set('DateOpened', new Date(moment.utc(this.get('DateOpened'))));

			return EzBob.Underwriter.BugModel.__super__.save.call(this, {}, {
				url: '' + window.gRootPath + 'Underwriter/Bugs/UpdateBug'
			});
		}
	}, // save
}); // EzBob.Underwriter.BugModel

EzBob.Underwriter.Bugs = Backbone.Collection.extend({
	model: EzBob.Underwriter.BugModel,
}); // EzBob.Underwriter.Bugs

EzBob.Underwriter.ReportBugView = EzBob.BoundItemView.extend({
	template: '#bug-report-template',

	bindings: {
		TextOpened: {
			selector: "textarea[name='description']"
		},
	},
}); // EzBob.Underwriter.ReportBugView

(function() {
	var eqConverter = function(v) {
		return function(direction, value) {
			return value === v;
		};
	}; // eqConverter

	var notEqConverter = function(v) {
		return function(direction, value) {
			return value !== v;
		};
	}; // notEqConverter

	EzBob.Underwriter.EditBugView = EzBob.BoundItemView.extend({
		events: {
			'click .closeBug': 'closeBug',
			'click .reopenBug': 'reopenBug'
		}, // events

		template: '#bug-edit-template',

		bindings: {
			TextOpened: {
				selector: "textarea[name='description']"
			},
			TextClosed: {
				selector: "textarea[name='closeDescription']"
			},
			State: [
				{
					selector: 'textarea',
					converter: notEqConverter('Closed'),
					elAttribute: 'enabled'
				}, {
					selector: '.closeBug',
					converter: notEqConverter('Closed'),
					elAttribute: 'displayed'
				}, {
					selector: '.underwriter-closed',
					converter: eqConverter('Closed'),
					elAttribute: 'displayed'
				}, {
					selector: '.reopenBug',
					converter: eqConverter('Closed'),
					elAttribute: 'displayed'
				}, {
					selector: '.saveBug',
					converter: notEqConverter('Closed'),
					elAttribute: 'enabled'
				}
			],
		}, // bindings

		closeBug: function() {
			this.trigger('closeBug');
			this.close();
		}, // closeBug

		reopenBug: function() {
			this.trigger("reopenBug");
		}, // reopenBug
	}); // EzBob.Underwriter.EditBugView
})();

EzBob.InitBugs = function() {
	$('body').unbind('click').on('click', 'a[data-bug-type]', function(e) {
		var $e = $(e.currentTarget);
		var bugType = $e.attr('data-bug-type');
		var bugMP = $e.attr('data-bug-mp');
		var bugCustomer = $e.attr('data-bug-customer');
		var director = $e.attr('data-credit-bureau-director-id');

		if (!((bugType != null) && (bugCustomer != null)))
			return false;

		var xhr = $.getJSON('' + window.gRootPath + 'Underwriter/Bugs/TryGet', {
			MP: bugMP,
			CustomerId: bugCustomer,
			BugType: bugType,
			Director: director
		});

		xhr.done(function(data) {
			if ((data != null ? data.error : void 0))
				return;

			var view = null;
			var model = null;

			if (data != null ? data.Id : void 0) {
				model = new EzBob.Underwriter.BugModel(data);

				view = new EzBob.Underwriter.EditBugView({
					model: model
				});

				view.on('closeBug', function() {
					$.post(window.gRootPath + 'Underwriter/Bugs/Close', model.toJSON()).done(function(response) {
						EzBob.UpdateBugsIcon($e, response.State);
						view.close();
					});
				});

				view.on('reopenBug', function() {
					$.post(window.gRootPath + 'Underwriter/Bugs/Reopen', model.toJSON()).done(function(response) {
						model = new EzBob.Underwriter.BugModel(response);
						view.model = model;
						view.render();
						return EzBob.UpdateBugsIcon($e, response.State);
					});
				});

				view.on('closed', function() {
					EzBob.UpdateBugsIcon($e, model.get('State'));
				});
			}
			else {
				model = new EzBob.Underwriter.BugModel({
					Type: bugType,
					CustomerId: bugCustomer,
					MarketPlaceId: bugMP,
					DirectorId: director
				});

				view = new EzBob.Underwriter.ReportBugView({
					model: model
				});

				EzBob.UpdateBugsIcon($e, model.get('State'));

				view.on('closed', function() {
					EzBob.UpdateBugsIcon($e, 'New');
				});
			} // if

			view.on('save', function() {
				if (model.get('State') !== 'Closed') {
					model.save();
				}
				view.close();
			});

			view.options = {
				show: true,
				keyboard: false,
				focusOn: 'textarea:first',
			};

			EzBob.App.jqmodal.show(view);
		}); // done of TryGet

		return false;
	}); // on click
}; // EzBob.InitBugs
