namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.LogicalGlue {
	using System;
	using System.Data;
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
		public virtual RejectionTrail Trail { get; private set; }

		public Agent(int nCustomerID, long? cashRequestID, DateTime? now, AConnection oDB, ASafeLog oLog) {
			DB = oDB;
			Log = oLog.Safe();
			Args = new Arguments(nCustomerID, cashRequestID, now);

			this.oldWayAgent = new Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.Agent(
				nCustomerID,
				cashRequestID,
				oDB,
				oLog
			);
		} // constructor

		public virtual Agent Init() {
			Trail = new RejectionTrail(
				Args.CustomerID,
				Args.CashRequestID,
				Log,
				CurrentValues.Instance.AutomationExplanationMailReciever,
				CurrentValues.Instance.MailSenderEmail,
				CurrentValues.Instance.MailSenderName
			);

			Cfg = InitCfg();

			return this;
		} // Init

		public virtual void MakeAndVerifyDecision(string tag, bool quiet = false) {
			AutomationCalculator.AutoDecision.AutoRejection.LGAgent oSecondary = null;

			try {
				Trail.SetTag(tag);

				RunPrimary();

				oSecondary = RunSecondary();

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
			this.oldWayAgent.RunPrimaryOnly();
			this.oldWayAgent.Trail.SetTag(Trail.Tag);
			this.oldWayAgent.Trail.Save(DB, null, TrailPrimaryStatus.OldPrimary);

			ChooseInternalOrLogicalGlueFlow();

			if (LogicalGlueFlowFollowed) {
				Inference inference = InjectorStub.GetEngine().GetInference(Args.CustomerID, Args.Now, false, 0);

				if (inference == null) {
					Trail.Negative<LGDataFound>(true).Init(false);
					return;
				} else
					Trail.Dunno<LGDataFound>().Init(true);

				bool hasError =
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

				var sp = new OriginSupportsGrade(DB, Log) {
					CustomerID = Args.CustomerID,
					GradeID = (int)inference.Bucket.Value,
				};

				sp.ExecuteNonQuery();

				if (sp.GradeOriginID <= 0) {
					Trail.Affirmative<BucketSupported>(true).Init(false);
					return;
				} else
					Trail.Dunno<BucketSupported>().Init(true);

				Trail.DecideIfNotDecided();
			} else
				Trail.AppendOverridingResults(this.oldWayAgent.Trail);
		} // RunPrimary

		protected virtual Configuration InitCfg() {
			return new Configuration(DB, Log);
		} // InitCfg

		protected virtual AConnection DB { get; private set; }
		protected virtual ASafeLog Log { get; private set; }

		protected virtual Configuration Cfg { get; private set; }
		protected virtual Arguments Args { get; private set; }

		private AutomationCalculator.AutoDecision.AutoRejection.LGAgent RunSecondary() {
			var oSecondary = new AutomationCalculator.AutoDecision.AutoRejection.LGAgent(
					DB,
					Log,
					Trail.Tag,
					Args.CustomerID,
					Trail.InputData.DataAsOf,
					Args.CashRequestID,
					Cfg.Values
				);

			oSecondary.MakeDecision();

			return oSecondary;
		} // RunSecondary

		protected virtual void ChooseInternalOrLogicalGlueFlow() {
			var sp = new GetCustomerCompanyType(DB, Log) {
				CustomerID = Args.CustomerID,
				Now = Args.Now,
			};

			sp.ExecuteNonQuery();

			TypeOfBusiness typeOfBusiness;

			if (!Enum.TryParse(sp.TypeOfBusiness, out typeOfBusiness))
				typeOfBusiness = TypeOfBusiness.Entrepreneur;

			if (typeOfBusiness.IsRegulated())
				Trail.Dunno<InternalFlow>().Init();
			else
				Trail.Dunno<LogicalGlueFlow>().Init();
		} // ChooseInternalOrLogicalGlueFlow

		private readonly Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.Agent oldWayAgent;

		private class GetCustomerCompanyType : AStoredProcedure {
			public GetCustomerCompanyType(AConnection db, ASafeLog log) : base(db, log) {}

			public override bool HasValidParameters() {
				return (CustomerID > 0) && (Now > longAgo);
			} // HasValidParameters

			[UsedImplicitly]
			public int CustomerID { get; set; }

			[UsedImplicitly]
			public DateTime Now { get; set; }

			[Direction(ParameterDirection.Output)]
			[Length(50)]
			public string TypeOfBusiness { get; [UsedImplicitly] set; }

			private static readonly DateTime longAgo = new DateTime(2012, 9, 1, 0, 0, 0, DateTimeKind.Utc);
		} // class GetCustomerCompanyType

		private class OriginSupportsGrade : AStoredProcedure {
			public OriginSupportsGrade(AConnection db, ASafeLog log) : base(db, log) {} 

			public override bool HasValidParameters() {
				return (CustomerID > 0) && (GradeID > 0);
			} // HasValidParameters

			[UsedImplicitly]
			public int CustomerID { get; set; }

			[UsedImplicitly]
			public int GradeID { get; set; }

			[UsedImplicitly]
			[Direction(ParameterDirection.Output)]
			public int GradeOriginID { get; set; }
		} // class OriginSupportsGrade
	} // class Agent
} // namespace
