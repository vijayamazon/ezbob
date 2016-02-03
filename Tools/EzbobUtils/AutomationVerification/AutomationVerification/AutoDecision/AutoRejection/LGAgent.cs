namespace AutomationCalculator.AutoDecision.AutoRejection {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using AutomationCalculator.AutoDecision.AutoRejection.Models;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoRejection;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Lingvo;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;

	public class LGAgent {
		public LGAgent(AutoRejectionArguments args) {
			this.args = args;

			Trail = new LGRejectionTrail(
				this.args.CustomerID,
				this.args.CashRequestID,
				this.args.NLCashRequestID,
				this.args.Log
			);
			Trail.SetTag(this.args.Tag).UniqueID = this.args.TrailUniqueID;

			this.oldWayAgent = new RejectionAgent(
				DB,
				Log,
				this.args.CustomerID,
				Trail.CashRequestID,
				this.args.NLCashRequestID
			);
			this.oldWayAgent.Trail.SetTag(this.args.Tag).UniqueID = this.args.TrailUniqueID;

			Output = new AutoRejectionOutput();
		} // constructor

		public AutoRejectionOutput Output { get; private set; }

		public LGRejectionTrail Trail { get; private set; }

		public void MakeDecision() {
			using (Trail.AddCheckpoint(ProcessCheckpoints.OldWayFlow)) {
				this.oldWayAgent.MakeDecision(this.oldWayAgent.GetRejectionInputData(this.args.Now));
				this.oldWayAgent.Trail.Save(DB, null, TrailPrimaryStatus.OldVerification);
			} // old way step

			using (Trail.AddCheckpoint(ProcessCheckpoints.GatherData))
				GatherData();

			using (Trail.AddCheckpoint(ProcessCheckpoints.MakeDecision)) {
				Log.Debug("Secondary LG: checking auto reject for customer {0}...", this.args.CustomerID);

				if (Trail.MyInputData.CompanyIsRegulated) {
					Output.FlowType = AutoDecisionFlowTypes.Internal;
					StepNoDecision<InternalFlow>().Init();
					Trail.AppendOverridingResults(this.oldWayAgent.Trail);
					return;
				} else {
					Output.FlowType = AutoDecisionFlowTypes.LogicalGlue;
					StepNoDecision<LogicalGlueFlow>().Init();
				} // if

				bool noLgData =
					(Trail.MyInputData.RequestID == null) ||
					(Trail.MyInputData.ResponseHttpStatus != (int)HttpStatusCode.OK);

				if (noLgData) {
					StepNoDecision<LGDataFound>().Init(false);
					StepNoDecision<InternalFlow>().Init();
					Trail.AppendOverridingResults(this.oldWayAgent.Trail);
					return;
				} else
					StepNoDecision<LGDataFound>().Init(true);

				if (Trail.MyInputData.ResponseErrors.Count > 0) {
					Output.ErrorInLGData = true;
					StepNoReject<LGWithoutError>(true).Init(false);
				} else {
					Output.ErrorInLGData = false;
					StepNoDecision<LGWithoutError>().Init(true);
				} // if

				if (Trail.MyInputData.HardReject)
					StepReject<LGHardReject>(true).Init(true);
				else
					StepNoDecision<LGHardReject>().Init(false);

				if (Trail.MyInputData.Bucket == null)
					StepNoReject<HasBucket>(true).Init(false);
				else
					StepNoDecision<HasBucket>().Init(true);

				int rangesCount = Trail.MyInputData.MatchingGradeRanges.Count;

				if (rangesCount < 1) {
					StepReject<OfferConfigurationFound>(true).Init(0);
					return;
				} // if

				if (rangesCount > 1) {
					StepNoReject<OfferConfigurationFound>(true).Init(rangesCount);

					Log.Alert(
						"Too many configurations found for a {0} customer {1}, " +
						"score {2}, origin {3}, company is {4}regulated, loan source {5}.",
						Trail.MyInputData.LoanCount > 0 ? "returning" : "new",
						this.args.CustomerID,
						Trail.MyInputData.Score == null ? "'N/A'" : Trail.MyInputData.Score.ToString(),
						Trail.MyInputData.CustomerOrigin == null
							? "'N/A'"
							: Trail.MyInputData.CustomerOrigin.Value.ToString(),
						Trail.MyInputData.CompanyIsRegulated ? string.Empty : "non-",
						Trail.MyInputData.LoanSource == null ? "'N/A'" : Trail.MyInputData.LoanSource.Value.ToString()
					);

					return;
				} // if

				StepNoDecision<OfferConfigurationFound>().Init(1);

				MatchingGradeRanges.SubproductGradeRange spgr = Trail.MyInputData.MatchingGradeRanges.First();

				Output.GradeRangeID = spgr.GradeRangeID;
				Output.ProductSubTypeID = spgr.ProductSubTypeID;

				Trail.DecideIfNotDecided();
			} // using
		} // MakeDecision

		private void GatherData() {
			var inputData = new LGRejectionInputData();
			inputData.Init(
				this.oldWayAgent.Trail.MyInputData.DataAsOf,
				this.oldWayAgent.Trail.MyInputData,
				this.oldWayAgent.Trail.MyInputData
			);

			var sr = DB.GetFirst(
				"AV_LogicalGlueDataForAutoReject", CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", this.args.CustomerID),
				new QueryParameter("CompanyID", this.args.CompanyID),
				new QueryParameter("ProcessingDate", this.args.Now)
			);

			inputData.CompanyID = this.args.CompanyID;

			TypeOfBusiness typeOfBusiness;
			inputData.TypeOfBusiness = Enum.TryParse(sr["TypeOfBusiness"], out typeOfBusiness)
				? typeOfBusiness
				: EZBob.DatabaseLib.Model.Database.TypeOfBusiness.Entrepreneur;
			
			inputData.CompanyIsRegulated = inputData.TypeOfBusiness.IsRegulated();
			inputData.LoanCount = sr["LoanCount"];

			CustomerOriginEnum coe;
			inputData.CustomerOrigin = Enum.TryParse(sr["CustomerOrigin"], out coe) ? coe : (CustomerOriginEnum?)null;

			LoanSourceName ls;
			inputData.LoanSource = Enum.TryParse(sr["LoanSource"], out ls) ? ls : (LoanSourceName?)null;

			inputData.RequestID = sr["RequestID"];
			inputData.ResponseID = sr["ResponseID"];

			inputData.ResponseErrors = new List<string>();

			if (inputData.ResponseID <= 0)
				inputData.ResponseErrors.Add("No response received.");

			inputData.ResponseHttpStatus = sr["HttpStatus"];

			AddError(inputData.ResponseErrors, sr["ErrorMessage"]);
			AddError(inputData.ResponseErrors, sr["ParsingExceptionType"]);
			AddError(inputData.ResponseErrors, sr["ParsingExceptionMessage"]);

			long? timeoutSourceID = sr["TimeoutSourceID"];

			if (timeoutSourceID != null) {
				AddError(
					inputData.ResponseErrors,
					"Timeout: " + Enum.GetNames(typeof(LGTimeoutSources))[timeoutSourceID.Value - 1]
				);
			} // if

			if ((long?)sr["ModelOutputID"] == null)
				inputData.ResponseErrors.Add("Neural network model not found.");

			AddError(inputData.ResponseErrors, sr["ErrorCode"]);
			AddError(inputData.ResponseErrors, sr["Exception"]);

			int errCount = sr["EncodingFailureCount"];

			if (errCount > 0)
				inputData.ResponseErrors.Add(Grammar.Number(errCount, "encoding failure") + " detected.");

			errCount = sr["MissingColumnCount"];

			if (errCount > 0)
				inputData.ResponseErrors.Add(Grammar.Number(errCount, "missing column") + " detected.");

			errCount = sr["WarningCount"];

			if (errCount > 0)
				inputData.ResponseErrors.Add(Grammar.Number(errCount, "warning") + " detected.");

			inputData.HardReject = sr["EtlCodeID"] == (int)LGEtlCode.HardReject;

			Bucket bucket;
			inputData.Bucket = Enum.TryParse(sr["Grade"], out bucket) ? bucket : (Bucket?)null;

			inputData.Score = sr["Score"];

			inputData.MatchingGradeRanges = new MatchingGradeRanges();

			if (inputData.Score != null) {
				DB.ForEachRowSafe(
					r => inputData.MatchingGradeRanges.Add(new MatchingGradeRanges.SubproductGradeRange {
						ProductSubTypeID = sr["ProductSubTypeID"],
						GradeRangeID = r["GradeRangeID"],
					}),
					"AV_LoadMatchingGradeRanges",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", this.args.CustomerID),
					new QueryParameter("Score", inputData.Score.Value),
					new QueryParameter("Regulated", inputData.CompanyIsRegulated),
					new QueryParameter("ProcessingDate", this.args.Now)
				);
			} // if

			Trail.MyInputData.Init(inputData.DataAsOf, inputData, inputData);

			Log.Debug(
				"Customer {0} has company {1} of type {3} data at {2:d}",
				this.args.CustomerID,
				this.args.CompanyID,
				this.args.Now,
				inputData.TypeOfBusiness
			);
		} // GatherData

		private static void AddError(List<string> errList, string errorMsg) {
			if (string.IsNullOrWhiteSpace(errorMsg))
				return;

			errList.Add(errorMsg);
		} // AddError

		private T StepReject<T>(bool bLockDecisionAfterAddingAStep) where T : ATrace {
			return Trail.Affirmative<T>(bLockDecisionAfterAddingAStep);
		} // StepReject

		private T StepNoReject<T>(bool bLockDecisionAfterAddingAStep) where T : ATrace {
			return Trail.Negative<T>(bLockDecisionAfterAddingAStep);
		} // StepNoReject

		private T StepNoDecision<T>() where T : ATrace {
			return Trail.Dunno<T>();
		} // StepNoDecision

		private readonly AutoRejectionArguments args;
		private readonly RejectionAgent oldWayAgent;
		private AConnection DB { get { return this.args.DB; } }
		private ASafeLog Log { get { return this.args.Log; } }
	} // class LGAgent
} // namespace
