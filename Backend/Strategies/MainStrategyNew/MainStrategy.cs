namespace Ezbob.Backend.Strategies.MainStrategyNew {
	using System;
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

			this.steps = new SortedDictionary<string, AMainStrategyStepBase>();

			this.algorithm = new SortedDictionary<StepResult, Func<AMainStrategyStepBase>>();
			InitFSM();
		} // constructor

		public override string Name {
			get { return "Main strategy"; }
		} // Name

		public override void Execute() {
			AMainStrategyStepBase currentStep = TheFirstOne();

			for ( ; ; ) {
				StepResults stepResult = currentStep.Execute();

				if (stepResult == StepResults.StopMachine)
					break;

				var nextStepKey = new StepResult(currentStep.GetType(), stepResult);

				if (!this.algorithm.ContainsKey(nextStepKey)) {
					Log.Alert("Aborted: next step not specified for result {0}.", nextStepKey);
					break;
				} // if

				var nextStepCreator = this.algorithm[nextStepKey];

				if (nextStepCreator == null) {
					Log.Alert("Aborted: next step creator is NULL for result {0}.", nextStepKey);
					break;
				} // if

				currentStep = nextStepCreator();

				if (currentStep == null) {
					Log.Alert("Aborted: failed to create next step for result {0}.", nextStepKey);
					break;
				} // if
			} // while
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

		private AMainStrategyStepBase FindOrCreateStep<T>(Func<T> creator) where T : AMainStrategyStepBase {
			string key = typeof(T).FullName;

			if (this.steps.ContainsKey(key))
				return this.steps[key];

			T step = creator();

			this.steps[key] = step;

			return step;
		} // FindOrCreateStep

		private AMainStrategyStepBase TheFirstOne() {
			return FindOrCreateStep(() => new TheFirstOne(this.context.Description));
		} // TheFirstOne

		private AMainStrategyStepBase ValidateInput() {
			return FindOrCreateStep(() => new ValidateInput(
				this.context.Description,
				this.context.CustomerDetails,
				this.context.CashRequestID,
				this.context.CashRequestOriginator
			));
		} // ValidateInput

		private AMainStrategyStepBase FinishWizard() {
			return FindOrCreateStep(() => new FinishWizardIfRequested(
				this.context.Description,
				this.context.FinishWizardArgs
			));
		} // FinishWizard

		private AMainStrategyStepBase GatherData() {
			return FindOrCreateStep(() => {
				var gatherData = new GatherData(this.context.Description, this.context.CustomerID);
				gatherData.CollectOutputValue += CollectStepOutputValue;
				return gatherData;
			});
		} // GatherData

		private AMainStrategyStepBase CreateFindCashRequest() {
			return FindOrCreateStep(() => {
				var createFindCashRequest = new CreateFindCashRequest(
					this.context.Description,
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
				return createFindCashRequest;
			});
		} // CreateFindCashRequest

		private AMainStrategyStepBase ApplyBackdoorLogic() {
			return FindOrCreateStep(() => {
				var applyBackdoorLogic = new ApplyBackdoorLogic(
					this.context.Description,
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
				return applyBackdoorLogic;
			});
		} // ApplyBackdoorLogic

		private AMainStrategyStepBase CheckUpdateDataRequested() {
			return FindOrCreateStep(() => {
				var checkUpdateDataRequested = new CheckUpdateDataRequested(
					this.context.Description,
					this.context.NewCreditLineOption,
					this.context.CustomerID,
					this.context.MarketplaceUpdateValidityDays
				);
				checkUpdateDataRequested.CollectOutputValue += UpdateStrategyContext;
				return checkUpdateDataRequested;
			});
		} // CheckUpdateDataRequested

		private AMainStrategyStepBase UpdateData() {
			return FindOrCreateStep(() => new UpdateData(
				this.context.Description,
				this.context.CustomerID,
				this.context.MarketplaceUpdateValidityDays,
				this.mailer,
				this.context.LogicalGlueEnabled,
				this.context.MonthlyRepayment.MonthlyPayment
			));
		} // UpdateData

		private AMainStrategyStepBase FraudCheck() {
			return FindOrCreateStep(() => new FraudCheck(
				this.context.Description,
				this.context.CustomerID,
				this.context.CustomerDetails.IsTest
			));
		} // FraudCheck

		private AMainStrategyStepBase UpdateNHibernate() {
			return FindOrCreateStep(() => new UpdateNHibernate(
				this.context.Description,
				this.context.CustomerID
			));
		} // UpdateNHibernate

		private AMainStrategyStepBase CheckAutoRulesRequested() {
			return FindOrCreateStep(() => new CheckAutoRulesRequested(
				this.context.Description,
				this.context.NewCreditLineOption
			));
		} // CheckAutoRulesRequested

		private AMainStrategyStepBase PreventAutoDecision() {
			return FindOrCreateStep(() => new PreventAutoDecision(
				this.context.Description,
				this.context.AutoDecisionResponse
			));
		} // PreventAutoDecision

		private AMainStrategyStepBase Rereject() {
			return FindOrCreateStep(() => new Rereject(
				this.context.Description,
				this.context.AvoidAutoDecision,
				this.context.EnableAutomaticReRejection,
				this.context.CustomerID,
				this.context.CashRequestID,
				this.context.NLCashRequestID,
				this.context.Tag
			));
		} // Rereject

		private AMainStrategyStepBase LockRerejected() {
			return this.FindOrCreateStep(() => new LockRerejected(
				this.context.Description,
				this.context.AutoDecisionResponse
			));
		} // LockRerejected

		private AMainStrategyStepBase LockManualAfterRereject() {
			return FindOrCreateStep(() => new LockManualAfterRereject(
				this.context.Description,
				this.context.AutoDecisionResponse
			));
		} // LockManualAfterRereject

		private AMainStrategyStepBase Reject() {
			return FindOrCreateStep(() => {
				var reject = new Reject(
					this.context.Description,
					this.context.AvoidAutoDecision,
					this.context.EnableAutomaticRejection,
					this.context.CustomerID,
					this.context.CashRequestID,
					this.context.NLCashRequestID,
					this.context.Tag,
					this.context.CompanyID,
					this.context.MonthlyRepayment.MonthlyPayment,
					this.context.CustomerDetails.IsAlibaba
				);
				reject.CollectOutputValue += CollectStepOutputValue;
				return reject;
			});
		} // Reject

		private AMainStrategyStepBase LockRejected() {
			return FindOrCreateStep(() => new LockRejected(
				this.context.Description,
				this.context.AutoDecisionResponse
			));
		} // LockRejected

		private AMainStrategyStepBase LockManualAfterReject() {
			return FindOrCreateStep(() => new LockManualAfterReject(
				this.context.Description,
				this.context.AutoDecisionResponse
			));
		} // LockManualAfterReject

		private AMainStrategyStepBase UpdateLandRegistryData() {
			return FindOrCreateStep(() => new UpdateLandRegistryData(
				this.context.Description,
				this.context.CustomerID,
				this.context.CustomerDetails.FullName,
				this.context.AutoDecisionResponse.DecidedToReject,
				this.context.CustomerDetails.PropertyStatusDescription,
				this.context.CustomerDetails.IsOwnerOfMainAddress,
				this.context.CustomerDetails.IsOwnerOfOtherProperties,
				this.context.NewCreditLineOption
			));
		} // UpdateLandRegistryData

		private void InitFSM() {
			InitTransition<TheFirstOne>(ValidateInput);
			InitTransition<ValidateInput>(FinishWizard);
			InitTransition<FinishWizardIfRequested>(GatherData);
			InitTransition<GatherData>(CreateFindCashRequest);
			InitTransition<CreateFindCashRequest>(ApplyBackdoorLogic);

			InitTransition<ApplyBackdoorLogic>(StepResults.Applied, null); // TODO save decision
			InitTransition<ApplyBackdoorLogic>(StepResults.NotApplied, CheckUpdateDataRequested);

			InitTransition<CheckUpdateDataRequested>(StepResults.Requested, UpdateData);
			InitTransition<CheckUpdateDataRequested>(StepResults.NotRequestedWithAutoRules, FraudCheck);
			InitTransition<CheckUpdateDataRequested>(StepResults.NotRequestedWithoutAutoRules, PreventAutoDecision);

			InitTransition<UpdateData>(FraudCheck);
			InitTransition<FraudCheck>(UpdateNHibernate);
			InitTransition<UpdateNHibernate>(CheckAutoRulesRequested);

			InitTransition<CheckAutoRulesRequested>(StepResults.Requested, Rereject);
			InitTransition<CheckAutoRulesRequested>(StepResults.NotRequested, PreventAutoDecision);

			InitTransition<Rereject>(StepResults.Affirmative, LockRerejected);
			InitTransition<Rereject>(StepResults.Negative, Reject);
			InitTransition<Rereject>(StepResults.Negative, LockManualAfterRereject);

			InitTransition<Reject>(StepResults.Affirmative, LockRejected);
			InitTransition<Reject>(StepResults.Negative, UpdateLandRegistryData);
			InitTransition<Reject>(StepResults.Negative, LockManualAfterReject);

			InitTransition<UpdateLandRegistryData>(null); // TODO calculate offer
		} // InitFSM

		private void InitTransition<T>(Func<AMainStrategyStepBase> createStepFunc) where T : AMainStrategyStepBase {
			InitTransition<T>(StepResults.Completed, createStepFunc);
		} // InitTransition

		private void InitTransition<T>(
			StepResults result,
			Func<AMainStrategyStepBase> createStepFunc
		) where T : AMainStrategyStepBase {
			this.algorithm.Add(new StepResult(typeof(T), result), createStepFunc);
		} // InitTransition

		private readonly MainStrategyContextData context;
		private readonly StrategiesMailer mailer;

		private readonly SortedDictionary<StepResult, Func<AMainStrategyStepBase>> algorithm;
		private readonly SortedDictionary<string, AMainStrategyStepBase> steps;
	} // class MainStrategy
} // namespace

