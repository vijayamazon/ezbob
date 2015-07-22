namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Backend.Strategies.OfferCalculation;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using StructureMap;

	public partial class Approval : AAutoDecisionBase {
		public Approval(
			int customerId,
			long? cashRequestID,
			int offeredCreditLine,
			Medal medalClassification,
			AutomationCalculator.Common.MedalType medalType,
			AutomationCalculator.Common.TurnoverType? turnoverType,
			AConnection db,
			ASafeLog log
		) {
			this.trail = new ApprovalTrail(
				customerId,
				cashRequestID,
				this.log,
				CurrentValues.Instance.AutomationExplanationMailReciever,
				CurrentValues.Instance.MailSenderEmail,
				CurrentValues.Instance.MailSenderName
			) {
				Amount = offeredCreditLine,
			};

			using (this.trail.AddCheckpoint(ProcessCheckpoints.Creation)) {
				Now = DateTime.UtcNow;

				this.db = db;
				this.log = log.Safe();

				this.loanRepository = ObjectFactory.GetInstance<LoanRepository>();
				this.loanSourceRepository = ObjectFactory.GetInstance<LoanSourceRepository>();
				var customerRepo = ObjectFactory.GetInstance<CustomerRepository>();
				this.cashRequestsRepository = ObjectFactory.GetInstance<CashRequestsRepository>();
				this.loanScheduleTransactionRepository = ObjectFactory.GetInstance<LoanScheduleTransactionRepository>();
				this.customerAnalytics = ObjectFactory.GetInstance<CustomerAnalyticsRepository>();

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

				if (this.customer == null) {
					this.isBrokerCustomer = false;
					this.hasLoans = false;
				} else {
					this.isBrokerCustomer = this.customer.Broker != null;
					this.hasLoans = this.customer.Loans.Any();
				} // if

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

				this.hmrcNames = new List<string>();

				this.db.ForEachRowSafe(
					names => {
						if (!names["BelongsToCustomer"])
							return;

						string name = AutomationCalculator.Utils.AdjustCompanyName(names["BusinessName"]);
						if (name != string.Empty)
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

		public bool MakeAndVerifyDecision(string tag = null, bool quiet = false) {
			this.trail.SetTag(tag);

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

			this.m_oSecondaryImplementation.MakeDecision();

			bool bSuccess = this.trail.EqualsTo(this.m_oSecondaryImplementation.Trail, quiet);
			WasMismatch = !bSuccess;

			if (bSuccess && this.trail.HasDecided) {
				if (this.trail.RoundedAmount == this.m_oSecondaryImplementation.Trail.RoundedAmount) {
					this.trail.Affirmative<SameAmount>(false).Init(this.trail.RoundedAmount);
					this.m_oSecondaryImplementation.Trail.Affirmative<SameAmount>(false)
						.Init(this.m_oSecondaryImplementation.Trail.RoundedAmount);
				} else {
					this.trail.Negative<SameAmount>(false).Init(this.trail.RoundedAmount);
					this.m_oSecondaryImplementation.Trail.Negative<SameAmount>(false)
						.Init(this.m_oSecondaryImplementation.Trail.RoundedAmount);
					bSuccess = false;
					WasMismatch = true;
				} // if
			} // if

			this.trail.SetTag(tag).Save(this.db, this.m_oSecondaryImplementation.Trail);

			return bSuccess;
		} // MakeAndVerifyDecision

		public int ApprovedAmount {
			get { return this.trail.RoundedAmount; }
		} // ApprovedAmount

		public void MakeDecision(AutoDecisionResponse response, string tag) {
			try {
				response.LoanOfferUnderwriterComment = "Checking auto approve...";

				bool bSuccess = MakeAndVerifyDecision(tag);

				if (bSuccess) {
					this.log.Info(
						"Both Auto Approval implementations have reached the same decision: {0}",
						this.trail.HasDecided ? "approved" : "not approved"
					);
					response.AutoApproveAmount = this.trail.RoundedAmount;
				} else {
					this.log.Alert(
						"Switching to manual decision: Auto Approval implementations " +
						"have not reached the same decision for customer {0}, diff id is {1}.",
						this.trail.CustomerID,
						this.trail.UniqueID.ToString("N")
					);

					response.LoanOfferUnderwriterComment = "Mismatch - " + this.trail.UniqueID;

					response.AutoApproveAmount = 0;

					response.CreditResult = CreditResultStatus.WaitingForDecision;
					response.UserStatus = Status.Manual;
					response.SystemDecision = SystemDecision.Manual;
				} // if

				this.log.Info("Decided to auto approve rounded amount: {0}", response.AutoApproveAmount);

				if (response.AutoApproveAmount == 0)
					return;

				var source = this.loanSourceRepository.GetDefault();
				var offerDualCalculator = new OfferDualCalculator(
					this.trail.CustomerID,
					Now,
					response.AutoApproveAmount,
					this.hasLoans,
					this.medalClassification,
					source.ID,
					source.DefaultRepaymentPeriod ?? 15
				);

				OfferResult offerResult = offerDualCalculator.CalculateOffer();

				if (offerResult == null || offerResult.IsError) {
					this.log.Alert(
						"Customer {1} - will use manual. Offer result: {0}",
						offerResult != null ? offerResult.Description : "",
						this.trail.CustomerID
					);

					response.CreditResult = CreditResultStatus.WaitingForDecision;
					response.UserStatus = Status.Manual;
					response.SystemDecision = SystemDecision.Manual;
					response.LoanOfferUnderwriterComment = "Calculator failure - " + this.trail.UniqueID;
				} else if (CurrentValues.Instance.AutoApproveIsSilent) {
					response.CreditResult = CreditResultStatus.WaitingForDecision;
					response.UserStatus = Status.Manual;
					response.SystemDecision = SystemDecision.Manual;
					response.LoanOfferUnderwriterComment = "Silent Approve - " + this.trail.UniqueID;

					NotifyAutoApproveSilentMode(
						response.AutoApproveAmount,
						offerResult.Period,
						offerResult.InterestRate / 100m,
						offerResult.SetupFee / 100m
					);
				} else {
					response.CreditResult = CreditResultStatus.Approved;
					response.UserStatus = Status.Approved;
					response.SystemDecision = SystemDecision.Approve;
					response.LoanOfferUnderwriterComment = "Auto Approval";

					response.DecisionName = "Approval";
					response.AppValidFor = Now.AddDays(this.trail.MyInputData.MetaData.OfferLength);
					response.Decision = DecisionActions.Approve;
					response.LoanOfferEmailSendingBannedNew = this.trail.MyInputData.MetaData.IsEmailSendingBanned;

					// Use offer calculated data
					response.RepaymentPeriod = offerResult.Period;
					response.LoanSourceID = (int)LoanSourceName.COSME; // TODO replace with Loan source and not IsEU
					response.LoanTypeID = offerResult.LoanTypeId;
					response.InterestRate = offerResult.InterestRate / 100;
					response.SetupFee = offerResult.SetupFee / 100;
				} // if
			} catch (Exception e) {
				this.log.Error(e, "Exception during auto approval.");
				response.LoanOfferUnderwriterComment = "Exception - " + this.trail.UniqueID;
			} // try
		} // MakeDecision
	} // class Approval
} // namespace
