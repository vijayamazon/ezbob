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

	/// <summary>
	/// Detects whether customer should be rejected on specific time using Logical Glue data.
	/// Detection should work for any specified time (i.e. can be run retrospectively).
	/// Should fill <see cref="Trail"/> properties.
	/// </summary>
	/// <remarks>
	/// For regulated (non-limited) companies old flow should be applied.
	/// Logical Glue flow applies to non-regulated (limited) companies only;
	/// old flow should also be executed and results stored in DB for comparison.
	/// </remarks>
	/// <example>
	/// Intended usage:
	/// var agent = new LGAgent(db, log, customer ID, time, cash request id, configuration);
	/// agent.MakeDecision();
	/// </example>
	public class LGAgent {
		/// <summary>
		/// Creates an instance.
		/// </summary>
		/// <param name="oDB">Database connection.</param>
		/// <param name="oLog">Logger.</param>
		/// <param name="nCustomerID">Customer to inspect.</param>
		/// <param name="now">Inspection date and time.</param>
		/// <param name="cashRequestID">Link to customer's cash request. Can be null.
		/// DO NOT USE THIS ARGUMENT to deduce inspection time, it is used for logging only.</param>
		/// <param name="configs">Auto-rejection configuration. Can be null. Should be loaded from DB if null.</param>
		public LGAgent(
			AConnection oDB,
			ASafeLog oLog,
			int nCustomerID,
			DateTime now,
			long? cashRequestID,
			RejectionConfigs configs = null
		) {
			this.customerID = nCustomerID;
			this.now = now;

			this.log = oLog;
			this.db = oDB;

			this.configs = configs; // Can be null (thus should be loaded from DB somehow).

			Trail = new RejectionTrail(nCustomerID, cashRequestID, oLog);
			Trail.SetTag("AutomationVerificationAutoRejectLGAgent");

			// old auto-reject with medal (run in parallel)
			this.internalAgent = new RejectionAgent(this.db, this.log, this.customerID, Trail.CashRequestID, this.configs);

			model = null;
		} // constructor

		public RejectionTrail Trail { get; private set; }
		private readonly AConnection db;
		private readonly ASafeLog log;
		private readonly int customerID;
		private readonly RejectionConfigs configs;
		private readonly DateTime now;
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
			this.internalAgent.MakeDecision(this.internalAgent.GetRejectionInputData(this.now));
			this.internalAgent.Trail.Save(this.db, null, TrailPrimaryStatus.OldVerification);

			using (Trail.AddCheckpoint(ProcessCheckpoints.MakeDecision)) {

				this.log.Debug("Secondary LG: checking auto reject for customer {0}...", this.customerID);
	
				// get company data at "now" date 
				model = this.db.FillFirst<AV_LogicalGlueDataModel>(
					"select top 1 h.CompanyId, co.TypeOfBusiness from [dbo].[CustomerCompanyHistory] h join [dbo].[Company] co on co.Id=h.CompanyId and h.CustomerId=@CustomerID and h.[InsertDate] <= @ProcessingDate order by h.[InsertDate] desc", CommandSpecies.Text,
					new QueryParameter("CustomerID", this.customerID),
					new QueryParameter("ProcessingDate", this.now));

				this.log.Debug("Customer {0} has company {1} data at {2:d}", this.customerID, model.CompanyId, this.now);

				// get company data from Customer table 
				if (model == null) {
					model = this.db.FillFirst<AV_LogicalGlueDataModel>(
					"select CompanyId, TypeOfBusiness from Customer where Id=@CustomerID", CommandSpecies.Text, new QueryParameter("CustomerID", this.customerID));
				}

				TypeOfBusiness typeOfBusiness;
				Enum.TryParse(model.TypeOfBusiness, out typeOfBusiness);

				this.log.Debug("Customer {0} has company {1} of type {2}", this.customerID, model.CompanyId, typeOfBusiness);

				if (typeOfBusiness.IsRegulated() || model == null) {

					Trail.AddNote(string.Format("Company for customer {0} for {1:d} not found or not regulated. Auto-reject transmitted to 'old' (internal) autodecision with medal.", this.customerID, this.now));

					// add InternalFlow step 
					StepNoDecision<InternalFlow>().Init();
					return;
				}

				// init LogicalGlueFlow  
				StepNoDecision<LogicalGlueFlow>().Init();

				model = this.db.FillFirst<AV_LogicalGlueDataModel>("AV_LogicalGlueDataForCustomer", CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", this.customerID),
					new QueryParameter("CompanyID", model.CompanyId),
					new QueryParameter("ProcessingDate", this.now));

				// LG data not found
				if (model == null) {
					StepNoReject<LogicalGlueFlow>(true);
					return;
				}

				StepNoDecision<LGDataFound>().Init();

				// LG data returned error
				if (!string.IsNullOrEmpty(model.ErrorMessage)) {
					StepNoReject<LogicalGlueFlow>(true);
					return;
				}

				StepNoDecision<LGWithoutError>().Init();

				if (model.Message.Equals("Hard reject")) {
					StepReject<LogicalGlueFlow>(true);
					return;
				}

				StepNoDecision<LGHardReject>().Init();

				if (model.GradeID==0) {
					StepNoReject<LogicalGlueFlow>(true);
					return;
				}

				StepNoDecision<HasBucket>().Init();

				if (model.GradeOriginID == 0) {
					StepReject<LogicalGlueFlow>(true);
					return;
				}

				StepNoDecision<BucketSupported>().Init();
				
				// chto eto? zachem eto?
				if (!LogicalGlueFlowFollowed) {
					Trail.AppendOverridingResults(this.internalAgent.Trail);
				}
					

				//if (LogicalGlueFlowFollowed) {
				//	// TODO Logical Glue flow goes here.
				//} else
				//	Trail.AppendOverridingResults(this.internalAgent.Trail);
			}

		} // MakeDecision

		public bool LogicalGlueFlowFollowed {
			get { return Trail.FindTrace<LogicalGlueFlow>() != null; }
		} // LogicalGlueFlowFollowed

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
