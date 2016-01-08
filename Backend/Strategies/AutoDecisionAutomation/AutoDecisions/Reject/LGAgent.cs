namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject {
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoRejection;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;

	public class LGAgent : AAutoDecisionBase {
		public virtual RejectionTrail Trail { get; private set; }

		public LGAgent(int nCustomerID, long? cashRequestID, AConnection oDB, ASafeLog oLog) {
			DB = oDB;
			Log = oLog.Safe();
			Args = new Arguments(nCustomerID, cashRequestID);
			HasApprovalChance = false;

			this.oldWayAgent = new Agent(nCustomerID, cashRequestID, oDB, oLog);
		} // constructor

		public virtual LGAgent Init() {
			Trail = new RejectionTrail(
				Args.CustomerID,
				Args.CashRequestID,
				Log,
				CurrentValues.Instance.AutomationExplanationMailReciever,
				CurrentValues.Instance.MailSenderEmail,
				CurrentValues.Instance.MailSenderName
			);

			Now = DateTime.UtcNow;
			Cfg = InitCfg();
			MetaData = new MetaData();
			OriginationTime = new OriginationTime(Log);
			UpdateErrors = new List<MpError>();

			this.annualTurnover = 0;
			this.quarterTurnover = 0;

			return this;
		} // Init

		public bool HasApprovalChance { get; private set; }

		public virtual void MakeDecision(AutoDecisionResponse response, string tag) {
			bool bSuccess = false;

			try {
				bSuccess = MakeAndVerifyDecision(tag);
			} catch (Exception e) {
				Log.Error(e, "Exception during auto rejection.");
				StepNoReject<ExceptionThrown>().Init(e);
			} // try

			if (bSuccess && Trail.HasDecided) {
				response.CreditResult = CreditResultStatus.Rejected;
				response.UserStatus = Status.Rejected;
				response.SystemDecision = SystemDecision.Reject;
				response.DecisionName = "Rejection";
				response.Decision = DecisionActions.Reject;
			} // if
		} // MakeDecision

		public virtual bool MakeAndVerifyDecision(string tag, bool quiet = false) {
			Trail.SetTag(tag);

			RunPrimary();

			AutomationCalculator.AutoDecision.AutoRejection.LGAgent oSecondary = RunSecondary();

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

			WasMismatch = !Trail.EqualsTo(oSecondary.Trail, quiet);

			Trail.Save(DB, oSecondary.Trail);

			return !WasMismatch;
		} // MakeAndVerifyDecision

		protected virtual void RunPrimary() {
			this.oldWayAgent.RunPrimaryOnly();
			this.oldWayAgent.Trail.SetTag(Trail.Tag);
			this.oldWayAgent.Trail.Save(DB, null, TrailPrimaryStatus.OldPrimary);

			bool useOldFlow = true; // TODO: detect from company type

			if (useOldFlow)
				Trail = this.oldWayAgent.Trail;
			else {
				// TODO Logical Glue flow goes here.
			} // if
		} // RunPrimary

		protected virtual Configuration InitCfg() {
			return new Configuration(DB, Log);
		} // InitCfg

		protected virtual DateTime Now { get; set; }

		protected virtual AConnection DB { get; private set; }
		protected virtual ASafeLog Log { get; private set; }

		protected virtual Configuration Cfg { get; private set; }
		protected virtual Arguments Args { get; private set; }
		protected virtual MetaData MetaData { get; private set; }

		protected virtual List<MpError> UpdateErrors { get; private set; }
		protected virtual OriginationTime OriginationTime { get; private set; }

		private AutomationCalculator.AutoDecision.AutoRejection.LGAgent RunSecondary() {
			var oSecondary = new AutomationCalculator.AutoDecision.AutoRejection.LGAgent(
					DB,
					Log,
					Args.CustomerID,
					Trail.InputData.DataAsOf,
					Args.CashRequestID,
					Cfg.Values
				);

			oSecondary.MakeDecision();

			return oSecondary;
		} // RunSecondary

		private T StepNoReject<T>() where T : ATrace {
			return Trail.Negative<T>(true);
		} // StepNoReject

		private decimal quarterTurnover;
		private decimal annualTurnover;
		private readonly Agent oldWayAgent;
	} // class LGAgent
} // namespace
