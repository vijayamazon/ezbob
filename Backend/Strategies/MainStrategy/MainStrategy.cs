namespace Ezbob.Backend.Strategies.MainStrategy {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Reflection;
	using AutomationCalculator.Common;
	using DbConstants;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.Exceptions;
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using Ezbob.Backend.Strategies.MainStrategy.Steps;

	[SuppressMessage("ReSharper", "ConvertIfStatementToNullCoalescingExpression")]
	public class MainStrategy : AStrategy {
		public MainStrategy(MainStrategyArguments args) {
			if (args == null)
				throw new StrategyAlert(this, "No arguments specified for the main strategy.");

			this.context = new MainStrategyContextData(args);
			this.mailer = new StrategiesMailer();

			this.steps = new SortedDictionary<string, AMainStrategyStepBase>();

			this.transitions = new SortedDictionary<StepResult, Func<AMainStrategyStepBase>>();
			InitMachineTransitions();

			UpdateStrategyContext();
		} // constructor

		public override string Name {
			get { return "Main strategy"; }
		} // Name

		public override void Execute() {
			AMainStrategyStepBase currentStep = TheFirstOne();

			var stepHistory = new List<StepHistoryItem>();

			for ( ; ; ) {
				var historyItem = new StepHistoryItem(currentStep.Name);
				stepHistory.Add(historyItem);

				this.context.CurrentStepName = currentStep.Name;
				UpdateStrategyContext();

				StepResults stepResult = currentStep.Execute();

				historyItem.SetResult(stepResult, currentStep.Outcome);

				if (stepResult == StepResults.NormalShutdown)
					break;

				StepResult nextStepKey;

				if (stepResult == StepResults.AbnormalShutdown) {
					if (this.context.ShuttingDownUbnormally) {
						historyItem.Message =
							"Something went terribly wrong: " +
							"abnormal shutdown request while handling previous abnormal shutdown.";

						Log.Alert("{0}", historyItem.Message);

						break;
					} // if

					this.context.ShuttingDownUbnormally = true;

					nextStepKey = OnAbnormalShutdown();
				} else
					nextStepKey = new StepResult(currentStep.GetType(), stepResult);

				historyItem.NextStepKey = nextStepKey;

				if (!this.transitions.ContainsKey(nextStepKey)) {
					historyItem.Message = string.Format("Aborted: next step not specified for result {0}.", nextStepKey);
					Log.Alert("{0}", historyItem.Message);
					break;
				} // if

				var nextStepCreator = this.transitions[nextStepKey];

				if (nextStepCreator == null) {
					historyItem.Message = string.Format("Aborted: next step creator is NULL for result {0}.", nextStepKey);
					Log.Alert("{0}", historyItem.Message);
					break;
				} // if

				currentStep = nextStepCreator();

				if (currentStep == null) {
					historyItem.Message = string.Format("Aborted: failed to create next step for result {0}.", nextStepKey);
					Log.Alert("{0}", historyItem.Message);
					break;
				} // if

				historyItem.NextStepName = currentStep.Name;
			} // while

			Log.Debug("Trace for {0}:\n\t{1}\n", this.context.Description, string.Join("\n\t", stepHistory));
		} // Execute

		private StepResult OnAbnormalShutdown() {
			Type handlerStepType;

			if (this.context.CashRequestWasWritten)
				handlerStepType = typeof(AbnormalShutdownAfterCashRequestWasWritten);
			else {
				handlerStepType = this.context.HasCashRequest
					? typeof(AbnormalShutdownAfterHavingCashRequest)
					: typeof(AbnormalShutdownBeforeHavingCashRequest);
			} // if

			return new StepResult(handlerStepType, StepResults.Success);
		} // OnAbnormalShutdown

		private void UpdateStrategyContext() {
			Context.Description = string.Format(
				"Current step is {0}.{1}",
				this.context.CurrentStepName,
				this.context.DelayReason
			);
		} // UpdateStrategyContext

		private void UpdateDelayReason(string propertyName, object propertyValue) {
			var mpNamesToUpdate = propertyValue as List<string>;

			if ((mpNamesToUpdate == null) || (mpNamesToUpdate.Count < 1))
				return;

			this.context.DelayReason = string.Format(
				" This strategy can take long time (updating {0}).",
				string.Join(", ", mpNamesToUpdate)
			);

			UpdateStrategyContext();
		} // UpdateDelayReason

		private void CollectStepOutputValue(string propertyName, object propertyValue) {
			PropertyInfo pi = this.context.GetType().GetProperty(propertyName);

			if (pi.PropertyType == typeof(AutoDecisionResponse))
				this.context.AutoDecisionResponse.CopyFrom((AutoDecisionResponse)propertyValue);
			else {
				pi.SetValue(this.context, propertyValue);

				Log.Debug(
					"Collected value of {0} is {1}",
					propertyName,
					propertyValue == null ? "null" : propertyValue.ToString()
				);
			} // if
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
					this.context.Tag,
					this.context.MaxCapHomeOwner,
					this.context.MaxCapNotHomeOwner,
					this.context.SmallLoanScenarioLimit,
					this.context.AspireToMinSetupFee,
					this.context.TypeOfBusiness,
					this.context.CustomerDetails.OriginID,
					this.context.MonthlyRepayment
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
				checkUpdateDataRequested.CollectOutputValue += UpdateDelayReason;
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
				this.context.AutoRejectionOutput == null
					? AutoDecisionFlowTypes.Unknown
					: this.context.AutoRejectionOutput.FlowType,
				this.context.CustomerDetails.IsOwnerOfMainAddress,
				this.context.CustomerDetails.IsOwnerOfOtherProperties,
				this.context.NewCreditLineOption,
				this.context.CustomerDetails.IsTest,
				this.context.AvoidAutoDecision,
				this.context.AutoDecisionResponse.Decision == DecisionActions.ReApprove
			));
		} // UpdateLandRegistryData

		private AMainStrategyStepBase CalculateOfferIfPossible() {
			return FindOrCreateStep(() => {
				var step = new CalculateOfferIfPossible(
					this.context.Description,
					this.context.CustomerID,
					this.context.CashRequestID,
					this.context.NLCashRequestID,
					this.context.Tag,
					this.context.AutoRejectionOutput,
					this.context.MonthlyRepayment,
					this.context.MaxCapHomeOwner,
					this.context.MaxCapNotHomeOwner,
					this.context.SmallLoanScenarioLimit,
					this.context.AspireToMinSetupFee
				);
				step.CollectOutputValue += CollectStepOutputValue;
				return step;
			});
		} // CalculateOfferIfPossible

		private AMainStrategyStepBase LockManualAfterOffer() {
			return FindOrCreateStep(() => new LockManualAfterOffer(
				this.context.Description,
				this.context.AutoDecisionResponse
			));
		} // LockManualAfterOffer

		private AMainStrategyStepBase Reapproval() {
			return FindOrCreateStep(() => {
				var step = new Reapproval(
					this.context.Description,
					this.context.AvoidAutoDecision,
					this.context.EnableAutomaticReApproval,
					this.context.CustomerID,
					this.context.CashRequestID,
					this.context.NLCashRequestID,
					this.context.Tag,
					this.context.CustomerDetails.CustomerStatusIsEnabled,
					this.context.CustomerDetails.CustomerStatusIsWarning,
					this.context.EnableAutomaticRejection,
					this.context.EnableAutomaticReRejection
				);
				step.CollectOutputValue += CollectStepOutputValue;
				return step;
			});
		} // Reapproval

		private AMainStrategyStepBase LockManualAfterReapproval() {
			return FindOrCreateStep(() => new LockManualAfterReapproval(
				this.context.Description,
				this.context.AutoDecisionResponse
			));
		} // LockManualAfterReapproval

		private AMainStrategyStepBase LockReapproved() {
			return FindOrCreateStep(() => new LockReapproved(
				this.context.Description,
				this.context.AutoDecisionResponse,
				this.context.AutoReapprovalOutput
			));
		} // LockReapproved

		private AMainStrategyStepBase Approval() {
			return FindOrCreateStep(() => {
				var step = new Approval(
					this.context.Description,
					this.context.AvoidAutoDecision,
					this.context.EnableAutomaticApproval,
					this.context.CustomerID,
					this.context.CashRequestID,
					this.context.NLCashRequestID,
					this.context.Tag,
					this.context.CustomerDetails.CustomerStatusIsEnabled,
					this.context.CustomerDetails.CustomerStatusIsWarning,
					this.context.EnableAutomaticRejection,
					this.context.EnableAutomaticReRejection,
					this.context.AutoDecisionResponse.ProposedAmount,
					this.context.Medal,
					this.context.AutoRejectionOutput,
					this.context.CustomerDetails.IsAlibaba
				);
				step.CollectOutputValue += CollectStepOutputValue;
				return step;
			});
		} // Approval

		private AMainStrategyStepBase LockManualAfterApproval() {
			return FindOrCreateStep(() => new LockManualAfterApproval(
				this.context.Description,
				this.context.AutoDecisionResponse
			));
		} // LockManualAfterApproval

		private AMainStrategyStepBase LockApproved() {
			return FindOrCreateStep(() => {
				var step = new LockApproved(
					this.context.Description,
					this.context.AutoDecisionResponse,
					this.context.AutoApproveIsSilent,
					this.context.OfferResult,
					this.context.LoanSourceID,
					this.context.LoanOfferEmailSendingBannedNew,
					this.context.OfferValidForHours,
					this.context.MinLoanAmount,
					this.context.MaxLoanAmount,
					(this.context.AutoRejectionOutput == null) || (this.context.AutoRejectionOutput.ProductSubTypeID <= 0)
						? (int?)null
						: this.context.AutoRejectionOutput.ProductSubTypeID
				);
				step.CollectOutputValue += CollectStepOutputValue;
				return step;
			});
		} // LockManualAfterApproval

		private AMainStrategyStepBase ManualIfNotDecided() {
			return FindOrCreateStep(() => new ManualIfNotDecided(
				this.context.Description,
				this.context.AutoDecisionResponse
			));
		} // ManualIfNotDecided

		private AMainStrategyStepBase LookForInvestor() {
			return FindOrCreateStep(() => new LookForInvestor(
				this.context.Description,
				this.context.CustomerID,
				this.context.CashRequestID,
				this.context.UnderwriterID,
				this.context.BackdoorLogicApplied,
				this.context.BackdoorInvestorID
			));
		} // LookForInvestor

		private AMainStrategyStepBase RestoreAndSaveApproved() {
			return FindOrCreateStep(() => new RestoreAndSaveApproved(
				this.context.Description,
				this.context.UnderwriterID,
				this.context.CustomerID,
				this.context.CashRequestID,
				this.context.NLCashRequestID,
				this.context.OverrideApprovedRejected,
				this.context.OfferValidForHours,
				this.context.AutoDecisionResponse,
				this.context.WriteDecisionOutput
			));
		} // RestoreAndSaveApproved

		private AMainStrategyStepBase WriteDecisionDown() {
			return FindOrCreateStep(() => {
				var step = new WriteDecisionDown(
					this.context.Description,
					this.context.UnderwriterID,
					this.context.CustomerID,
					this.context.CashRequestID,
					this.context.NLCashRequestID,
					this.context.OfferValidForHours,
					this.context.AutoDecisionResponse,
					this.context.Medal,
					this.context.OverrideApprovedRejected,
					this.context.CustomerDetails.ExperianConsumerScore
				);
				step.CollectOutputValue += CollectStepOutputValue;
				return step;
			});
		} // WriteDecisionDown

		private AMainStrategyStepBase DispatchNotifications() {
			return FindOrCreateStep(() => new DispatchNotifications(
				this.context.Description,
				this.context.IsSilentlyApproved,
				this.mailer,
				this.context.Medal,
				this.context.CustomerDetails,
				this.context.AutoDecisionResponse,
				this.context.OfferResult,
				this.context.AutoApprovalTrailUniqueID,
				this.context.SilentEmailRecipient,
				this.context.SilentEmailSenderName,
				this.context.SilentEmailSenderAddress
			));
		} // DispatchNotifications

		private AMainStrategyStepBase TheLastOne() {
			return FindOrCreateStep(() => new TheLastOne(this.context.Description));
		} // TheLastOne

		private AMainStrategyStepBase ForceManual() {
			return FindOrCreateStep(() => new ForceManual(
				this.context.Description,
				this.context.AutoDecisionResponse
			));
		} // ForceManual

		private void InitMachineTransitions() {
			InitTransition<TheFirstOne>().Always(ValidateInput);
			InitTransition<ValidateInput>().Always(FinishWizard);
			InitTransition<FinishWizardIfRequested>().Always(GatherData);
			InitTransition<GatherData>().Always(CreateFindCashRequest);
			InitTransition<CreateFindCashRequest>().Always(ApplyBackdoorLogic);

			InitTransition<ApplyBackdoorLogic>()
				.OnResults(WriteDecisionDown, StepResults.Applied)
				.OnResults(CheckUpdateDataRequested, StepResults.NotApplied);

			InitTransition<CheckUpdateDataRequested>()
				.OnResults(UpdateData, StepResults.Requested)
				.OnResults(FraudCheck, StepResults.NotRequestedWithAutoRules)
				.OnResults(PreventAutoDecision, StepResults.NotRequestedWithoutAutoRules);

			InitTransition<UpdateData>().Always(FraudCheck);
			InitTransition<FraudCheck>().Always(UpdateNHibernate);
			InitTransition<UpdateNHibernate>().Always(CheckAutoRulesRequested);

			InitTransition<CheckAutoRulesRequested>()
				.OnResults(Rereject, StepResults.Requested)
				.OnResults(PreventAutoDecision, StepResults.NotRequested);

			InitTransition<PreventAutoDecision>().Always(Rereject);

			InitTransition<Rereject>()
				.OnResults(LockRerejected, StepResults.Affirmative)
				.OnResults(Reject, StepResults.Negative)
				.OnResults(LockManualAfterRereject, StepResults.Failed);

			InitTransition<LockRerejected>().Always(Reject);
			InitTransition<LockManualAfterRereject>().Always(Reject);

			InitTransition<Reject>()
				.OnResults(LockRejected, StepResults.Affirmative)
				.OnResults(Reapproval, StepResults.Negative)
				.OnResults(LockManualAfterReject, StepResults.Failed);

			InitTransition<LockRejected>().Always(Reapproval);
			InitTransition<LockManualAfterReject>().Always(Reapproval);

			InitTransition<Reapproval>()
				.OnResults(LockReapproved, StepResults.Affirmative)
				.OnResults(UpdateLandRegistryData, StepResults.Negative)
				.OnResults(LockManualAfterReapproval, StepResults.Failed);

			InitTransition<LockReapproved>()
				.OnResults(UpdateLandRegistryData, StepResults.Success)
				.OnResults(LockManualAfterReapproval, StepResults.Failed);

			InitTransition<LockManualAfterReapproval>().Always(UpdateLandRegistryData);

			InitTransition<UpdateLandRegistryData>().Always(CalculateOfferIfPossible);

			InitTransition<CalculateOfferIfPossible>()
				.OnResults(Approval, StepResults.Success)
				.OnResults(LockManualAfterOffer, StepResults.Failed);

			InitTransition<LockManualAfterOffer>().Always(Approval);

			InitTransition<Approval>()
				.OnResults(LockApproved, StepResults.Affirmative)
				.OnResults(LockManualAfterApproval, StepResults.Negative, StepResults.Failed);

			InitTransition<LockApproved>()
				.OnResults(ManualIfNotDecided, StepResults.Success)
				.OnResults(LockManualAfterApproval, StepResults.Failed);

			InitTransition<LockManualAfterApproval>().Always(ManualIfNotDecided);

			InitTransition<ManualIfNotDecided>().Always(WriteDecisionDown);

			InitTransition<WriteDecisionDown>()
				.OnResults(TheLastOne, StepResults.Failed)
				.OnResults(DispatchNotifications, StepResults.RejectedManual)
				.OnResults(LookForInvestor, StepResults.Approved);

			InitTransition<LookForInvestor>()
				.OnResults(RestoreAndSaveApproved, StepResults.Found)
				.OnResults(DispatchNotifications, StepResults.NotFound);

			InitTransition<RestoreAndSaveApproved>().Always(DispatchNotifications);

			InitTransition<DispatchNotifications>().Always(TheLastOne);

			InitTransition<AbnormalShutdownBeforeHavingCashRequest>().Always(TheLastOne);
			InitTransition<AbnormalShutdownAfterHavingCashRequest>().Always(ForceManual);
			InitTransition<AbnormalShutdownAfterCashRequestWasWritten>().Always(DispatchNotifications);
			InitTransition<ForceManual>().Always(WriteDecisionDown);
		} // InitMachineTransitions

		private MachineTransition<T> InitTransition<T>() where T : AMainStrategyStepBase {
			return new MachineTransition<T>(this.transitions);
		} // InitTransition

		private readonly MainStrategyContextData context;
		private readonly StrategiesMailer mailer;

		private readonly SortedDictionary<StepResult, Func<AMainStrategyStepBase>> transitions;
		private readonly SortedDictionary<string, AMainStrategyStepBase> steps;

		private class MachineTransition<T> where T : AMainStrategyStepBase {
			public MachineTransition(SortedDictionary<StepResult, Func<AMainStrategyStepBase>> transitions) {
				this.transitions = transitions;
			} // constructor

			public void Always(Func<AMainStrategyStepBase> createStepFunc) {
				OnResults(createStepFunc, StepResults.Success);
			} // Always

			public MachineTransition<T> OnResults(Func<AMainStrategyStepBase> createStepFunc, params StepResults[] results) {
				foreach (StepResults result in results)
					this.transitions.Add(new StepResult(typeof(T), result), createStepFunc);

				return this;
			} // OnResults

			private readonly SortedDictionary<StepResult, Func<AMainStrategyStepBase>> transitions;
		} // class MachineTransition
	} // class MainStrategy
} // namespace

