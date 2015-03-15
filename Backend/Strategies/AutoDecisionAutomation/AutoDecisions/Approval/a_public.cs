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

	public partial class Approval {
		public Approval(
			int customerId,
			int offeredCreditLine,
			Medal medalClassification,
			AutomationCalculator.Common.MedalType medalType,
			AutomationCalculator.Common.TurnoverType? turnoverType,
			AConnection db,
			ASafeLog log
		) {
			this.m_oTrail = new ApprovalTrail(
				customerId,
				this.log,
				CurrentValues.Instance.AutomationExplanationMailReciever,
				CurrentValues.Instance.MailSenderEmail,
				CurrentValues.Instance.MailSenderName
			) {
				Amount = offeredCreditLine,
			};

			using (this.m_oTrail.AddCheckpoint(ProcessCheckpoints.Creation)) {
				Now = DateTime.UtcNow;

				this.db = db;
				this.log = log ?? new SafeLog();

				this.loanRepository = ObjectFactory.GetInstance<LoanRepository>();
				var customerRepo = ObjectFactory.GetInstance<CustomerRepository>();
				this.cashRequestsRepository = ObjectFactory.GetInstance<CashRequestsRepository>();
				this.loanScheduleTransactionRepository = ObjectFactory.GetInstance<LoanScheduleTransactionRepository>();
				this.customerAnalytics = ObjectFactory.GetInstance<CustomerAnalyticsRepository>();

				this.customerId = customerId;
				this.medalClassification = medalClassification;
				this.medalType = medalType;
				this.turnoverType = turnoverType;

				this.consumerCaisDetailWorstStatuses = new List<string>();

				this.customer = customerRepo.ReallyTryGet(customerId);

				this.m_oTurnover = new AutoApprovalTurnover {
					TurnoverType = this.turnoverType,
				};
				this.m_oTurnover.Init();
			} // using timer step

			this.m_oSecondaryImplementation = new Agent(
				customerId,
				offeredCreditLine,
				(AutomationCalculator.Common.Medal)medalClassification,
				medalType,
				turnoverType,
				db,
				log
			);
		} // constructor

		public Approval Init() {
			using (this.m_oTrail.AddCheckpoint(ProcessCheckpoints.Initializtion)) {
				var stra = new LoadExperianConsumerData(this.customerId, null, null);
				stra.Execute();

				this.m_oConsumerData = stra.Result;

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
						string name = AutomationCalculator.Utils.AdjustCompanyName(names["BusinessName"]);
						if (name != string.Empty)
							this.hmrcNames.Add(name);
					},
					"GetHmrcBusinessNames",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", this.customerId)
				);

				SafeReader sr = this.db.GetFirst(
					"GetExperianMinMaxConsumerDirectorsScore",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", this.customerId),
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
					new QueryParameter("CustomerId", this.customerId),
					new QueryParameter("TakeMinScore", true),
					oScore,
					oDate
				);

				int nScore;
				if (int.TryParse(oScore.SafeReturnedValue, out nScore))
					this.minCompanyScore = nScore;

				this.consumerCaisDetailWorstStatuses.Clear();
				var oWorstStatuses = new SortedSet<string>();

				if (this.m_oConsumerData.Cais != null) {
					foreach (var c in this.m_oConsumerData.Cais)
						oWorstStatuses.Add(c.WorstStatus.Trim());
				} // if

				this.consumerCaisDetailWorstStatuses.AddRange(oWorstStatuses);

				this.m_oSecondaryImplementation.Init();
			} // using timer step

			return this;
		} // Init

		public bool MakeAndVerifyDecision() {
			using (this.m_oTrail.AddCheckpoint(ProcessCheckpoints.MakeDecision)) {
				GetAvailableFunds availFunds;

				using (this.m_oTrail.AddCheckpoint(ProcessCheckpoints.GatherData)) {
					availFunds = new GetAvailableFunds();
					availFunds.Execute();

					SaveTrailInputData(availFunds);
				} // using timer step

				using (this.m_oTrail.AddCheckpoint(ProcessCheckpoints.RunCheck))
					CheckAutoApprovalConformance(availFunds.ReservedAmount);
			} // using timer step

			this.m_oSecondaryImplementation.MakeDecision();

			bool bSuccess = this.m_oTrail.EqualsTo(this.m_oSecondaryImplementation.Trail);

			if (bSuccess && this.m_oTrail.HasDecided) {
				if (this.m_oTrail.RoundedAmount == this.m_oSecondaryImplementation.Trail.RoundedAmount) {
					this.m_oTrail.Affirmative<SameAmount>(false).Init(this.m_oTrail.RoundedAmount);
					this.m_oSecondaryImplementation.Trail.Affirmative<SameAmount>(false)
						.Init(this.m_oSecondaryImplementation.Trail.RoundedAmount);
				} else {
					this.m_oTrail.Negative<SameAmount>(false).Init(this.m_oTrail.RoundedAmount);
					this.m_oSecondaryImplementation.Trail.Negative<SameAmount>(false)
						.Init(this.m_oSecondaryImplementation.Trail.RoundedAmount);
					bSuccess = false;
				} // if
			} // if

			this.m_oTrail.Save(this.db, this.m_oSecondaryImplementation.Trail);

			return bSuccess;
		} // MakeAndVerifyDecision

		public void MakeDecision(AutoDecisionResponse response) {
			try {
				response.LoanOfferUnderwriterComment = "Checking auto approve...";

				bool bSuccess = MakeAndVerifyDecision();

				if (bSuccess) {
					this.log.Info(
						"Both Auto Approval implementations have reached the same decision: {0}",
						this.m_oTrail.HasDecided ? "approved" : "not approved"
					);
					response.AutoApproveAmount = this.m_oTrail.RoundedAmount;
				} else {
					this.log.Alert(
						"Switching to manual decision: Auto Approval implementations " +
						"have not reached the same decision for customer {0}, diff id is {1}.",
						this.customerId,
						this.m_oTrail.UniqueID.ToString("N")
					);

					response.LoanOfferUnderwriterComment = "Mismatch - " + this.m_oTrail.UniqueID;

					response.AutoApproveAmount = 0;

					response.CreditResult = CreditResultStatus.WaitingForDecision;
					response.UserStatus = Status.Manual;
					response.SystemDecision = SystemDecision.Manual;
				} // if

				this.log.Info("Decided to auto approve rounded amount: {0}", response.AutoApproveAmount);

				if (response.AutoApproveAmount != 0) {
					if (this.m_oTrail.MyInputData.AvailableFunds > response.AutoApproveAmount) {
						var offerDualCalculator = new OfferDualCalculator(
							this.customerId,
							Now,
							response.AutoApproveAmount,
							this.hasLoans,
							this.medalClassification
						);

						OfferResult offerResult = offerDualCalculator.CalculateOffer();

						if (CurrentValues.Instance.AutoApproveIsSilent) {
							if (offerResult == null || offerResult.IsError) {
								this.log.Alert(
									"Customer {1} - will use manual. Offer result: {0}",
									offerResult != null ? offerResult.Description : "",
									this.customerId
								);

								response.CreditResult = CreditResultStatus.WaitingForDecision;
								response.UserStatus = Status.Manual;
								response.SystemDecision = SystemDecision.Manual;
								response.LoanOfferUnderwriterComment = "Calculator failure - " + this.m_oTrail.UniqueID;
							} else {
								response.LoanOfferUnderwriterComment = "Silent Approve - " + this.m_oTrail.UniqueID;
								response.CreditResult = CreditResultStatus.WaitingForDecision;
								response.UserStatus = Status.Manual;
								response.SystemDecision = SystemDecision.Manual;
							} // if

							NotifyAutoApproveSilentMode(
								response.AutoApproveAmount,
								offerResult == null ? 0 : offerResult.Period,
								offerResult == null ? 0 : offerResult.InterestRate / 100m,
								offerResult == null ? 0 : offerResult.SetupFee / 100m
							);
						} else {
							if (offerResult == null || offerResult.IsError) {
								this.log.Alert(
									"Customer {1} - will use manual. Offer result: {0}",
									offerResult != null ? offerResult.Description : "",
									this.customerId
								);

								response.CreditResult = CreditResultStatus.WaitingForDecision;
								response.UserStatus = Status.Manual;
								response.SystemDecision = SystemDecision.Manual;
								response.LoanOfferUnderwriterComment = "Calculator failure - " + this.m_oTrail.UniqueID;
							} else {
								response.CreditResult = CreditResultStatus.Approved;
								response.UserStatus = Status.Approved;
								response.SystemDecision = SystemDecision.Approve;
								response.LoanOfferUnderwriterComment = "Auto Approval";
								response.DecisionName = "Approval";
								response.AppValidFor = Now.AddDays(this.m_oTrail.MyInputData.MetaData.OfferLength);
								response.Decision = DecisionActions.Approve;
								response.LoanOfferEmailSendingBannedNew =
									this.m_oTrail.MyInputData.MetaData.IsEmailSendingBanned;

								// Use offer calculated data
								response.RepaymentPeriod = offerResult.Period;
								response.LoanSourceID = offerResult.IsEu
									? (int)LoanSourceName.EU
									: (int)LoanSourceName.Standard; // TODO replace with Loan source and not IsEU
								response.LoanTypeID = offerResult.LoanTypeId;
								response.InterestRate = offerResult.InterestRate / 100;
								response.SetupFee = offerResult.SetupFee / 100;
							}
						} // if is silent
					} // if there are enough funds
				} // if auto approved amount is not 0
			} catch (Exception e) {
				this.log.Error(e, "Exception during auto approval.");
				response.LoanOfferUnderwriterComment = "Exception - " + this.m_oTrail.UniqueID;
			} // try
		} // MakeDecision
	} // class Approval
} // namespace
