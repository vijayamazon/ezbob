namespace Ezbob.Backend.Strategies.MainStrategyNew {
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Reflection;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.Exceptions;
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using Ezbob.Backend.Strategies.MainStrategy;
	using Ezbob.Backend.Strategies.MainStrategyNew.Steps;

	[SuppressMessage("ReSharper", "ConvertIfStatementToNullCoalescingExpression")]
	public class MainStrategy : AStrategy {
		public MainStrategy(MainStrategyArguments args) {
			if (args == null)
				throw new StrategyAlert(this, "No arguments specified for the main strategy.");

			this.context = new MainStrategyContextData(args);
			this.mailer = new StrategiesMailer();
		} // constructor

		public override string Name {
			get { return "Main strategy"; }
		} // Name

		public override void Execute() {
			AMainStrategyStepBase currentStep = TheFirstOne;

			while (currentStep != null)
				currentStep = currentStep.Execute();
		} // Execute

		private void UpdateStrategyContext(string propertyName, object propertyValue) {
			var mpNamesToUpdate = propertyValue as List<string>;

			if ((mpNamesToUpdate == null) || (mpNamesToUpdate.Count < 1))
				return;

			Context.Description = string.Format(
				"This strategy can take long time (updating {0}).",
				string.Join(", ", mpNamesToUpdate)
			);
		} // UpdateStrategyContext

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

		private TheFirstOne TheFirstOne {
			get {
				if (this.theFirstOne == null)
					this.theFirstOne = new TheFirstOne(this.context.Description, ValidateInput);

				return this.theFirstOne;
			} // get
		} // TheFirstOne

		private ValidateInput ValidateInput {
			get {
				if (this.validateInput == null) {
					this.validateInput = new ValidateInput(
						this.context.Description,
						FinishWizard,
						this.context.CustomerDetails,
						this.context.CashRequestID,
						this.context.CashRequestOriginator
					);
				} // if

				return this.validateInput;
			} // get
		} // ValidateInput

		private FinishWizardIfRequested FinishWizard {
			get {
				if (this.finishWizard == null) {
					this.finishWizard = new FinishWizardIfRequested(
						this.context.Description,
						GatherData,
						this.context.FinishWizardArgs
					);
				} // if

				return this.finishWizard;
			} // get
		} // FinishWizard

		private GatherData GatherData {
			get {
				if (this.gatherData == null) {
					this.gatherData = new GatherData(
						this.context.Description,
						CreateFindCashRequest,
						this.context.CustomerID
					);

					this.gatherData.CollectOutputValue += CollectStepOutputValue;
				} // if

				return this.gatherData;
			} // get
		} // GatherData

		private CreateFindCashRequest CreateFindCashRequest {
			get {
				if (this.createFindCashRequest == null) {
					this.createFindCashRequest = new CreateFindCashRequest(
						this.context.Description,
						ApplyBackdoorLogic,
						this.context.CashRequestID,
						this.context.CashRequestOriginator,
						this.context.CustomerID,
						this.context.CustomerDetails.FullName,
						this.context.CustomerDetails.AppEmail,
						this.context.CustomerDetails.Origin,
						this.context.CustomerDetails.NumOfLoans,
						this.context.UnderwriterID
					);

					this.createFindCashRequest.CollectOutputValue += CollectStepOutputValue;
				} // if

				return this.createFindCashRequest;
			} // get
		} // CreateFindCashRequest

		private ApplyBackdoorLogic ApplyBackdoorLogic {
			get {
				if (this.applyBackdoorLogic == null) {
					this.applyBackdoorLogic = new ApplyBackdoorLogic(
						this.context.Description,
						null, // TODO Save decision
						CheckUpdateDataRequested,
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

					this.applyBackdoorLogic.CollectOutputValue += CollectStepOutputValue;
				} // if

				return this.applyBackdoorLogic;
			} // get
		} // ApplyBackdoorLogic

		private CheckUpdateDataRequested CheckUpdateDataRequested {
			get {
				if (this.checkUpdateDataRequested == null) {
					this.checkUpdateDataRequested = new CheckUpdateDataRequested(
						this.context.Description,
						UpdateData,
						FraudCheck,
						PreventAutoDecision,
						this.context.NewCreditLineOption,
						this.context.CustomerID,
						this.context.MarketplaceUpdateValidityDays
					);

					this.checkUpdateDataRequested.CollectOutputValue += UpdateStrategyContext;
				} // if

				return this.checkUpdateDataRequested;
			} // get
		} // CheckUpdateDataRequested

		private UpdateData UpdateData {
			get {
				if (this.updateData == null) {
					this.updateData = new UpdateData(
						this.context.Description,
						FraudCheck,
						this.context.CustomerID,
						this.context.MarketplaceUpdateValidityDays,
						this.mailer,
						this.context.LogicalGlueEnabled,
						this.context.MonthlyRepayment.MonthlyPayment
					);
				} // if

				return this.updateData;
			} // get
		} // UpdateData

		private FraudCheck FraudCheck {
			get {
				if (this.fraudCheck == null) {
					this.fraudCheck = new FraudCheck(
						this.context.Description,
						UpdateNHibernate,
						this.context.CustomerID,
						this.context.CustomerDetails.IsTest
					);
				} // if

				return this.fraudCheck;
			} // get
		} // FraudCheck

		private UpdateNHibernate UpdateNHibernate {
			get {
				if (this.updateNHibernate == null) {
					this.updateNHibernate = new UpdateNHibernate(
						this.context.Description,
						CheckAutoRulesRequested,
						this.context.CustomerID
					);
				} // if

				return this.updateNHibernate;
			} // get
		} // UpdateNHibernate

		private CheckAutoRulesRequested CheckAutoRulesRequested {
			get {
				if (this.checkAutoRulesRequested == null) {
					this.checkAutoRulesRequested = new CheckAutoRulesRequested(
						this.context.Description,
						Rereject,
						PreventAutoDecision,
						this.context.NewCreditLineOption
					);
				} // if

				return this.checkAutoRulesRequested;
			} // get
		} // CheckAutoRulesRequested

		private LockManual LockManual(AMainStrategyStep nextStep) {
			return new LockManual(this.context.Description, nextStep, this.context.AutoDecisionResponse);
		} // LockManual

		private LockManual PreventAutoDecision {
			get {
				if (this.preventAutoDecision == null)
					this.preventAutoDecision = LockManual(Rereject);

				return this.preventAutoDecision;
			} // get
		} // PreventAutoDecision

		private Rereject Rereject {
			get {
				if (this.rereject == null) {
					this.rereject = new Rereject(
						this.context.Description,
						LockRerejected,
						Reject,
						LockManual(Reject),
						this.context.AvoidAutoDecision,
						this.context.EnableAutomaticReRejection,
						this.context.CustomerID,
						this.context.CashRequestID,
						this.context.NLCashRequestID,
						this.context.Tag
					);
				} // if

				return this.rereject;
			} // get
		} // Rereject

		private LockRerejected LockRerejected {
			get {
				if (this.lockRerejected == null) {
					this.lockRerejected = new LockRerejected(
						this.context.Description,
						Reject,
						this.context.AutoDecisionResponse
					);
				} // if

				return this.lockRerejected;
			} // get
		} // LockRerejected

		private Reject Reject {
			get {
				if (this.reject == null) {
					this.reject = new Reject(
						this.context.Description,
						null, // TODO lock rejected
						null, // TODO land registry
						LockManual(null), // TODO land registry
						this.context.AvoidAutoDecision,
						this.context.EnableAutomaticRejection,
						this.context.CustomerID,
						this.context.CashRequestID,
						this.context.NLCashRequestID,
						this.context.Tag,
						this.context.CompanyID,
						this.context.MonthlyRepayment.MonthlyPayment
					);
				} // if

				return this.reject;
			} // get
		} // Reject

		private readonly MainStrategyContextData context;
		private readonly StrategiesMailer mailer;

		private TheFirstOne theFirstOne;
		private ValidateInput validateInput;
		private FinishWizardIfRequested finishWizard;
		private GatherData gatherData;
		private CreateFindCashRequest createFindCashRequest;
		private ApplyBackdoorLogic applyBackdoorLogic;
		private CheckUpdateDataRequested checkUpdateDataRequested;
		private UpdateData updateData;
		private FraudCheck fraudCheck;
		private UpdateNHibernate updateNHibernate;
		private CheckAutoRulesRequested checkAutoRulesRequested;
		private LockManual preventAutoDecision;
		private Rereject rereject;
		private LockRerejected lockRerejected;
		private Reject reject;
	} // class MainStrategy
} // namespace

