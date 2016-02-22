namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using AutomationCalculator;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using StructureMap;

	public partial class Approval : AAutoDecisionBase, ICreateOfferInputData {
		public Approval(
			int customerId,
			long? cashRequestID,
			long? nlCashRequestID,
			int offeredCreditLine,
			Medal medalClassification,
			AutomationCalculator.Common.MedalType medalType,
			AutomationCalculator.Common.TurnoverType? turnoverType,
			string tag,
			AConnection db,
			ASafeLog log
		) {
			this.incorporationDate = null;

			this.trail = new ApprovalTrail(
				customerId,
				cashRequestID,
				nlCashRequestID,
				this.log,
				CurrentValues.Instance.AutomationExplanationMailReciever,
				CurrentValues.Instance.MailSenderEmail,
				CurrentValues.Instance.MailSenderName) {
				Amount = offeredCreditLine,
			};

			using (this.trail.AddCheckpoint(ProcessCheckpoints.Creation)) {
				this.trail.SetTag(tag);

				Now = DateTime.UtcNow;

				this.db = db;
				this.log = log.Safe();

				this.loanRepository = ObjectFactory.GetInstance<LoanRepository>();
				var customerRepo = ObjectFactory.GetInstance<CustomerRepository>();
				this.cashRequestsRepository = ObjectFactory.GetInstance<CashRequestsRepository>();
				this.loanScheduleTransactionRepository = ObjectFactory.GetInstance<LoanScheduleTransactionRepository>();

				this.medalClassification = medalClassification;
				this.medalType = medalType;
				this.turnoverType = turnoverType;

				this.customer = customerRepo.ReallyTryGet(customerId);

				this.turnover = new AutoApprovalTurnover {
					TurnoverType = this.turnoverType,
				};
				this.turnover.Init();
			} // using timer step

			this.m_oSecondaryImplementation = new Agent(
				this.trail.CustomerID,
				this.trail.CashRequestID,
				this.trail.NLCashRequestID,
				offeredCreditLine,
				(AutomationCalculator.Common.Medal)medalClassification,
				medalType,
				turnoverType,
				db,
				log
			);
		} // constructor

		public Approval Init() {
			using (this.trail.AddCheckpoint(ProcessCheckpoints.Initializtion)) {
				var stra = new LoadExperianConsumerData(this.trail.CustomerID, null, null);
				stra.Execute();

				this.experianConsumerData = stra.Result;

				if (this.customer == null)
					this.isBrokerCustomer = false;
				else
					this.isBrokerCustomer = this.customer.Broker != null;

				bool hasLtd =
					(this.customer != null) &&
					(this.customer.Company != null) &&
					(this.customer.Company.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited) &&
					(this.customer.Company.ExperianRefNum != "NotFound");

				if (hasLtd) {
					var limited = new LoadExperianLtd(this.customer.Company.ExperianRefNum, 0);
					limited.Execute();

					this.companyDissolutionDate = limited.Result.DissolutionDate;

					this.directors = new List<Name>();

					foreach (ExperianLtdDL72 dataRow in limited.Result.GetChildren<ExperianLtdDL72>())
						this.directors.Add(new Name(dataRow.FirstName, dataRow.LastName));

					foreach (ExperianLtdDLB5 dataRow in limited.Result.GetChildren<ExperianLtdDLB5>())
						this.directors.Add(new Name(dataRow.FirstName, dataRow.LastName));
				} // if

				this.hmrcNames = new List<NameForComparison>();

				this.db.ForEachRowSafe(
					names => {
						if (!names["BelongsToCustomer"])
							return;

						var name = new NameForComparison(names["BusinessName"]);
						if (name.AdjustedName != string.Empty)
							this.hmrcNames.Add(name);
					},
					"GetHmrcBusinessNames",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", this.trail.CustomerID)
				);

				SafeReader sr = this.db.GetFirst(
					"GetExperianMinMaxConsumerDirectorsScore",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", this.trail.CustomerID),
					new QueryParameter("Now", Now)
				);

				if (!sr.IsEmpty)
					this.minExperianScore = sr["MinExperianScore"];

				var oScore = new QueryParameter("CompanyScore") {
					Type = DbType.Int32,
					Direction = ParameterDirection.Output,
				};

				var oDate = new QueryParameter("IncorporationDate") {
					Type = DbType.DateTime2,
					Direction = ParameterDirection.Output,
				};

				this.db.ExecuteNonQuery(
					"GetCompanyScoreAndIncorporationDate",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", this.trail.CustomerID),
					new QueryParameter("TakeMinScore", true),
					oScore,
					oDate
				);

				int nScore;
				if (int.TryParse(oScore.SafeReturnedValue, out nScore))
					this.minCompanyScore = nScore;

				this.m_oSecondaryImplementation.Init();
			} // using timer step

			return this;
		} // Init

		public override void MakeAndVerifyDecision() {
			try {
				RunPrimaryOnly();

				this.m_oSecondaryImplementation.MakeDecision();

				WasMismatch = !this.trail.EqualsTo(this.m_oSecondaryImplementation.Trail);

				if (!WasMismatch && this.trail.HasDecided) {
					if (this.trail.RoundedAmount == this.m_oSecondaryImplementation.Trail.RoundedAmount) {
						this.trail.Affirmative<SameAmount>(false)
							.Init(this.trail.RoundedAmount);
						this.m_oSecondaryImplementation.Trail.Affirmative<SameAmount>(false)
							.Init(this.m_oSecondaryImplementation.Trail.RoundedAmount);
					} else {
						this.trail.Negative<SameAmount>(false)
							.Init(this.trail.RoundedAmount);
						this.m_oSecondaryImplementation.Trail.Negative<SameAmount>(false)
							.Init(this.m_oSecondaryImplementation.Trail.RoundedAmount);
						WasMismatch = true;
					} // if
				} // if
			} catch (Exception e) {
				this.log.Error(e, "Exception during auto approval.");
				this.trail.Negative<ExceptionThrown>(true).Init(e);
			} // try

			this.trail.Save(this.db, this.m_oSecondaryImplementation.Trail);
		} // MakeAndVerifyDecision

		public void RunPrimaryOnly() {
			using (this.trail.AddCheckpoint(ProcessCheckpoints.MakeDecision)) {
				GetAvailableFunds availFunds;

				using (this.trail.AddCheckpoint(ProcessCheckpoints.GatherData)) {
					availFunds = new GetAvailableFunds();
					GetAvailableFunds.LoadFromDB();
					availFunds.Execute();

					SaveTrailInputData(availFunds);
				} // using timer step

				using (this.trail.AddCheckpoint(ProcessCheckpoints.RunCheck))
					CheckAutoApprovalConformance(availFunds.ReservedAmount);
			} // using timer step
		} // RunPrimaryOnly

		public override bool WasException {
			get {
				if (this.trail == null)
					return false;

				return this.trail.FindTrace<ExceptionThrown>() != null;
			} // get
		} // WasException

		public override bool AffirmativeDecisionMade {
			get { return this.trail.HasDecided; }
		} // AffirmativeDecisionMade

		public int ApprovedAmount {
			get { return this.trail.RoundedAmount; }
		} // ApprovedAmount

		public ApprovalTrail Trail {
			get { return this.trail; }
		} // Trail

		public bool LogicalGlueFlowFollowed {
			get {
				if (this.trail == null)
					return false;

				return this.trail.FindTrace<LogicalGlueFlow>() != null;
			} // get
		} // LogicalGlueFlowFollowed
	} // class Approval
} // namespace
