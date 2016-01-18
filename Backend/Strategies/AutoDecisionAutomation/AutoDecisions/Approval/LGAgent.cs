namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval {
	using System;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;

	public class LGAgent : AAutoDecisionBase, ICreateOfferInputData {
		public LGAgent(
			int customerId,
			long? cashRequestID,
			long? nlCashRequestID,
			string tag,
			DateTime now,
			int offeredCreditLine,
			Medal medalClassification,
			AutomationCalculator.Common.MedalType medalType,
			AutomationCalculator.Common.TurnoverType? turnoverType,
			AConnection db,
			ASafeLog log
		) {
			Now = now;

			DB = db;
			Log = log.Safe();

			this.trail = new ApprovalTrail(
				customerId,
				cashRequestID,
				nlCashRequestID,
				Log,
				CurrentValues.Instance.AutomationExplanationMailReciever,
				CurrentValues.Instance.MailSenderEmail,
				CurrentValues.Instance.MailSenderName
			) {
				Amount = offeredCreditLine,
			};

			Trail.SetTag(tag);

			this.oldWayAgent = new Approval(
				Trail.CustomerID,
				Trail.CashRequestID,
				Trail.NLCashRequestID,
				offeredCreditLine,
				medalClassification,
				medalType,
				turnoverType,
				db,
				log
			);

			this.secondaryAgent = new AutomationCalculator.AutoDecision.AutoApproval.LGAgent(
				Trail.CustomerID,
				Trail.CashRequestID,
				Trail.NLCashRequestID,
				Trail.Tag,
				now,
				offeredCreditLine,
				(AutomationCalculator.Common.Medal)medalClassification,
				medalType,
				turnoverType,
				db,
				log
			);
		} // constructor

		public LGAgent Init() {
			// TODO

			return this;
		} // Init

		public void MakeAndVerifyDecision(string tag, bool quiet = false) {
			try {
				Trail.SetTag(tag);

				RunPrimary();

				this.secondaryAgent.MakeDecision();

				WasMismatch = !Trail.EqualsTo(this.secondaryAgent.Trail, quiet);

				if (!WasMismatch && Trail.HasDecided) {
					if (Trail.RoundedAmount == this.secondaryAgent.Trail.RoundedAmount) {
						Trail.Affirmative<SameAmount>(false).Init(Trail.RoundedAmount);

						this.secondaryAgent.Trail.Affirmative<SameAmount>(false).Init(
							this.secondaryAgent.Trail.RoundedAmount
						);
					} else {
						Trail.Negative<SameAmount>(false).Init(Trail.RoundedAmount);
						this.secondaryAgent.Trail.Negative<SameAmount>(false).Init(this.secondaryAgent.Trail.RoundedAmount);
						WasMismatch = true;
					} // if
				} // if
			} catch (Exception e) {
				Log.Alert(e, "Exception during auto approval.");
				Trail.Negative<ExceptionThrown>(true).Init(e);
			} // try

			Trail.Save(DB, this.secondaryAgent.Trail);
		} // MakeAndVerifyDecision

		public bool ExceptionWhileDeciding {
			get { return Trail.FindTrace<ExceptionThrown>() != null; }
		} // ExceptionWhileDeciding

		public ApprovalTrail Trail {
			get { return this.trail; }
		} // Trail

		public bool LogicalGlueFlowFollowed {
			get { return Trail.FindTrace<LogicalGlueFlow>() != null; }
		} // LogicalGlueFlowFollowed

		public DateTime Now { get; protected set; }

		protected AConnection DB { get; set; }
		protected ASafeLog Log { get; set; }

		protected virtual void RunPrimary() {
			this.oldWayAgent.MakeAndVerifyDecision(Trail.Tag);
			this.oldWayAgent.Trail.Save(DB, null, TrailPrimaryStatus.OldPrimary);

			ChooseInternalOrLogicalGlueFlow();

			if (LogicalGlueFlowFollowed) {
				// TODO Logical Glue flow goes here.
			} else
				Trail.AppendOverridingResults(this.oldWayAgent.Trail);
		} // RunPrimary

		protected virtual void ChooseInternalOrLogicalGlueFlow() {
			if (false) // TODO detect from company type; insert LogicalGlueFlow or InternalFlow into Trail.
				Trail.Dunno<LogicalGlueFlow>().Init();
			else
				Trail.Dunno<InternalFlow>().Init();
		} // ChooseInternalOrLogicalGlueFlow

		private readonly ApprovalTrail trail;
		private readonly Approval oldWayAgent;
		private readonly AutomationCalculator.AutoDecision.AutoApproval.LGAgent secondaryAgent;
	} // class LGAgent
} // namespace
