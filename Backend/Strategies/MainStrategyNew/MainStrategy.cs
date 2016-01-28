namespace Ezbob.Backend.Strategies.MainStrategyNew {
	using System.Reflection;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.Exceptions;
	using Ezbob.Backend.Strategies.MainStrategy;
	using Ezbob.Backend.Strategies.MainStrategyNew.Steps;

	public class MainStrategy : AStrategy {
		public MainStrategy(MainStrategyArguments args) {
			if (args == null)
				throw new StrategyAlert(this, "No arguments specified for the main strategy.");

			this.context = new MainStrategyContextData(args);
		} // constructor

		public override string Name {
			get { return "Main strategy"; }
		} // Name

		public override void Execute() {
			AMainStrategyStepBase currentStep = CreateSteps();

			while (currentStep != null)
				currentStep = currentStep.Execute();
		} // Execute

		private AMainStrategyStepBase CreateSteps() {
			var checkUpdateDataRequested = new CheckUpdateDataRequested(
				this.context.Description,
				null, // TODO update data
				null, // TODO fraud check
				null, // TODO lock manual at the auto decision process beginning
				this.context.NewCreditLineOption
			);

			var applyBackdoorLogic = new ApplyBackdoorLogic(
				this.context.Description,
				null, // TODO Save decision
				checkUpdateDataRequested,
				this.context.BackdoorEnabled,
				this.context.CustomerID,
				this.context.CustomerDetails.AppEmail,
				this.context.CustomerDetails.OwnsProperty,
				this.context.CustomerDetails.IsTest,
				this.context.CashRequestOriginator,
				this.context.CashRequestID,
				this.context.NLCashRequestID,
				this.context.Tag
			);
			applyBackdoorLogic.CollectOutputValue += CollectStepOutputValue;

			var createFindCashRequest = new CreateFindCashRequest(
				this.context.Description,
				applyBackdoorLogic,
				this.context.CashRequestID,
				this.context.CashRequestOriginator,
				this.context.CustomerID,
				this.context.CustomerDetails.FullName,
				this.context.CustomerDetails.AppEmail,
				this.context.CustomerDetails.Origin,
				this.context.CustomerDetails.NumOfLoans,
				this.context.UnderwriterID
			);
			createFindCashRequest.CollectOutputValue += CollectStepOutputValue;

			var gatherData = new GatherData(
				this.context.Description,
				createFindCashRequest,
				this.context.CustomerID
			);
			gatherData.CollectOutputValue += CollectStepOutputValue;

			var finishWizard = new FinishWizardIfRequested(
				this.context.Description,
				gatherData,
				this.context.FinishWizardArgs
			);

			var validateInput = new ValidateInput(
				this.context.Description,
				finishWizard,
				this.context.CustomerDetails,
				this.context.CashRequestID,
				this.context.CashRequestOriginator
			);

			return new TheFirstOne(this.context.Description, validateInput);
		} // CreateSteps

		private void CollectStepOutputValue(string propertyName, object propertyValue) {
			PropertyInfo pi = this.context.GetType().GetProperty(propertyName);

			if (pi.PropertyType == typeof(InternalCashRequestID)) {
				InternalCashRequestID dst = (InternalCashRequestID)pi.GetValue(this.context);
				InternalCashRequestID src = (InternalCashRequestID)propertyValue;
				dst.Value = src.Value;
			} else if (pi.PropertyType == typeof(AutoDecisionResponse)) {
				this.context.AutoDecisionResponse.CopyFrom((AutoDecisionResponse)propertyValue);
			} else
				pi.SetValue(this.context, propertyValue);
		} // CollectStepOutputValue

		private readonly MainStrategyContextData context;
	} // class MainStrategy
} // namespace

