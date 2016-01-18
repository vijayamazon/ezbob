namespace AutomationCalculator.AutoDecision.AutoRejection {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoRejection;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;

	public class LGAgent {
		public LGAgent(AutoRejectionArguments argss) {
			args = argss;

			Trail = new RejectionTrail(args.CustomerID, args.CashRequestID, args.NLCashRequestID, args.Log);
			Trail.SetTag(args.Tag);

			// old auto-reject with medal (run in parallel)
			this.internalAgent = new RejectionAgent(
				DB,
				Log,
				args.CustomerID,
				Trail.CashRequestID,
				args.NLCashRequestID,
				args.Configs
			);

			Output = new AutoRejectionOutput();
			model = null;
		} // constructor

		public AutoRejectionOutput Output { get; private set; }

		public RejectionTrail Trail { get; private set; }

		public AutoRejectionArguments args { get; private set; }

		private AConnection DB { get { return args.DB; } }
		private ASafeLog Log { get { return args.Log; } }

		// old auto-reject with medal (run in parallel)
		private readonly RejectionAgent internalAgent;

		public AV_LogicalGlueDataModel model { get; private set; }

		public RejectionTrail InternalTrail {
			get { return this.internalAgent.Trail; }
		} // InternalTrail

		/// <summary>
		/// Makes decision: to reject or not to reject.
		/// </summary>
		public void MakeDecision() {
			// old auto-reject with medal (run in parallel)
			this.internalAgent.MakeDecision(this.internalAgent.GetRejectionInputData(args.Now));
			this.internalAgent.Trail.SetTag(Trail.Tag);
			this.internalAgent.Trail.Save(DB, null, TrailPrimaryStatus.OldVerification);

			using (Trail.AddCheckpoint(ProcessCheckpoints.MakeDecision)) {
				Log.Debug("Secondary LG: checking auto reject for customer {0}...", args.CustomerID);

				// get company data at "now" date 
				model = DB.FillFirst<AV_LogicalGlueDataModel>(
					"select top 1 co.TypeOfBusiness from CustomerCompanyHistory h join [dbo].[Company] co on co.Id=h.CompanyId where h.CustomerId=@CustomerID and h.CompanyId=@CompanyID and h.InsertDate<=@ProcessingDate order by h.[InsertDate] desc", CommandSpecies.Text,
					new QueryParameter("CustomerID", args.CustomerID),
					new QueryParameter("CompanyID", args.CompanyID),
					new QueryParameter("ProcessingDate", args.Now));

				Log.Debug("Customer {0} has company {1} of type {3} data at {2:d}", args.CustomerID, args.CompanyID, args.Now, model.TypeOfBusiness);

				// get company data from Customer table 
				if (model == null) {
					model = DB.FillFirst<AV_LogicalGlueDataModel>("select TypeOfBusiness from Customer where Id=@CustomerID", CommandSpecies.Text, new QueryParameter("CustomerID", args.CustomerID));
				}

				TypeOfBusiness typeOfBusiness;
				Enum.TryParse(model.TypeOfBusiness, out typeOfBusiness);

				if (typeOfBusiness.IsRegulated() || model == null) {
					// add InternalFlow step 
					Output.FlowType = AutoDecisionFlowTypes.Internal;
					StepNoDecision<InternalFlow>().Init();
					Trail.AppendOverridingResults(this.internalAgent.Trail);
					return;
				}

				// init LogicalGlueFlow  
				Output.FlowType = AutoDecisionFlowTypes.LogicalGlue;
				StepNoDecision<LogicalGlueFlow>().Init();

				model = DB.FillFirst<AV_LogicalGlueDataModel>("AV_LogicalGlueDataForCustomer", CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", args.CustomerID),
					new QueryParameter("CompanyID", args.CompanyID),
					new QueryParameter("PlannedPayment", args.MonthlyPayment),
					new QueryParameter("ProcessingDate", args.Now));

				Log.Debug("{0}", model);

				// LG data not found
				if (model == null) {
					StepNoDecision<LGDataFound>().Init(false);
					return;
				}

				StepNoDecision<LGDataFound>().Init(true);

				// LG data returned error
				if (!string.IsNullOrEmpty(model.ErrorMessage) || model.Message.Contains("error")) {
					StepNoReject<LGWithoutError>(true).Init(false);
					return;
				}

				StepNoDecision<LGWithoutError>().Init(true);

				if (model.EtlCode.Equals("Hard reject")) {
					StepReject<LGHardReject>(true).Init(true);
					return;
				}

				StepNoDecision<LGHardReject>().Init(false);

				if (model.Score == null || model.Score == 0 || model.GradeID == 0 || model.GradeID == null) {
					StepNoReject<HasBucket>(true).Init(false);
					return;
				}

				StepNoDecision<HasBucket>().Init(true);

				List<AutoRejectionOutput> rangesList = DB.Fill<AutoRejectionOutput>(
				"select distinct r.GradeRangeID from [dbo].[I_GradeRange] r " +
					"join CustomerOrigin org on r.OriginID=org.CustomerOriginID and org.CustomerOriginID=(select c.OriginID from Customer c where Id=@CustomerID) " +
					"and r.GradeID=@GradeID and r.IsActive=1 " +
					"join LoanSource ls on ls.LoanSourceID = r.LoanSourceID and ls.IsDefault=1 " +
					"join [dbo].[I_ProductSubType] st on st.LoanSourceID=ls.LoanSourceID and st.IsRegulated=@Regulated " +
					"where r.IsFirstLoan=(CASE WHEN (select COUNT(Id) xx from Loan l where CustomerId=@CustomerID) > 0 THEN 1 ELSE 0 END)", CommandSpecies.Text,
					new QueryParameter("CustomerID", args.CustomerID),
					new QueryParameter("GradeID", model.GradeID),
					new QueryParameter("Regulated", typeOfBusiness.IsRegulated()));

				int reangesCount = rangesList.Count;

				if (reangesCount == 0) {
					StepReject<OfferconfigurationFound>(true).Init(0);
					return;
				}

				if (reangesCount > 1) {
					StepReject<OfferconfigurationFound>(true).Init(reangesCount);

					// company type, loan source, customer is new/old
					Log.Alert("Too many configurations found. Args: {0}; score: {1}", args, model.Score);

					return;
				}

				StepNoDecision<OfferconfigurationFound>().Init(1);

				Output.GradeRangeID = rangesList.First().GradeRangeID;

				Trail.DecideIfNotDecided();
			} // using
		} // MakeDecision

		private T StepReject<T>(bool bLockDecisionAfterAddingAStep) where T : ATrace {
			return Trail.Affirmative<T>(bLockDecisionAfterAddingAStep);
		} // StepReject

		private T StepNoReject<T>(bool bLockDecisionAfterAddingAStep) where T : ATrace {
			return Trail.Negative<T>(bLockDecisionAfterAddingAStep);
		} // StepNoReject

		private T StepNoDecision<T>() where T : ATrace {
			return Trail.Dunno<T>();
		} // StepNoDecision
	} // class LGAgent
} // namespace
