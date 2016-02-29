namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.LogicalGlue {
	using System;
	using System.Data;
	using System.Linq;
	using System.Net;
	using AutomationCalculator.AutoDecision.AutoRejection;
	using AutomationCalculator.AutoDecision.AutoRejection.Models;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoRejection;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
	using Ezbob.Backend.Strategies.MainStrategy.Helpers;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Logger;
	using Ezbob.Utils.dbutils;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using JetBrains.Annotations;

	using LocalBucket = AutomationCalculator.Common.Bucket;

	public class Agent : AAutoDecisionBase {
		public Agent(AutoRejectionArguments args) {
			this.args = args;
			Output = new AutoRejectionOutput();

			this.oldWayAgent = new Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.Agent(
				this.args.CustomerID,
				this.args.CashRequestID,
				this.args.NLCashRequestID,
				this.args.Tag,
				this.args.DB,
				this.args.Log
			);

			Trail = new LGRejectionTrail(
				this.args.CustomerID,
				this.args.CashRequestID,
				this.args.NLCashRequestID,
				this.args.Log,
				CurrentValues.Instance.AutomationExplanationMailReciever,
				CurrentValues.Instance.MailSenderEmail,
				CurrentValues.Instance.MailSenderName
			);

			CompareTrailsQuietly = false;
		} // constructor

		public virtual AutoRejectionOutput Output { get; private set; }
		public virtual LGRejectionTrail Trail { get; private set; }

		public bool CompareTrailsQuietly { get; set; }

		public override void MakeAndVerifyDecision() {
			Trail.SetTag(this.args.Tag).UniqueID = this.args.TrailUniqueID;

			AutomationCalculator.AutoDecision.AutoRejection.LGAgent oSecondary = null;

			try {
				RunPrimary();

				oSecondary = RunSecondary();

				ComparePrimaryAndSecondary(oSecondary);

				WasMismatch = !Trail.EqualsTo(oSecondary.Trail, CompareTrailsQuietly);
			} catch (Exception e) {
				Log.Error(e, "Exception during auto rejection.");
				Trail.Negative<ExceptionThrown>(true).Init(e);
			} // try

			Trail.Save(DB, oSecondary == null ? null : oSecondary.Trail);
		} // MakeAndVerifyDecision

		public override bool WasException {
			get { return Trail.FindTrace<ExceptionThrown>() != null; }
		} // WasException

		public override bool AffirmativeDecisionMade {
			get { return Trail.HasDecided; }
		} // AffirmativeDecisionMade

		public bool LogicalGlueFlowFollowed {
			get {
				return
					(Trail.FindTrace<LogicalGlueFlow>() != null)
					&&
					(Trail.FindTrace<InternalFlow>() == null);
			} // get
		} // LogicalGlueFlowFollowed

		protected virtual AConnection DB { get { return this.args.DB; } }
		protected virtual ASafeLog Log { get { return this.args.Log; } }

		protected virtual void RunPrimary() {
			using (Trail.AddCheckpoint(ProcessCheckpoints.OldWayFlow)) {
				this.oldWayAgent.Init().RunPrimaryOnly();
				this.oldWayAgent.Trail.UniqueID = this.args.TrailUniqueID;
				this.oldWayAgent.Trail.SetTag(this.args.Tag).Save(DB, null, TrailPrimaryStatus.OldPrimary);
			} // old flow step

			using (Trail.AddCheckpoint(ProcessCheckpoints.GatherData)) {
				var inputData = new LGRejectionInputData();
				inputData.Init(
					this.oldWayAgent.Trail.MyInputData.DataAsOf,
					this.oldWayAgent.Trail.MyInputData,
					this.oldWayAgent.Trail.MyInputData
				);

				GatherData(inputData);

				Trail.MyInputData.Init(inputData.DataAsOf, inputData, inputData);
			} // gather data step

			using (Trail.AddCheckpoint(ProcessCheckpoints.MakeDecision)) {
				if (Trail.MyInputData.AutoDecisionInternalLogic)
					Trail.Dunno<InternalFlow>().Init();
				else
					Trail.Dunno<LogicalGlueFlow>().Init();

				if (LogicalGlueFlowFollowed)
					LogicalGlueFlow();
				else
					InternalFlow();
			} // make decision step
		} // RunPrimary

		private AutomationCalculator.AutoDecision.AutoRejection.LGAgent RunSecondary() {
			var oSecondary = new AutomationCalculator.AutoDecision.AutoRejection.LGAgent(this.args);

			oSecondary.MakeDecision();

			return oSecondary;
		} // RunSecondary

		private void GatherData(LGRejectionInputData inputData) {
			var sp = new LoadLGAutoRejectData(DB, Log) {
				CustomerID = this.args.CustomerID,
				CompanyID = this.args.CompanyID,
				Now = this.args.Now,
			};
			sp.ExecuteNonQuery();

			inputData.CompanyID = this.args.CompanyID;
			inputData.TypeOfBusiness = sp.TypeOfBusiness;
			inputData.CompanyIsRegulated = sp.IsRegulated;
			inputData.AutoDecisionInternalLogic = sp.AutoDecisionInternalLogic;

			inputData.CustomerOrigin = customerOrigins.Contains(sp.OriginID)
				? (CustomerOriginEnum)sp.OriginID
				: (CustomerOriginEnum?)null;

			inputData.LoanSource = loanSources.Contains(sp.LoanSourceID)
				? (LoanSourceName)sp.LoanSourceID
				: (LoanSourceName?)null;

			Inference inference = InjectorStub.GetEngine().GetInferenceIfExists(
				this.args.CustomerID,
				this.args.Now,
				false,
				0
			);

			if (inference == null) {
				inputData.RequestID = null;
				inputData.ResponseID = null;
				inputData.ResponseErrors = null;
				inputData.HardReject = false;
				inputData.Bucket = null;
				inputData.Score = null;
				inputData.MatchingGradeRanges = null;
			} else {
				inputData.RequestID = inference.UniqueID;
				inputData.ResponseID = inference.ResponseID;

				if (inference.ResponseID <= 0)
					inputData.ResponseErrors.Add("No response received.");

				inputData.ResponseHttpStatus = (inference.Status == null) ? (int?)null : (int)inference.Status.HttpStatus;

				if (inference.Error.HasError()) {
					inputData.ResponseErrors.AddRange(
						new [] {
							inference.Error.Message,
							inference.Error.ParsingExceptionType,
							inference.Error.ParsingExceptionMessage,
						}.Where(s => !string.IsNullOrWhiteSpace(s))
					);

					if (inference.Error.TimeoutSource != null)
						inputData.ResponseErrors.Add("Timeout: " + inference.Error.TimeoutSource.Name);

					ModelOutput model = inference.ModelOutputs.ContainsKey(ModelNames.NeuralNetwork)
						? inference.ModelOutputs[ModelNames.NeuralNetwork]
						: null;

					if ((model != null) && !model.Error.IsEmpty) {
						if (!string.IsNullOrWhiteSpace(model.Error.ErrorCode))
							inputData.ResponseErrors.Add(model.Error.ErrorCode);

						if (!string.IsNullOrWhiteSpace(model.Error.Exception))
							inputData.ResponseErrors.Add(model.Error.Exception);

						inputData.ResponseErrors.AddRange(
							model.Error.EncodingFailures.Where(ef => !ef.IsEmpty).Select(ef => ef.ToString())
						);

						inputData.ResponseErrors.AddRange(model.Error.MissingColumns);

						inputData.ResponseErrors.AddRange(
							model.Error.Warnings.Where(w => !w.IsEmpty).Select(w => w.ToString())
						);
					} // if
				} // if inference has error

				inputData.HardReject = (inference.Etl != null) && (inference.Etl.Code == EtlCode.HardReject);
				inputData.Bucket = inference.Bucket == null ? (LocalBucket?)null : (LocalBucket)(int)inference.Bucket;
				inputData.Score = inference.Score;

				inputData.MatchingGradeRanges = new MatchingGradeRanges();

				if (inputData.Score.HasValue && inputData.CustomerOrigin.HasValue && inputData.LoanSource.HasValue) {
					var loader = new LoadOfferRanges(
						this.args.CustomerID,
						this.args.CompanyID,
						this.args.Now,
						DB,
						Log
					).Execute();

					loader.ExportMatchingGradeRanges(inputData.MatchingGradeRanges);
				} // if
			} // if inference is null
		} // GatherData

		private void LogicalGlueFlow() {
			Output.FlowType = AutoDecisionFlowTypes.LogicalGlue;

			if ((Trail.MyInputData.RequestID == null) || (Trail.MyInputData.ResponseHttpStatus != (int)HttpStatusCode.OK)) {
				Trail.Dunno<LGDataFound>().Init(false);
				Trail.Dunno<InternalFlow>().Init();
				InternalFlow();
				return;
			} else
				Trail.Dunno<LGDataFound>().Init(true);

			if (Trail.MyInputData.ResponseErrors.Count > 0) {
				Output.ErrorInLGData = true;
				Trail.Negative<LGWithoutError>(true).Init(false);
			} else {
				Output.ErrorInLGData = false;
				Trail.Dunno<LGWithoutError>().Init(true);
			} // if

			if (Trail.MyInputData.HardReject)
				Trail.Affirmative<LGHardReject>(true).Init(true);
			else
				Trail.Dunno<LGHardReject>().Init(false);

			if (Trail.MyInputData.Bucket == null)
				Trail.Negative<HasBucket>(true).Init(false);
			else
				Trail.Dunno<HasBucket>().Init(true);

			if (Trail.MyInputData.MatchingGradeRanges.Count < 1)
				Trail.Affirmative<OfferConfigurationFound>(true).Init(0);
			else if (Trail.MyInputData.MatchingGradeRanges.Count > 1) {
				Trail.Negative<OfferConfigurationFound>(true).Init(Trail.MyInputData.MatchingGradeRanges.Count);

				Log.Alert(
					"Too many grade range + product subtype pairs found for a {0} customer {1}, " +
					"score {2}, origin {3}, company is {4}regulated, loan source {5}.",
					Trail.MyInputData.LoanCount > 0 ? "returning" : "new",
					this.args.CustomerID,
					Trail.MyInputData.Score == null ? "'N/A'" : Trail.MyInputData.Score.ToString(),
					Trail.MyInputData.CustomerOrigin == null ? "'N/A'" : Trail.MyInputData.CustomerOrigin.Value.ToString(),
					Trail.MyInputData.CompanyIsRegulated ? string.Empty : "non-",
					Trail.MyInputData.LoanSource == null ? "'N/A'" : Trail.MyInputData.LoanSource.Value.ToString()
				);
			} else {
				Trail.Dunno<OfferConfigurationFound>().Init(1);

				MatchingGradeRanges.SubproductGradeRange spgr = Trail.MyInputData.MatchingGradeRanges[0];

				Output.GradeRangeID = spgr.GradeRangeID;
				Output.ProductSubTypeID = spgr.ProductSubTypeID;
			} // if

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

		private class LoadLGAutoRejectData : AStoredProcedure {
			public LoadLGAutoRejectData(AConnection db, ASafeLog log) : base(db, log) {
				TypeOfBusiness = TypeOfBusiness.Entrepreneur;
				IsRegulated = true;
				AutoDecisionInternalLogic = true;
			} // constructor

			public override bool HasValidParameters() {
				return (CustomerID > 0) && (CompanyID > 0) && (Now >= longAgo);
			} // HasValidParameters

			[UsedImplicitly]
			public DateTime Now { get; set; }

			[UsedImplicitly]
			public int CustomerID { get; set; }

			[UsedImplicitly]
			public int CompanyID { get; set; }

			[UsedImplicitly]
			[Direction(ParameterDirection.Output)]
			public int OriginID { get; set; }

			[UsedImplicitly]
			[Direction(ParameterDirection.Output)]
			public int LoanSourceID { get; set; }

			[UsedImplicitly]
			[Direction(ParameterDirection.Output)]
			public int LoanCount { get; set; }

			[UsedImplicitly]
			[Direction(ParameterDirection.Output)]
			public bool IsRegulated { get; set; }

			[UsedImplicitly]
			[Direction(ParameterDirection.Output)]
			public bool AutoDecisionInternalLogic { get; set; }

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
					IsRegulated = true;
					AutoDecisionInternalLogic = true;

					Log.Warn(
						"Failed to parse type of business for company {0} from '{1}', defaulting to {2}.",
						CompanyID,
						value,
						TypeOfBusiness
					);
				} // set
			} // TypeOfBusinessName

			public TypeOfBusiness TypeOfBusiness { get; private set; }
		} // class LoadLGAutoRejectData

		private static readonly int[] customerOrigins =
			((CustomerOriginEnum[])Enum.GetValues(typeof(CustomerOriginEnum))).Select(x => (int)x).ToArray();

		private static readonly int[] loanSources =
			((LoanSourceName[])Enum.GetValues(typeof(LoanSourceName))).Select(x => (int)x).ToArray();

		private static readonly DateTime longAgo = new DateTime(2012, 9, 1, 0, 0, 0, DateTimeKind.Utc);
	} // class Agent
} // namespace
