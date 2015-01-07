namespace Ezbob.Backend.Strategies.MainStrategy.AutoDecisions {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Web;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoApproval;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using AutomationCalculator.Turnover;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Backend.Strategies.OfferCalculation;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EzBob.Models;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using MailApi;
	using StructureMap;

	public class Approval {
		public Approval(
			int customerId,
			int offeredCreditLine,
			Medal medalClassification,
			AutomationCalculator.Common.MedalType medalType,
			AutomationCalculator.Common.TurnoverType? turnoverType,
			AConnection db,
			ASafeLog log
		) {
			Now = DateTime.UtcNow;

			this.db = db;
			this.log = log ?? new SafeLog();

			this.loanRepository = ObjectFactory.GetInstance<LoanRepository>();
			var customerRepo = ObjectFactory.GetInstance<CustomerRepository>();
			this.cashRequestsRepository = ObjectFactory.GetInstance<CashRequestsRepository>();
			this.loanScheduleTransactionRepository = ObjectFactory.GetInstance<LoanScheduleTransactionRepository>();
			this.customerAnalytics = ObjectFactory.GetInstance<CustomerAnalyticsRepository>();

			this.customerId = customerId;
			this.autoApprovedAmount = offeredCreditLine;
			this.medalClassification = medalClassification;
			this.medalType = medalType;
			this.turnoverType = turnoverType;

			this.consumerCaisDetailWorstStatuses = new List<string>();

			this.customer = customerRepo.ReallyTryGet(customerId);

			this.m_oTrail = new ApprovalTrail(customerId, this.log, CurrentValues.Instance.AutomationExplanationMailReciever, CurrentValues.Instance.MailSenderEmail, CurrentValues.Instance.MailSenderName);

			this.m_oSecondaryImplementation = new Agent(
				customerId,
				offeredCreditLine,
				(AutomationCalculator.Common.Medal)medalClassification,
				medalType,
				turnoverType,
				db,
				log
				);

			this.m_oTurnover = new AutoApprovalTurnover {
				TurnoverType = this.turnoverType,
			};
			this.m_oTurnover.Init();
		} // constructor

		public Approval Init() {
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

			if (this.customer != null && this.customer.Company != null) {
				if (this.customer.Company.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited && this.customer.Company.ExperianRefNum != "NotFound") {
					var limited = new LoadExperianLtd(this.customer.Company.ExperianRefNum, 0);
					limited.Execute();

					this.directors = new List<Name>();

					foreach (ExperianLtdDL72 dataRow in limited.Result.GetChildren<ExperianLtdDL72>())
						this.directors.Add(new Name(dataRow.FirstName, dataRow.LastName));

					foreach (ExperianLtdDLB5 dataRow in limited.Result.GetChildren<ExperianLtdDLB5>())
						this.directors.Add(new Name(dataRow.FirstName, dataRow.LastName));
				}
			}

			this.hmrcNames = new List<string>();

			this.db.ForEachRowSafe((names, hren) => {
				string name = AutomationCalculator.Utils.AdjustCompanyName(names["BusinessName"]);
				if (name != string.Empty)
					this.hmrcNames.Add(name);
				return ActionResult.Continue;
			}, "GetHmrcBusinessNames", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", this.customerId));

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
			}

			this.consumerCaisDetailWorstStatuses.AddRange(oWorstStatuses);

			this.m_oSecondaryImplementation.Init();

			return this;
		} // Init

		public bool MakeAndVerifyDecision() {
			var availFunds = new GetAvailableFunds();
			availFunds.Execute();

			SaveTrailInputData(availFunds);

			CheckAutoApprovalConformance(availFunds.ReservedAmount);
			this.m_oSecondaryImplementation.MakeDecision();

			bool bSuccess = this.m_oTrail.EqualsTo(this.m_oSecondaryImplementation.Trail);

			if (bSuccess && this.m_oTrail.HasDecided) {
				if (this.autoApprovedAmount == this.m_oSecondaryImplementation.Result.ApprovedAmount) {
					this.m_oTrail.Affirmative<SameAmount>(false)
						.Init(this.autoApprovedAmount);
					this.m_oSecondaryImplementation.Trail.Affirmative<SameAmount>(false)
						.Init(this.m_oSecondaryImplementation.Result.ApprovedAmount);
				} else {
					this.m_oTrail.Negative<SameAmount>(false)
						.Init(this.autoApprovedAmount);
					this.m_oSecondaryImplementation.Trail.Negative<SameAmount>(false)
						.Init(this.m_oSecondaryImplementation.Result.ApprovedAmount);
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
					this.log.Info("Both Auto Approval implementations have reached the same decision: {0}", this.m_oTrail.HasDecided ? "approved" : "not approved");
					response.AutoApproveAmount = this.autoApprovedAmount;
				} else {
					this.log.Alert(
						"Switching to manual decision: Auto Approval implementations " +
							"have not reached the same decision for customer {0}, diff id is {1}.", this.customerId, this.m_oTrail.UniqueID.ToString("N")
						);

					response.LoanOfferUnderwriterComment = "Mismatch - " + this.m_oTrail.UniqueID;

					response.AutoApproveAmount = 0;

					response.CreditResult = CreditResultStatus.WaitingForDecision;
					response.UserStatus = Status.Manual;
					response.SystemDecision = SystemDecision.Manual;
				} // if

				decimal minLoanAmount = CurrentValues.Instance.GetCashSliderStep;

				response.AutoApproveAmount = (int)(
					Math.Round(response.AutoApproveAmount / minLoanAmount, 0, MidpointRounding.AwayFromZero) * minLoanAmount
					);

				this.log.Info("Decided to auto approve rounded amount: {0}", response.AutoApproveAmount);

				if (response.AutoApproveAmount != 0) {
					if (this.m_oTrail.MyInputData.AvailableFunds > response.AutoApproveAmount) {
						if (CurrentValues.Instance.AutoApproveIsSilent) {
							NotifyAutoApproveSilentMode(response);

							response.LoanOfferUnderwriterComment = "Silent Approve - " + this.m_oTrail.UniqueID;
							response.CreditResult = CreditResultStatus.WaitingForDecision;
							response.UserStatus = Status.Manual;
							response.SystemDecision = SystemDecision.Manual;
						} else {
							var offerDualCalculator = new OfferDualCalculator(this.db, this.log);
							OfferResult offerResult = offerDualCalculator.CalculateOffer(this.customerId, Now, response.AutoApproveAmount, this.hasLoans, this.medalClassification);
							if (offerResult == null || !string.IsNullOrEmpty(offerResult.Error)) {
								this.log.Alert("Failed calculating offer for auto-approve error:{0}. Will use manual. Customer:{1}", offerResult != null ? offerResult.Error : "", this.customerId);
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
								response.LoanOfferEmailSendingBannedNew = this.m_oTrail.MyInputData.MetaData.IsEmailSendingBanned;

								// Use offer calculated data
								response.RepaymentPeriod = offerResult.Period;
								response.IsEu = offerResult.IsEu;
								response.LoanTypeId = offerResult.LoanTypeId;
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

		private DateTime Now { get; set; }

		private int CalculateRollovers() {
			return this.loanRepository.ByCustomer(this.customerId)
				.SelectMany(loan => loan.Schedule)
				.Sum(sch => sch.Rollovers.Count());
		}

		private int CalculateSeniority() {
			if (this.customer == null)
				return -1;

			DateTime oMpOriginationDate = this.customer.GetMarketplaceOriginationDate(oIncludeMp: mp =>
				!mp.Marketplace.IsPaymentAccount ||
				mp.Marketplace.InternalId == PayPal ||
				mp.Marketplace.InternalId == Hmrc
			);

			DateTime oIncorporationDate = GetCustomerIncorporationDate();

			DateTime oDate = (oMpOriginationDate < oIncorporationDate) ? oMpOriginationDate : oIncorporationDate;

			return (int)(DateTime.UtcNow - oDate).TotalDays;
		}

		private static readonly Guid Hmrc = new Guid("AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA");
		private static readonly Guid PayPal = new Guid("3FA5E327-FCFD-483B-BA5A-DC1815747A28");

		public DateTime GetCustomerIncorporationDate() {
			if (customer == null)
				return DateTime.UtcNow;

			bool bIsLimited =
				(customer.Company != null) &&
				(customer.Company.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited);

			if (bIsLimited) {
				CustomerAnalytics oAnalytics = this.customerAnalytics.GetAll()
					.FirstOrDefault(ca => ca.Id == customer.Id);

				DateTime oIncorporationDate = (oAnalytics != null) ? oAnalytics.IncorporationDate : DateTime.UtcNow;

				if (oIncorporationDate.Year < 1000)
					oIncorporationDate = DateTime.UtcNow;

				return oIncorporationDate;
			} // if ltd

			DateTime? oDate = db.ExecuteScalar<DateTime?>(
				"GetNoLtdIncorporationDate",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", customer.Id)
			);

			return oDate ?? DateTime.UtcNow;
		} // GetCustomerIncorporationDate

		private int CalculateTodaysApprovals() {
			DateTime today = Now;
			return this.cashRequestsRepository.GetAll()
				.Count(cr => cr.CreationDate.HasValue && cr.CreationDate.Value.Year == today.Year && cr.CreationDate.Value.Month == today.Month && cr.CreationDate.Value.Day == today.Day && cr.UnderwriterComment == "Auto Approval");
		}

		private decimal CalculateTodaysLoans() {
			DateTime today = Now;

			var todayLoans = this.loanRepository.GetAll()
				.Where(l => l.Date.Year == today.Year && l.Date.Month == today.Month && l.Date.Day == today.Day);

			decimal todayLoansAmount = 0;

			if (todayLoans.Any())
				todayLoansAmount = todayLoans.Sum(l => l.LoanAmount);

			return todayLoansAmount;
		}

		private void CheckAge() {
			int autoApproveCustomerMinAge = CurrentValues.Instance.AutoApproveCustomerMinAge;
			int autoApproveCustomerMaxAge = CurrentValues.Instance.AutoApproveCustomerMaxAge;

			if ((this.customer == null) || (this.customer.PersonalInfo == null) || (this.customer.PersonalInfo.DateOfBirth == null)) {
				StepFailed<Age>()
					.Init(-1, autoApproveCustomerMinAge, autoApproveCustomerMaxAge);
			} else {
				DateTime now = Now;

				int customerAge = now.Year - this.customer.PersonalInfo.DateOfBirth.Value.Year;

				if (now < this.customer.PersonalInfo.DateOfBirth.Value.AddYears(customerAge))
					customerAge--;

				if (customerAge < autoApproveCustomerMinAge || customerAge > autoApproveCustomerMaxAge) {
					StepFailed<Age>()
						.Init(customerAge, autoApproveCustomerMinAge, autoApproveCustomerMaxAge);
				} else {
					StepDone<Age>()
						.Init(customerAge, autoApproveCustomerMinAge, autoApproveCustomerMaxAge);
				}
			} // if
		}

		private void CheckAllowedRange() {
			if (this.m_oTrail.MyInputData.Configuration.IsSilent) {
				StepDone<AmountOutOfRangle>()
					.Init(this.autoApprovedAmount, this.m_oTrail.MyInputData.Configuration.IsSilent);
			} else {
				int autoApproveMinAmount = this.m_oTrail.MyInputData.Configuration.MinLoan;
				int autoApproveMaxAmount = this.m_oTrail.MyInputData.Configuration.MaxAmount;

				if (this.autoApprovedAmount < autoApproveMinAmount || this.autoApprovedAmount > autoApproveMaxAmount) {
					StepFailed<AmountOutOfRangle>()
						.Init(this.autoApprovedAmount, autoApproveMinAmount, autoApproveMaxAmount);
				} else {
					StepDone<AmountOutOfRangle>()
						.Init(this.autoApprovedAmount, autoApproveMinAmount, autoApproveMaxAmount);
				}
			}
		}

		private void CheckAMLResult() {
			if (this.m_oTrail.MyInputData.MetaData.AmlResult != "Passed") {
				StepFailed<AmlCheck>()
					.Init(this.m_oTrail.MyInputData.MetaData.AmlResult);
			} else {
				StepDone<AmlCheck>()
					.Init(this.m_oTrail.MyInputData.MetaData.AmlResult);
			}
		}

		private void CheckAutoApprovalConformance(decimal outstandingOffers) {
			this.log.Debug("Primary: checking if auto approval should take place for customer {0}...", this.customerId);

			try {
				CheckInit();

				CheckMedal();
				CheckIsFraud();
				CheckIsBroker();
				CheckTodaysApprovals();
				CheckTodaysLoans();
				CheckOutstandingOffers(outstandingOffers);
				CheckAMLResult();
				CheckCustomerStatus();
				CheckBusinessScore();
				CheckExperianScore();
				CheckAge();
				CheckTurnovers();
				CheckSeniority();
				CheckDefaultAccounts();
				CheckIsDirector();
				CheckHmrcIsCompany();
				CheckTotalLoanCount();
				CheckWorstCaisStatus(this.m_oTrail.MyInputData.MetaData.TotalLoanCount > 0
					? CurrentValues.Instance.AutoApproveAllowedCaisStatusesWithLoan
					: CurrentValues.Instance.AutoApproveAllowedCaisStatusesWithoutLoan
					);
				CheckRollovers();
				CheckLateDays();
				CheckCustomerOpenLoans();
				CheckRepaidRatio();
				ReduceOutstandingPrincipal();

				CheckAllowedRange();

				CheckComplete();
			} catch (Exception ex) {
				StepFailed<ExceptionThrown>()
					.Init(ex);
			} // try

			this.log.Debug("Primary: checking if auto approval should take place for customer {0} complete.", this.customerId);

			this.log.Msg("Primary: auto approved amount: {0}. {1}", this.autoApprovedAmount, this.m_oTrail);
		}

		private void CheckBusinessScore() {
			int nThreshold = CurrentValues.Instance.AutoApproveBusinessScoreThreshold;
			int nScore = this.m_oTrail.MyInputData.MetaData.CompanyScore;

			if (nScore <= 0) {
				StepDone<BusinessScore>()
					.Init(nScore, nThreshold);
			} else if (nScore < nThreshold) {
				StepFailed<BusinessScore>()
					.Init(nScore, nThreshold);
			} else {
				StepDone<BusinessScore>()
					.Init(nScore, nThreshold);
			}
		}

		private void CheckComplete() {
			int nAutoApprovedAmount = this.autoApprovedAmount;

			if (nAutoApprovedAmount > 0) {
				StepDone<Complete>()
					.Init(nAutoApprovedAmount);
			} else {
				StepFailed<Complete>()
					.Init(nAutoApprovedAmount);
			}
		}

		private void CheckCustomerOpenLoans() {
			int autoApproveMaxNumOfOutstandingLoans = CurrentValues.Instance.AutoApproveMaxNumOfOutstandingLoans;

			if (this.m_oTrail.MyInputData.MetaData.OpenLoanCount > autoApproveMaxNumOfOutstandingLoans) {
				StepFailed<OutstandingLoanCount>()
					.Init(this.m_oTrail.MyInputData.MetaData.OpenLoanCount, autoApproveMaxNumOfOutstandingLoans);
			} else {
				StepDone<OutstandingLoanCount>()
					.Init(this.m_oTrail.MyInputData.MetaData.OpenLoanCount, autoApproveMaxNumOfOutstandingLoans);
			}
		}

		private void CheckCustomerStatus() {
			if (!this.m_oTrail.MyInputData.MetaData.CustomerStatusEnabled) {
				StepFailed<CustomerStatus>()
					.Init(this.m_oTrail.MyInputData.MetaData.CustomerStatusName);
			} else {
				StepDone<CustomerStatus>()
					.Init(this.m_oTrail.MyInputData.MetaData.CustomerStatusName);
			}
		}

		private void CheckDefaultAccounts() {
			if (this.m_oTrail.MyInputData.MetaData.NumOfDefaultAccounts > 0) {
				StepFailed<DefaultAccounts>()
					.Init(this.m_oTrail.MyInputData.MetaData.NumOfDefaultAccounts);
			} else {
				StepDone<DefaultAccounts>()
					.Init(this.m_oTrail.MyInputData.MetaData.NumOfDefaultAccounts);
			}
		}

		private void CheckExperianScore() {
			int nThreshold = CurrentValues.Instance.AutoApproveExperianScoreThreshold;
			int nScore = this.m_oTrail.MyInputData.MetaData.ConsumerScore;

			if (nScore < nThreshold) {
				StepFailed<ConsumerScore>()
					.Init(nScore, nThreshold);
			} else {
				StepDone<ConsumerScore>()
					.Init(nScore, nThreshold);
			}
		}

		private void CheckHmrcIsCompany() {
			bool isCompany = false;

			if (this.m_oTrail.MyInputData.HmrcBusinessNames.Count < 1) {
				StepDone<HmrcIsOfBusiness>()
					.Init();
				return;
			} // if

			foreach (string hmrcName in this.m_oTrail.MyInputData.HmrcBusinessNames) {
				if (hmrcName.Equals(this.m_oTrail.MyInputData.CompanyName)) {
					isCompany = true;
					break;
				} // if
			} // for

			if (!isCompany) {
				StepFailed<HmrcIsOfBusiness>()
					.Init(this.m_oTrail.MyInputData.HmrcBusinessNames, this.m_oTrail.MyInputData.CompanyName
					);
			} else {
				StepDone<HmrcIsOfBusiness>()
					.Init(this.m_oTrail.MyInputData.HmrcBusinessNames, this.m_oTrail.MyInputData.CompanyName
					);
			} // if
		}

		private void CheckTurnovers() {
			if (this.m_oTrail.MyInputData.IsTurnoverGood()) {
				StepDone<ThreeMonthsTurnover>()
					.Init(this.m_oTrail.MyInputData.Turnover3M, this.m_oTrail.MyInputData.Turnover1Y, this.m_oTrail.MyInputData.Configuration.TurnoverDropQuarterRatio);
			} else {
				StepFailed<ThreeMonthsTurnover>()
					.Init(this.m_oTrail.MyInputData.Turnover3M, this.m_oTrail.MyInputData.Turnover1Y, this.m_oTrail.MyInputData.Configuration.TurnoverDropQuarterRatio);
			}
		}

		private void CheckInit() {
			int nAutoApprovedAmount = this.autoApprovedAmount;

			if (nAutoApprovedAmount > 0) {
				StepDone<InitialAssignment>()
					.Init(this.autoApprovedAmount);
			} else {
				StepFailed<InitialAssignment>()
					.Init(this.autoApprovedAmount);
			}
		}

		private void CheckIsBroker() {
			if (this.isBrokerCustomer) {
				StepFailed<IsBrokerCustomer>()
					.Init();
			} else {
				StepDone<IsBrokerCustomer>()
					.Init();
			}
		}

		private void CheckIsDirector() {
			if (!this.m_oTrail.MyInputData.MetaData.IsLimitedCompanyType) {
				StepDone<CustomerIsDirector>()
					.Init(this.m_oTrail.MyInputData.MetaData.IsLimitedCompanyType);
				return;
			}

			bool isDirector = false;

			if (this.m_oTrail.MyInputData.DirectorNames.Count < 1) {
				StepFailed<CustomerIsDirector>()
					.Init(this.m_oTrail.MyInputData.CustomerName.ToString()
					);
				return;
			} // if

			foreach (Name directorName in this.m_oTrail.MyInputData.DirectorNames) {
				if (directorName.Equals(this.m_oTrail.MyInputData.CustomerName)) {
					isDirector = true;
					break;
				} // if
			} // for

			if (!isDirector) {
				StepFailed<CustomerIsDirector>()
					.Init(this.m_oTrail.MyInputData.CustomerName.ToString(), this.m_oTrail.MyInputData.DirectorNames.Select(x => x.ToString())
						.ToList()
					);
			} else {
				StepDone<CustomerIsDirector>()
					.Init(this.m_oTrail.MyInputData.CustomerName.ToString(), this.m_oTrail.MyInputData.DirectorNames.Select(x => x.ToString())
						.ToList()
					);
			} // if
		}

		private void CheckIsFraud() {
			if (this.m_oTrail.MyInputData.MetaData.FraudStatus == FraudStatus.Ok) {
				StepDone<FraudSuspect>()
					.Init(this.m_oTrail.MyInputData.MetaData.FraudStatus);
			} else {
				StepFailed<FraudSuspect>()
					.Init(this.m_oTrail.MyInputData.MetaData.FraudStatus);
			}
		}

		private void CheckLateDays() {
			int autoApproveMaxAllowedDaysLate = CurrentValues.Instance.AutoApproveMaxAllowedDaysLate;

			if (this.m_oTrail.MyInputData.LatePayments.Count < 1) {
				StepDone<LatePayment>()
					.Init(0, 0, Now, 0, Now, autoApproveMaxAllowedDaysLate);
				return;
			} // if

			foreach (Payment oPayment in this.m_oTrail.MyInputData.LatePayments) {
				StepFailed<LatePayment>()
					.Init(
						oPayment.LoanID,
						oPayment.ScheduleID, oPayment.ScheduleDate,
						oPayment.TransactionID, oPayment.TransactionTime,
						autoApproveMaxAllowedDaysLate
					);
			} // for
		}

		private void CheckMedal() {
			if (this.medalClassification == Medal.NoClassification) {
				StepFailed<MedalIsGood>()
					.Init((AutomationCalculator.Common.Medal)this.medalClassification);
			} else {
				StepDone<MedalIsGood>()
					.Init((AutomationCalculator.Common.Medal)this.medalClassification);
			}
		}

		private void CheckOutstandingOffers(decimal outstandingOffers) {
			int autoApproveMaxOutstandingOffers = CurrentValues.Instance.AutoApproveMaxOutstandingOffers;

			if (outstandingOffers >= autoApproveMaxOutstandingOffers) {
				StepFailed<OutstandingOffers>()
					.Init(outstandingOffers, autoApproveMaxOutstandingOffers);
			} else {
				StepDone<OutstandingOffers>()
					.Init(outstandingOffers, autoApproveMaxOutstandingOffers);
			}
		}

		private void CheckRepaidRatio() {
			decimal autoApproveMinRepaidPortion = CurrentValues.Instance.AutoApproveMinRepaidPortion;

			if (this.m_oTrail.MyInputData.MetaData.RepaidRatio >= autoApproveMinRepaidPortion) {
				StepDone<OutstandingRepayRatio>()
					.Init(this.m_oTrail.MyInputData.MetaData.RepaidRatio, autoApproveMinRepaidPortion);
			} else {
				StepFailed<OutstandingRepayRatio>()
					.Init(this.m_oTrail.MyInputData.MetaData.RepaidRatio, autoApproveMinRepaidPortion);
			}
		}

		private void CheckRollovers() {
			if (this.m_oTrail.MyInputData.MetaData.NumOfRollovers > 0) {
				StepFailed<Rollovers>()
					.Init();
			} else {
				StepDone<Rollovers>()
					.Init();
			}
		}

		private void CheckSeniority() {
			int autoApproveMinMpSeniorityDays = CurrentValues.Instance.AutoApproveMinMPSeniorityDays;

			if (this.m_oTrail.MyInputData.MarketplaceSeniority < autoApproveMinMpSeniorityDays) {
				StepFailed<MarketplaceSeniority>()
					.Init(this.m_oTrail.MyInputData.MarketplaceSeniority, autoApproveMinMpSeniorityDays);
			} else {
				StepDone<MarketplaceSeniority>()
					.Init(this.m_oTrail.MyInputData.MarketplaceSeniority, autoApproveMinMpSeniorityDays);
			}
		}

		private void CheckTodaysApprovals() {
			int autoApproveMaxDailyApprovals = CurrentValues.Instance.AutoApproveMaxDailyApprovals;

			if (this.m_oTrail.MyInputData.MetaData.NumOfTodayAutoApproval >= autoApproveMaxDailyApprovals) {
				StepFailed<TodayApprovalCount>()
					.Init(this.m_oTrail.MyInputData.MetaData.NumOfTodayAutoApproval, autoApproveMaxDailyApprovals);
			} else {
				StepDone<TodayApprovalCount>()
					.Init(this.m_oTrail.MyInputData.MetaData.NumOfTodayAutoApproval, autoApproveMaxDailyApprovals);
			}
		}

		private void CheckTodaysLoans() {
			int autoApproveMaxTodayLoans = CurrentValues.Instance.AutoApproveMaxTodayLoans;

			if (this.m_oTrail.MyInputData.MetaData.TodayLoanSum >= autoApproveMaxTodayLoans) {
				StepFailed<TodayLoans>()
					.Init(this.m_oTrail.MyInputData.MetaData.TodayLoanSum, autoApproveMaxTodayLoans);
			} else {
				StepDone<TodayLoans>()
					.Init(this.m_oTrail.MyInputData.MetaData.TodayLoanSum, autoApproveMaxTodayLoans);
			}
		}

		private void CheckTotalLoanCount() {
			StepDone<TotalLoanCount>()
				.Init(this.m_oTrail.MyInputData.MetaData.TotalLoanCount);
		}

		private void CheckWorstCaisStatus(string allowedStatuses) {
			List<string> oAllowedStatuses = allowedStatuses.Split(',')
				.ToList();

			List<string> diff = this.consumerCaisDetailWorstStatuses.Except(oAllowedStatuses)
				.ToList();

			if (diff.Count > 1) {
				StepFailed<WorstCaisStatus>()
					.Init(diff, this.consumerCaisDetailWorstStatuses, oAllowedStatuses);
			} else {
				StepDone<WorstCaisStatus>()
					.Init(null, this.consumerCaisDetailWorstStatuses, oAllowedStatuses);
			}
		}

		private void FindLatePayments() {
			int autoApproveMaxAllowedDaysLate = CurrentValues.Instance.AutoApproveMaxAllowedDaysLate;

			List<int> customerLoanIds = this.loanRepository.ByCustomer(this.customerId)
				.Select(d => d.Id)
				.ToList();

			foreach (int loanId in customerLoanIds) {
				int innerLoanId = loanId;

				IQueryable<LoanScheduleTransaction> backfilledMapping = this.loanScheduleTransactionRepository
					.GetAll()
					.Where(x =>
						x.Loan.Id == innerLoanId &&
							x.Schedule.Date.Date < x.Transaction.PostDate.Date &&
							x.Transaction.Status == LoanTransactionStatus.Done
					);

				foreach (var paymentMapping in backfilledMapping) {
					DateTime scheduleDate = paymentMapping.Schedule.Date.Date;

					DateTime transactionDate = paymentMapping.Transaction.PostDate.Date;

					double nTotalLateDays = (transactionDate - scheduleDate).TotalDays;

					if (nTotalLateDays > autoApproveMaxAllowedDaysLate) {
						this.m_oTrail.MyInputData.AddLatePayment(new Payment {
							LoanID = innerLoanId,
							ScheduleDate = paymentMapping.Schedule.Date.Date,
							ScheduleID = paymentMapping.Schedule.Id,
							TransactionID = paymentMapping.Transaction.Id,
							TransactionTime = paymentMapping.Transaction.PostDate,
						});
					} // if
				} // for
			} // for
		}

		private int FindNumOfDefaultAccounts() {
			if (this.m_oConsumerData.Cais.Count == 0)
				return 0;

			return this.m_oConsumerData.Cais.Count(c => c.AccountStatus == "F");
		} // FindNumOfDefaultAccounts

		private void FindOutstandingLoans() {
			MetaData oMeta = this.m_oTrail.MyInputData.MetaData; // just a shortcut

			List<Loan> outstandingLoans = FirstOfMonthStatusStrategyHelper.GetOutstandingLoans(this.customerId);

			oMeta.OpenLoanCount = outstandingLoans.Count;
			oMeta.TakenLoanAmount = 0;
			oMeta.RepaidPrincipal = 0;
			oMeta.SetupFees = 0;

			foreach (var loan in outstandingLoans) {
				oMeta.TakenLoanAmount += loan.LoanAmount;
				oMeta.RepaidPrincipal += loan.LoanAmount - loan.Principal;
				oMeta.SetupFees += loan.SetupFee;
			} // for
		}

		private void NotifyAutoApproveSilentMode(AutoDecisionResponse data) {
			try {
				var message = string.Format(
					@"<h1><u>Silent auto approve for customer <b style='color:red'>{0}</b></u></h1><br>
					<h2><b>Offer:</b></h2>
					<pre><h3>Amount: {1}</h3></pre><br>
					<pre><h3>Period: {2}</h3></pre><br>
					<pre><h3>Interest rate: {3}</h3></pre><br>
					<pre><h3>Setup fee: {4}</h3></pre><br>
					<h2><b>Decision flow:</b></h2>
					<pre><h3>{5}</h3></pre><br>
					<h2><b>Decision data:</b></h2>
					<pre><h3>{6}</h3></pre>", this.customerId,
					data.AutoApproveAmount,
					data.RepaymentPeriod,
					data.InterestRate,
					data.SetupFee,
					HttpUtility.HtmlEncode(this.m_oTrail.ToString()),
					HttpUtility.HtmlEncode(this.m_oTrail.InputData.Serialize())
					);

				new Mail().Send(
					CurrentValues.Instance.AutoApproveSilentToAddress,
					null,
					message,
					CurrentValues.Instance.MailSenderEmail,
					CurrentValues.Instance.MailSenderName,
					"#SilentApprove for customer " + this.customerId
					);
			} catch (Exception e) {
				this.log.Error(e, "Failed sending alert mail - silent auto approval.");
			} // try
		}

		private void ReduceOutstandingPrincipal() {
			this.autoApprovedAmount -= (int)this.m_oTrail.MyInputData.MetaData.OutstandingPrincipal;

			if (this.autoApprovedAmount < 0)
				this.autoApprovedAmount = 0;

			if (this.autoApprovedAmount > 0.00000001m) {
				StepDone<ReduceOutstandingPrincipal>()
					.Init(this.m_oTrail.MyInputData.MetaData.OutstandingPrincipal, this.autoApprovedAmount);
			} else {
				StepFailed<ReduceOutstandingPrincipal>()
					.Init(this.m_oTrail.MyInputData.MetaData.OutstandingPrincipal, this.autoApprovedAmount);
			}
		}

		private void SaveTrailInputData(GetAvailableFunds availFunds) {
			this.m_oTrail.MyInputData.SetDataAsOf(Now);

			this.m_oTrail.MyInputData.SetConfiguration(new Configuration {
				ExperianScoreThreshold = CurrentValues.Instance.AutoApproveExperianScoreThreshold,
				CustomerMinAge = CurrentValues.Instance.AutoApproveCustomerMinAge,
				CustomerMaxAge = CurrentValues.Instance.AutoApproveCustomerMaxAge,
				MinTurnover1M = CurrentValues.Instance.AutoApproveMinTurnover1M,
				MinTurnover3M = CurrentValues.Instance.AutoApproveMinTurnover3M,
				MinTurnover1Y = CurrentValues.Instance.AutoApproveMinTurnover1Y,
				MinMPSeniorityDays = CurrentValues.Instance.AutoApproveMinMPSeniorityDays,
				MaxOutstandingOffers = CurrentValues.Instance.AutoApproveMaxOutstandingOffers,
				MaxTodayLoans = CurrentValues.Instance.AutoApproveMaxTodayLoans,
				MaxDailyApprovals = CurrentValues.Instance.AutoApproveMaxDailyApprovals,
				MaxAllowedDaysLate = CurrentValues.Instance.AutoApproveMaxAllowedDaysLate,
				MaxNumOfOutstandingLoans = CurrentValues.Instance.AutoApproveMaxNumOfOutstandingLoans,
				MinRepaidPortion = CurrentValues.Instance.AutoApproveMinRepaidPortion,
				MinLoan = CurrentValues.Instance.MinLoan,
				MaxAmount = CurrentValues.Instance.AutoApproveMaxAmount,
				IsSilent = CurrentValues.Instance.AutoApproveIsSilent,
				SilentTemplateName = CurrentValues.Instance.AutoApproveSilentTemplateName,
				SilentToAddress = CurrentValues.Instance.AutoApproveSilentToAddress,
				BusinessScoreThreshold = CurrentValues.Instance.AutoApproveBusinessScoreThreshold,
				AllowedCaisStatusesWithLoan = CurrentValues.Instance.AutoApproveAllowedCaisStatusesWithLoan,
				AllowedCaisStatusesWithoutLoan = CurrentValues.Instance.AutoApproveAllowedCaisStatusesWithoutLoan,
				OnlineTurnoverAge = CurrentValues.Instance.AutoApproveOnlineTurnoverAge,
				OnlineTurnoverDropQuarterRatio = CurrentValues.Instance.AutoApproveOnlineTurnoverDropQuarterRatio,
				OnlineTurnoverDropMonthRatio = CurrentValues.Instance.AutoApproveOnlineTurnoverDropMonthRatio,
				HmrcTurnoverAge = CurrentValues.Instance.AutoApproveHmrcTurnoverAge,
				HmrcTurnoverDropQuarterRatio = CurrentValues.Instance.AutoApproveHmrcTurnoverDropQuarterRatio,
				HmrcTurnoverDropHalfYearRatio = CurrentValues.Instance.AutoApproveHmrcTurnoverDropHalfYearRatio,
				TurnoverDropQuarterRatio = CurrentValues.Instance.AutoApproveTurnoverDropQuarterRatio,
			});

			this.m_oTrail.MyInputData.SetArgs(
				this.customerId,
				this.autoApprovedAmount,
				(AutomationCalculator.Common.Medal)this.medalClassification,
				this.medalType,
				this.turnoverType
				);

			this.m_oTrail.MyInputData.SetMetaData(new MetaData {
				RowType = "MetaData",
				FirstName = (this.customer != null) && (this.customer.PersonalInfo != null) ? this.customer.PersonalInfo.FirstName : null,
				LastName = (this.customer != null) && (this.customer.PersonalInfo != null) ? this.customer.PersonalInfo.Surname : null,
				IsBrokerCustomer = this.isBrokerCustomer,
				NumOfTodayAutoApproval = CalculateTodaysApprovals(),
				TodayLoanSum = CalculateTodaysLoans(),
				FraudStatusValue = (int)((this.customer == null) ? FraudStatus.UnderInvestigation : this.customer.FraudStatus),
				AmlResult = (this.customer == null) ? "failed because customer not found" : this.customer.AMLResult,
				CustomerStatusName = this.customer == null ? "unknown" : this.customer.CollectionStatus.CurrentStatus.Name,
				CustomerStatusEnabled = this.customer != null && this.customer.CollectionStatus.CurrentStatus.IsEnabled,
				CompanyScore = this.minCompanyScore,
				ConsumerScore = this.minExperianScore,
				IncorporationDate = GetCustomerIncorporationDate(),
				DateOfBirth = ((this.customer != null) && (this.customer.PersonalInfo != null) && this.customer.PersonalInfo.DateOfBirth.HasValue) ? this.customer.PersonalInfo.DateOfBirth.Value : Now,
				NumOfDefaultAccounts = FindNumOfDefaultAccounts(),
				NumOfRollovers = CalculateRollovers(),
				TotalLoanCount = this.loanRepository.ByCustomer(this.customerId)
					.Count(),
				ExperianCompanyName = (this.customer != null) && (this.customer.Company != null) ? this.customer.Company.ExperianCompanyName : null,
				EnteredCompanyName = (this.customer != null) && (this.customer.Company != null) ? this.customer.Company.CompanyName : null,
				IsLimitedCompanyType = (this.customer != null) && (this.customer.PersonalInfo != null) && this.customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited
			});

			FindOutstandingLoans();

			SafeReader sr = this.db.GetFirst(
				"GetLastOfferDataForApproval",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId),
				new QueryParameter("Now", Now)
				);

			this.m_oTrail.MyInputData.MetaData.EmailSendingBanned = sr["EmailSendingBanned"];
			this.m_oTrail.MyInputData.MetaData.OfferStart = sr["OfferStart"];
			this.m_oTrail.MyInputData.MetaData.OfferValidUntil = sr["OfferValidUntil"];

			this.m_oTrail.MyInputData.SetDirectorNames(this.directors);
			this.m_oTrail.MyInputData.SetHmrcBusinessNames(this.hmrcNames);

			this.m_oTrail.MyInputData.SetWorstStatuses(this.consumerCaisDetailWorstStatuses);
			FindLatePayments();
			this.m_oTrail.MyInputData.SetSeniority(CalculateSeniority());
			this.m_oTrail.MyInputData.SetAvailableFunds(availFunds.AvailableFunds, availFunds.ReservedAmount);

			this.db.ForEachResult<TurnoverDbRow>(
				r => this.m_oTurnover.Add(r),
				"GetCustomerTurnoverForAutoDecision",
				new QueryParameter("IsForApprove", true),
				new QueryParameter("CustomerID", this.customerId),
				new QueryParameter("Now", Now)
				);

			this.m_oTrail.MyInputData.SetTurnoverData(this.m_oTurnover);

			this.m_oTrail.MyInputData.MetaData.Validate();
		} // SaveTrailInputData

		private T StepDone<T>() where T : ATrace {
			return this.m_oTrail.Affirmative<T>(false);
		} // StepDone

		private T StepFailed<T>() where T : ATrace {
			this.autoApprovedAmount = 0;
			return this.m_oTrail.Negative<T>(false);
		} // StepFailed

		private readonly CashRequestsRepository cashRequestsRepository;
		private readonly List<string> consumerCaisDetailWorstStatuses;
		private readonly Customer customer;
		private readonly int customerId;
		private readonly AConnection db;
		private readonly LoanRepository loanRepository;
		private readonly LoanScheduleTransactionRepository loanScheduleTransactionRepository;
		private readonly ASafeLog log;
		private readonly AutomationCalculator.AutoDecision.AutoApproval.Agent m_oSecondaryImplementation;
		private readonly ApprovalTrail m_oTrail;
		private readonly AutoApprovalTurnover m_oTurnover;
		private readonly Medal medalClassification;
		private readonly AutomationCalculator.Common.TurnoverType? turnoverType;
		private readonly AutomationCalculator.Common.MedalType medalType;
		private readonly CustomerAnalyticsRepository customerAnalytics;

		private int autoApprovedAmount;
		private List<Name> directors;
		private bool hasLoans;
		private List<String> hmrcNames;
		private bool isBrokerCustomer;
		private ExperianConsumerData m_oConsumerData;
		private int minCompanyScore;
		private int minExperianScore;
	} // class Approval
} // namespace
