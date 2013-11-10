var EzBob = EzBob || {};

EzBob.WizardStepModel = Backbone.Model.extend({
	defaults: {
		completed: false
	}
});

EzBob.WizardSteps = Backbone.Collection.extend({
	model: EzBob.WizardStepModel
});
