namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.LogicalGlue {
	using System;
	using System.Data;
	using AutomationCalculator.AutoDecision.AutoRejection;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoRejection;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Logger;
	using Ezbob.Utils.dbutils;
	using EZBob.DatabaseLib.Model.Database;
	using JetBrains.Annotations;

	public class Agent : AAutoDecisionBase {
		public Agent(AutoRejectionArguments args) {
			this.args = args;
			this.args.Log = this.args.Log.Safe();
			Output = new AutoRejectionOutput();

			this.oldWayAgent = new Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.Agent(
				this.args.CustomerID,
				this.args.CashRequestID,
				this.args.NLCashRequestID,
				this.args.DB,
				this.args.Log
			);

			Trail = new RejectionTrail(
				this.args.CustomerID,
				this.args.CashRequestID,
				this.args.NLCashRequestID,
				this.args.Log,
				CurrentValues.Instance.AutomationExplanationMailReciever,
				CurrentValues.Instance.MailSenderEmail,
				CurrentValues.Instance.MailSenderName
			);
		} // constructor

		public virtual AutoRejectionOutput Output { get; private set; }
		public virtual RejectionTrail Trail { get; private set; }

		public virtual void MakeAndVerifyDecision(string tag, bool quiet = false) {
			AutomationCalculator.AutoDecision.AutoRejection.LGAgent oSecondary = null;

			try {
				Trail.SetTag(tag);

				RunPrimary();

				oSecondary = RunSecondary();

				ComparePrimaryAndSecondary(oSecondary);

				WasMismatch = !Trail.EqualsTo(oSecondary.Trail, quiet);
			} catch (Exception e) {
				Log.Error(e, "Exception during auto rejection.");
				Trail.Negative<ExceptionThrown>(true).Init(e);
			} // try

			Trail.Save(DB, oSecondary == null ? null : oSecondary.Trail);
		} // MakeAndVerifyDecision

		public bool LogicalGlueFlowFollowed {
			get { return Trail.FindTrace<LogicalGlueFlow>() != null; }
		} // LogicalGlueFlowFollowed

		protected virtual void RunPrimary() {
			this.oldWayAgent.Init().RunPrimaryOnly();
			this.oldWayAgent.Trail.SetTag(Trail.Tag);
			this.oldWayAgent.Trail.Save(DB, null, TrailPrimaryStatus.OldPrimary);

			var sp = new GetCustomerCompanyType(DB, Log) { CompanyID = this.args.CompanyID, };
			sp.ExecuteNonQuery();

			if (sp.TypeOfBusiness.IsRegulated())
				Trail.Dunno<InternalFlow>().Init();
			else
				Trail.Dunno<LogicalGlueFlow>().Init();

			if (LogicalGlueFlowFollowed)
				LogicalGlueFlow();
			else
				InternalFlow();
		} // RunPrimary

		protected virtual AConnection DB { get { return this.args.DB; } }
		protected virtual ASafeLog Log { get { return this.args.Log; } }

		private AutomationCalculator.AutoDecision.AutoRejection.LGAgent RunSecondary() {
			var oSecondary = new AutomationCalculator.AutoDecision.AutoRejection.LGAgent(this.args);

			oSecondary.MakeDecision();

			return oSecondary;
		} // RunSecondary

		private void LogicalGlueFlow() {
			Output.FlowType = AutoDecisionFlowTypes.LogicalGlue;

			Inference inference = InjectorStub.GetEngine().GetInferenceIfExists(
				this.args.CustomerID,
				this.args.Now,
				false,
				this.args.MonthlyPayment
			);

			if (inference == null) {
				Trail.Negative<LGDataFound>(true).Init(false);
				InternalFlow();
				return;
			} else
				Trail.Dunno<LGDataFound>().Init(true);

			bool hasError =
				(inference.ResponseID <= 0) ||
				(inference.Error == null) || inference.Error.HasError() ||
				(inference.Etl == null) || (inference.Etl.Code == null);

			if (hasError) {
				Trail.Negative<LGWithoutError>(true).Init(false);
				return;
			} else
				Trail.Dunno<LGWithoutError>().Init(true);

			if (inference.Etl.Code == EtlCode.HardReject) {
				Trail.Affirmative<LGHardReject>(true).Init(true);
				return;
			} else
				Trail.Dunno<LGHardReject>().Init(false);

			if (inference.Bucket == null) {
				Trail.Negative<HasBucket>(true).Init(false);
				return;
			} else
				Trail.Dunno<HasBucket>().Init(true);

			/* TODO
			if (less than one configuration found) {
				StepReject<OfferConfigurationFound>(true).Init(0);
				return;
			} else if (more than one configuration found) {
				StepNoDecision<OfferConfigurationFound>().Init(number of found configurations);

				// TODO: append to the log message: customer ID, score, origin,
				// company type, loan source, customer is new/old
				this.log.Alert("Too many configurations found.");

				return;
			} else {
				StepNoDecision<OfferConfigurationFound>().Init(1);

				// TODO: Output.GradeRangeID = ID of found offer configuration
			} // if
			*/

			Trail.DecideIfNotDecided();
		} // LogicalGlueFlow

		private void InternalFlow() {
			Output.FlowType = AutoDecisionFlowTypes.Internal;
			Trail.AppendOverridingResults(this.oldWayAgent.Trail);
		} // InternalFlow

		private void ComparePrimaryAndSecondary(AutomationCalculator.AutoDecision.AutoRejection.LGAgent oSecondary) {
			if (Trail.HasApprovalChance == oSecondary.Trail.HasApprovalChance) {
				Trail.Negative<SameApprovalChance>(false)
					.Init(Trail.HasApprovalChance, oSecondary.Trail.HasApprovalChance);
				oSecondary.Trail.Negative<SameApprovalChance>(false)
					.Init(Trail.HasApprovalChance, oSecondary.Trail.HasApprovalChance);
			} else {
				Trail.Affirmative<SameApprovalChance>(false)
					.Init(Trail.HasApprovalChance, oSecondary.Trail.HasApprovalChance);
				oSecondary.Trail.Affirmative<SameApprovalChance>(false)
					.Init(Trail.HasApprovalChance, oSecondary.Trail.HasApprovalChance);
			} // if

			if (Output.FlowType == oSecondary.Output.FlowType) {
				Trail.Dunno<SameFlowChosen>().Init(Output.FlowType, oSecondary.Output.FlowType);
				oSecondary.Trail.Dunno<SameFlowChosen>().Init(Output.FlowType, oSecondary.Output.FlowType);
			} else {
				Trail.Negative<SameFlowChosen>(true).Init(Output.FlowType, oSecondary.Output.FlowType);
				oSecondary.Trail.Negative<SameFlowChosen>(true).Init(Output.FlowType, oSecondary.Output.FlowType);
			} // if

			if (Output.GradeRangeID == oSecondary.Output.GradeRangeID) {
				Trail.Dunno<SameConfigurationChosen>().Init(Output.GradeRangeID, oSecondary.Output.GradeRangeID);
				oSecondary.Trail.Dunno<SameConfigurationChosen>()
					.Init(Output.GradeRangeID, oSecondary.Output.GradeRangeID);
			} else {
				Trail.Negative<SameConfigurationChosen>(true).Init(Output.GradeRangeID, oSecondary.Output.GradeRangeID);
				oSecondary.Trail.Negative<SameConfigurationChosen>(true)
					.Init(Output.GradeRangeID, oSecondary.Output.GradeRangeID);
			} // if
		} // ComparePrimaryAndSecondary

		private readonly Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.Agent oldWayAgent;
		private readonly AutoRejectionArguments args;

		private class GetCustomerCompanyType : AStoredProcedure {
			public GetCustomerCompanyType(AConnection db, ASafeLog log) : base(db, log) {
				TypeOfBusiness = TypeOfBusiness.Entrepreneur;
			} // constructor

			public override bool HasValidParameters() {
				return (CompanyID > 0);
			} // HasValidParameters

			[UsedImplicitly]
			public int CompanyID { get; set; }

			[UsedImplicitly]
			[Direction(ParameterDirection.Output)]
			[Length(50)]
			public string TypeOfBusinessName {
				get { return TypeOfBusiness.ToString(); }
				set {
					if (string.IsNullOrWhiteSpace(value)) {
						TypeOfBusiness = TypeOfBusiness.Entrepreneur;

						Log.Warn(
							"Type of business not found for company {0}, defaulting to {1}.",
							CompanyID,
							TypeOfBusiness
						);

						return;
					} // if

					TypeOfBusiness tob;

					if (Enum.TryParse(value, false, out tob)) {
						TypeOfBusiness = tob;
						Log.Debug("Type of business for company {0} is {1}.", CompanyID, TypeOfBusiness);
						return;
					} // if

					TypeOfBusiness = TypeOfBusiness.Entrepreneur;

					Log.Warn(
						"Failed to parse type of business for company {0} from '{1}', defaulting to {2}.",
						CompanyID,
						value,
						TypeOfBusiness
					);
				} // set
			} // TypeOfBusinessName

			public TypeOfBusiness TypeOfBusiness { get; private set; }
		} // class GetCustomerCompanyType
	} // class Agent
} // namespace
