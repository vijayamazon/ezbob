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
		public LGAgent(AutoRejectionArguments args) {
			this.args = args;
			Output = new AutoRejectionOutput();

			Trail = new RejectionTrail(args.CustomerID, args.CashRequestID, args.Log);
			Trail.SetTag(args.Tag);

			// old auto-reject with medal (run in parallel)
			this.internalAgent = new RejectionAgent(DB, Log, CustomerID, Trail.CashRequestID, args.Configs);

			model = null;
		} // constructor

		public AutoRejectionOutput Output { get; private set; }

		public RejectionTrail Trail { get; private set; }

		private readonly AutoRejectionArguments args;

		private AConnection DB { get { return this.args.DB; } }
		private ASafeLog Log { get { return this.args.Log; } }
		private int CustomerID { get { return this.args.CustomerID; } }
		private DateTime Now { get { return this.args.Now; } }

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
			this.internalAgent.MakeDecision(this.internalAgent.GetRejectionInputData(Now));
			this.internalAgent.Trail.SetTag(Trail.Tag);
			this.internalAgent.Trail.Save(DB, null, TrailPrimaryStatus.OldVerification);

			using (Trail.AddCheckpoint(ProcessCheckpoints.MakeDecision)) {
				Log.Debug("Secondary LG: checking auto reject for customer {0}...", CustomerID);
	
				// get company data at "now" date 
				model = DB.FillFirst<AV_LogicalGlueDataModel>(
					"select top 1 h.CompanyId, co.TypeOfBusiness from [dbo].[CustomerCompanyHistory] h join [dbo].[Company] co on co.Id=h.CompanyId and h.CustomerId=@CustomerID and h.[InsertDate] <= @ProcessingDate order by h.[InsertDate] desc", CommandSpecies.Text,
					new QueryParameter("CustomerID", CustomerID),
					new QueryParameter("ProcessingDate", Now));

				Log.Debug("Customer {0} has company {1} data at {2:d}", CustomerID, model.CompanyId, Now);

				// get company data from Customer table 
				if (model == null) {
					model = DB.FillFirst<AV_LogicalGlueDataModel>(
					"select CompanyId, TypeOfBusiness from Customer where Id=@CustomerID", CommandSpecies.Text, new QueryParameter("CustomerID", CustomerID));
				}

				TypeOfBusiness typeOfBusiness;
				Enum.TryParse(model.TypeOfBusiness, out typeOfBusiness);

				Log.Debug("Customer {0} has company {1} of type {2}", CustomerID, model.CompanyId, typeOfBusiness);

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
					new QueryParameter("CustomerID", CustomerID),
					new QueryParameter("CompanyID", model.CompanyId),
					new QueryParameter("ProcessingDate", Now));

				// LG data not found
				if (model == null) {
					StepNoReject<LGDataFound>(true).Init(false);
					return;
				} else
					StepNoDecision<LGDataFound>().Init(true);

				// LG data returned error
				if (!string.IsNullOrEmpty(model.ErrorMessage)) {
					StepNoReject<LGWithoutError>(true).Init(false);
					return;
				} else
					StepNoDecision<LGWithoutError>().Init(true);

				if (model.Message.Equals("Hard reject")) {
					StepReject<LGHardReject>(true).Init(true);
					return;
				} else
					StepNoDecision<LGHardReject>().Init(false);

				if (model.GradeID==0) {
					StepNoReject<HasBucket>(true).Init(false);
					return;
				} else
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
