namespace AutomationCalculator.AutoDecision.AutoRejection {
	using System;
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

			Trail = new RejectionTrail(args.CustomerID, args.CashRequestID, args.Log);
			Trail.SetTag(args.Tag);

			// old auto-reject with medal (run in parallel)
			this.internalAgent = new RejectionAgent(DB, Log, args.CustomerID, Trail.CashRequestID, args.Configs);

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

				if (model.Score == null || model.Score == 0 || model.GradeID==0 || model.GradeID==null) {
					StepNoReject<HasBucket>(true).Init(false);
					return;
				}

				StepNoDecision<HasBucket>().Init(true);

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
