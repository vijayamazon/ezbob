namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EzBob.Models.Marketplaces.Builders;
	using EzBob.Models.Marketplaces.Yodlee;
	using EZBob.DatabaseLib.Model.Database;

	public class BankBasedApproval
	{
		private enum Risk
		{
			AboveAverage,
			BelowAverage,
			LowAndMinimum
		}

		private readonly AConnection db;
		private readonly ASafeLog log;
		private readonly int customerId;
		private decimal personalScoreThresholdWhenNoCompanyScore;
		private decimal personalScoreThreshold;
		private int minAge;
		private decimal minAmlScore;
		private decimal minCompanySeniorityDays;
		private int minBusinessScore;
		private int belowAverageRiskBusinessScoreMin;
		private int belowAverageRiskBusinessScoreMax;
		private int belowAverageRiskPersonalScoreMin;
		private int belowAverageRiskPersonalScoreMax;
		private int minOffer;
		private int homeOwnerCap;
		private int notHomeOwnerCap;
		private int euCap;
		private bool isCustomerViaBroker;
		private bool isOffline;
		private bool isUnderAge;
		private bool hasDefaultAccountsInPeriod;
		private decimal amlScore;
		private decimal personalScore;
		private bool isDirectorInExperian;
		private decimal companySeniorityDays;
		private decimal tangibleEquity;
		private int businessScore;
		private bool isHomeOwner;
		private int loanTerm;
		private decimal loanOffer;
		private int offerValidForHours;
		private DateTime earliestTransactionDate;
		private int minNumberOfDays;
		private decimal sumOfLoanTransactions;
		private int numberOfPayers;
		private int numberOfVatReturns;
		private decimal annualizedTurnover;
		private decimal minAnnualizedTurnover;
		private int minNumberOfPayers;
		private readonly Dictionary<string,bool> specialPayers = new Dictionary<string, bool>();
		private readonly List<string> payerNames = new List<string>();
		private bool hasNonYodleeMarketplace;
		private readonly StrategiesMailer mailer;
		private bool isSilent;
		private string silentTemplateName;
		private string silentToAddress;
		private DateTime dateOfBirth;
		private bool isEnabled;
		private int numOfMonthsToLookForDefaults;
		private int unsettledDefaultCount;
		private DateTime startTimeForVatCheck;
		private decimal minLoanAmount;

		public BankBasedApproval(int customerId)
		{
			this.db = Library.Instance.DB;
			this.log = Library.Instance.Log;
			this.customerId = customerId;
			this.mailer = new StrategiesMailer();
		}

		private void GetPersonalInfo()
		{
			this.log.Info("Getting personal info for customer:{0}", this.customerId);

			GetYodleeSums();

			SafeReader sr = this.db.GetFirst("GetPersonalInfoForBankBasedApproval",
				CommandSpecies.StoredProcedure, 
				new QueryParameter("CustomerId", this.customerId),
				new QueryParameter("NumOfMonthsToLookForDefaults", this.numOfMonthsToLookForDefaults),
				new QueryParameter("StartTimeForVatCheck", this.startTimeForVatCheck), 
				new QueryParameter("Now", DateTime.UtcNow));

			this.hasDefaultAccountsInPeriod = sr["HasDefaultAccounts"];
			this.isCustomerViaBroker = sr["IsCustomerViaBroker"];
			this.unsettledDefaultCount = sr["UnsettledDefaultCount"];
			this.hasNonYodleeMarketplace = sr["HasNonYodleeMarketplace"];
			this.isOffline = sr["IsOffline"];
			this.dateOfBirth = sr["DateOfBirth"];
			this.isUnderAge = this.dateOfBirth.AddYears(this.minAge) > DateTime.UtcNow;
			bool isOwnerOfMainAddress = sr["IsOwnerOfMainAddress"];
			bool isOwnerOfOtherProperties = sr["IsOwnerOfOtherProperties"];
			this.isHomeOwner = isOwnerOfMainAddress || isOwnerOfOtherProperties;
			this.personalScore = sr["ExperianScore"];
			this.earliestTransactionDate = sr["EarliestTransactionDate"];
			this.annualizedTurnover = sr["TotalAnnualizedValue"];
			this.numberOfVatReturns = sr["NumberOfVatReturns"];

			this.amlScore = sr["AmlScore"];
			string firstName = sr["FirstName"];
			string surame = sr["Surame"];

			this.tangibleEquity = sr["InTngblAssets"];
			decimal totalCurrentAssets = sr["TngblAssets"];
			DateTime? companyIncorporationDate = sr["IncorporationDate"];
			this.companySeniorityDays = companyIncorporationDate.HasValue ? (decimal)(DateTime.UtcNow - companyIncorporationDate.Value).TotalDays : 0;
			this.businessScore = sr["CommercialDelphiScore"];

			CheckIfIsDirector(firstName, surame);
		}

		private void CheckIfIsDirector(string firstName, string surame) {
			string comparableFirstName = firstName.Trim().ToLower();
			string comparableLastName = surame.Trim().ToLower();
			this.isDirectorInExperian = false;

			this.db.ForEachRowSafe(
				(sr, bRowsetStart) => {
					string directorFirstName = sr["FirstName"];
					string directorLastName = sr["LastName"];

					if (
						directorFirstName.Trim().ToLower() == comparableFirstName &&
						directorLastName.Trim().ToLower() == comparableLastName
					) {
						this.isDirectorInExperian = true;
						return ActionResult.SkipAll;
					} // if

					return ActionResult.Continue;
				},
				"GetExperianDirectorsNamesForCustomer",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
			);
		}

		private void GetYodleeSums()
		{
			this.db.ForEachRowSafe(
				(sr, bRowsetStart) => {
					int mpId = sr["Id"];
					YodleeModel yodleeModel = new YodleeMarketplaceModelBuilder().BuildYodlee(mpId);

					if (
						yodleeModel != null &&
						yodleeModel.CashFlowReportModel != null &&
						yodleeModel.CashFlowReportModel.YodleeCashFlowReportModelDict != null
					) {
						if (
							yodleeModel.CashFlowReportModel.YodleeCashFlowReportModelDict.ContainsKey("5aLoan Repayments") &&
							yodleeModel.CashFlowReportModel.YodleeCashFlowReportModelDict["5aLoan Repayments"].ContainsKey(YodleeCashFlowReportModelBuilder.TotalColumn)
						) {
							this.sumOfLoanTransactions += (decimal)yodleeModel.CashFlowReportModel.YodleeCashFlowReportModelDict["5aLoan Repayments"][YodleeCashFlowReportModelBuilder.TotalColumn];
						} // if
					} // if

					return ActionResult.Continue;
				},
				"GetCustomerMarketplaces",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
			);
		}

		private void GetYodleePayersData() {
			this.numberOfPayers = 0;

			this.db.ForEachRowSafe((sr, bRowsetStart) => {
				string description = sr["Description"];

				string parsedName = string.Empty; // Parse name - logic for this wasn't defined yet

				if (!this.payerNames.Contains(parsedName))
				{
					this.payerNames.Add(parsedName);
					this.numberOfPayers++;
				}

				return ActionResult.Continue;
			},"GetYodleePayersInfo", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", this.customerId));
		}

		private void GetSpecialPayers()
		{
			this.db.ForEachRowSafe((sr, bRowsetStart) => {
				string name = sr["Name"];

				if (!string.IsNullOrEmpty(name) && !this.specialPayers.ContainsKey(name))
					this.specialPayers.Add(name, true);

				return ActionResult.Continue;
			}, "GetSpecialPayers", CommandSpecies.StoredProcedure);
		}

		public bool MakeDecision(AutoDecisionResponse response)
		{
			try
			{
				ReadConfigurations();
				GetPersonalInfo();
				
				// Payers code is commented out as it is not completed and not planned to be completed
				//GetYodleePayersData();
				//GetSpecialPayers();

				if (!CheckConditionsForApproval())
				{
					return false;
				}
			
				this.loanOffer = CalculateLoanOffer();
				if (this.loanOffer < this.minOffer)
				{
					this.log.Info("No bank based approval since the loan offer is too low:{0} while the minimum is:{1}", this.loanOffer, this.minOffer);
					return false;
				}

				CapOffer();
				
				// Round loan offer
				int roundedLoanOffer = (int) (Math.Round(this.loanOffer/this.minLoanAmount, 0, MidpointRounding.AwayFromZero)*this.minLoanAmount);
				response.BankBasedAutoApproveAmount = roundedLoanOffer;
				
				var instance = new GetAvailableFunds();
				instance.Execute();
				decimal availableFunds = instance.AvailableFunds;
				if (availableFunds >= response.BankBasedAutoApproveAmount)
				{
					if (this.isSilent)
					{
						NotifyAutoApproveSilentMode();

						response.CreditResult = CreditResultStatus.WaitingForDecision;
						response.UserStatus = Status.Manual;
						response.SystemDecision = SystemDecision.Manual;
					}
					else
					{
						SetApproval(response);
					}
				}
				else
				{
					this.log.Info("Not enough available funds for bank based approval. Available:{0} required:{1}. Will use manual decision", availableFunds, response.BankBasedAutoApproveAmount);
					response.CreditResult = CreditResultStatus.WaitingForDecision;
					response.UserStatus = Status.Manual;
					response.SystemDecision = SystemDecision.Manual;
				}

				return true;
			}
			catch (Exception e)
			{
				this.log.Error("Exception during bank based approval:{0}", e);
				return false;
			}
		}

		private void NotifyAutoApproveSilentMode()
		{
			this.log.Info("Sending silent bank based auto approval mail for: customerId={0} ApproveAmount={1}", this.customerId, this.loanOffer);

			this.mailer.SendMailViaMandrill(new MailMetaData(this.silentTemplateName) {
				{"CustomerId", this.customerId.ToString(CultureInfo.InvariantCulture)},
				{"Amount", this.loanOffer.ToString(CultureInfo.InvariantCulture)},
				new Addressee(this.silentToAddress, bShouldRegister: false),
			});
		}

		private void SetApproval(AutoDecisionResponse response)
		{
			response.CreditResult = CreditResultStatus.Approved;
			response.UserStatus = Status.Approved;
			response.SystemDecision = SystemDecision.Approve;
			response.LoanOfferUnderwriterComment = "Auto bank based approval";
			response.DecisionName = "Bank Based Approval";
			response.IsAutoBankBasedApproval = true;
			response.AppValidFor = DateTime.UtcNow.AddHours(this.offerValidForHours);
			response.RepaymentPeriod = this.loanTerm;
		}

		private void CapOffer()
		{
			if (this.loanOffer > this.homeOwnerCap && this.isHomeOwner)
			{
				this.log.Info("Capping offer to {0} (Original was:{1}) [home owner]", this.homeOwnerCap, this.loanOffer);
				this.loanOffer = this.homeOwnerCap;
			}
			else if (this.loanOffer > this.notHomeOwnerCap && !this.isHomeOwner)
			{
				this.log.Info("Capping offer to {0} (Original was:{1}) [not home owner]", this.notHomeOwnerCap, this.loanOffer);
				this.loanOffer = this.notHomeOwnerCap;
			}

			// Should be done only for eu loans
			// The offer can be defined as eu only after pricing model implementation - which is not defined yet
			/*if (isEuLoan && loanOffer > euCap)
			{
				log.Info("Capping offer to {0} (Original was:{1}) [EU]", euCap, loanOffer);
				loanOffer = euCap;
			}*/
		}

		private void ReadConfigurations()
		{
			this.log.Info("Getting configurations");

			SafeReader sr = this.db.GetFirst("GetBankBasedApprovalConfigs", CommandSpecies.StoredProcedure);

			this.personalScoreThresholdWhenNoCompanyScore = sr["BankBasedApprovalPersonalScoreThresholdWhenNoCompanyScore"];
			this.personalScoreThreshold = sr["BankBasedApprovalPersonalScoreThreshold"];
			this.minAge = sr["BankBasedApprovalMinAge"];
			this.minAmlScore = sr["BankBasedApprovalMinAmlScore"];
			this.minCompanySeniorityDays = sr["BankBasedApprovalMinCompanySeniorityDays"];
			this.minBusinessScore = sr["BankBasedApprovalMinBusinessScore"];
			this.belowAverageRiskBusinessScoreMin = sr["BankBasedApprovalBelowAverageRiskMinBusinessScore"];
			this.belowAverageRiskBusinessScoreMax = sr["BankBasedApprovalBelowAverageRiskMaxBusinessScore"];
			this.belowAverageRiskPersonalScoreMin = sr["BankBasedApprovalBelowAverageRiskMinPersonalScore"];
			this.belowAverageRiskPersonalScoreMax = sr["BankBasedApprovalBelowAverageRiskMaxPersonalScore"];
			this.minOffer = sr["BankBasedApprovalMinOffer"];
			this.homeOwnerCap = sr["BankBasedApprovalHomeOwnerCap"];
			this.notHomeOwnerCap = sr["BankBasedApprovalNotHomeOwnerCap"];
			this.euCap = sr["BankBasedApprovalEuCap"];
			this.offerValidForHours = sr["OfferValidForHours"];
			this.minNumberOfDays = sr["BankBasedApprovalMinNumberOfDays"];
			this.isSilent = sr["BankBasedApprovalIsSilent"];
			this.silentTemplateName = sr["BankBasedApprovalSilentTemplateName"];
			this.silentToAddress = sr["BankBasedApprovalSilentToAddress"];
			this.minNumberOfPayers = sr["BankBasedApprovalMinNumberOfPayers"];
			this.minAnnualizedTurnover = sr["BankBasedApprovalMinAnnualizedTurnover"];
			this.isEnabled = sr["BankBasedApprovalIsEnabled"];
			this.numOfMonthsToLookForDefaults = sr["BankBasedApprovalNumOfMonthsToLookForDefaults"];
			int numOfMonthBackForVatCheck = sr["BankBasedApprovalNumOfMonthBackForVatCheck"];
			this.startTimeForVatCheck = DateTime.UtcNow.AddMonths(-1 * numOfMonthBackForVatCheck);
			this.minLoanAmount = sr["MinLoanAmount"];
		}

		private bool CheckConditionsForApproval()
		{
			if (!this.isEnabled)
			{
				this.log.Info("No bank based approval since it is disabled");
				return false;
			}

			if (this.hasNonYodleeMarketplace)
			{
				this.log.Info("No bank based approval since the customer:{0} has non Yodlee marketplaces", this.customerId);
				return false;
			}

			if (this.isCustomerViaBroker)
			{
				this.log.Info("No bank based approval since the customer:{0} is via broker", this.customerId);
				return false;
			}

			if (!this.isOffline)
			{
				this.log.Info("No bank based approval since the customer:{0} is online", this.customerId);
				return false;
			}

			if (this.isUnderAge)
			{
				this.log.Info("No bank based approval since the customer:{0} is under {1}. Date of birth:{2}", this.customerId, this.minAge, this.dateOfBirth);
				return false;
			}

			if (this.hasDefaultAccountsInPeriod)
			{
				this.log.Info("No bank based approval since the customer:{0} has defaults in the past {1} months", this.customerId, this.numOfMonthsToLookForDefaults);
				return false;
			}

			if (this.unsettledDefaultCount > 0)
			{
				this.log.Info("No bank based approval since the customer:{0} has {1} unsettled defaults", this.customerId, this.unsettledDefaultCount);
				return false;
			}

			if (this.amlScore < this.minAmlScore)
			{
				this.log.Info("No bank based approval since the customer:{0} has aml score of:{1} and the minimum is:{2}", this.customerId, this.amlScore, this.minAmlScore);
				return false;
			}

			if (this.businessScore != 0)
			{
				if (!this.isDirectorInExperian)
				{
					this.log.Info("No bank based approval since the customer:{0} is not a director", this.customerId);
					return false;
				}

				if (this.personalScore < this.personalScoreThreshold)
				{
					this.log.Info("No bank based approval since the customer:{0} has personal score of:{1} and the minimum is:{2}", this.customerId, this.personalScore, this.personalScoreThreshold);
					return false;
				}

				if (this.companySeniorityDays < this.minCompanySeniorityDays)
				{
					this.log.Info("No bank based approval since the company has seniority of:{0} and the minimum is:{1}", this.companySeniorityDays, this.minCompanySeniorityDays);
					return false;
				}

				if (this.tangibleEquity <= 0)
				{
					this.log.Info("No bank based approval since the customer:{0} has non-positive tangible equity:{1}", this.customerId, this.tangibleEquity);
					return false;
				}

				if (this.businessScore < this.minBusinessScore)
				{
					this.log.Info("No bank based approval since the customer:{0} has business score of:{1} and the minimum is:{2}", this.customerId, this.businessScore, this.minBusinessScore);
					return false;
				}
			}
			else if (this.personalScore < this.personalScoreThresholdWhenNoCompanyScore)
			{
				this.log.Info("No bank based approval since there is no business score and the customer score is:{1} which is lower than minimum value:{2}", this.customerId, this.personalScore, this.personalScoreThresholdWhenNoCompanyScore);
				return false;
			}

			if ((DateTime.UtcNow - this.earliestTransactionDate).TotalDays < this.minNumberOfDays)
			{
				this.log.Info("No bank based approval since the earliest transaction is less than {0} days old. It is from {1}", this.minNumberOfDays, this.earliestTransactionDate);
				return false;
			}

			if (this.sumOfLoanTransactions > 0)
			{
				this.log.Info("No bank based approval since the sum of loan transactions is positive:{0}", this.sumOfLoanTransactions);
				return false;
			}

			//if (numberOfPayers < minNumberOfPayers && !payerNames.Any(payerName => specialPayers.ContainsKey(payerName)))
			//{
			//	log.Info("No bank based approval since there are no special payers, and num of payers:{0} is less than minimum:{1}", numberOfPayers, minNumberOfPayers);
			//	return false;
			//}

			if (this.numberOfVatReturns > 0)
			{
				this.log.Info("No bank based approval since the customer:{0} has {1} vat returns", this.customerId, this.numberOfVatReturns);
				return false;
			}

			if (this.annualizedTurnover < this.minAnnualizedTurnover)
			{
				this.log.Info("No bank based approval since the annualizeded turnover is {0}. Which is less than minimum:{1}", this.annualizedTurnover, this.minAnnualizedTurnover);
				return false;
			}

			return true;
		}

		private Risk GetRisk()
		{
			if (this.businessScore != 0)
			{
				if (this.businessScore >= this.minBusinessScore && this.businessScore < this.belowAverageRiskBusinessScoreMin)
				{
					return Risk.AboveAverage;
				}

				if (this.businessScore >= this.belowAverageRiskBusinessScoreMin && this.businessScore <= this.belowAverageRiskBusinessScoreMax)
				{
					return Risk.BelowAverage;
				}

				return Risk.LowAndMinimum;
			}

			if (this.personalScore >= this.personalScoreThresholdWhenNoCompanyScore && this.personalScore < this.belowAverageRiskPersonalScoreMin)
			{
				return Risk.AboveAverage;
			}

			if (this.personalScore >= this.belowAverageRiskPersonalScoreMin && this.personalScore <= this.belowAverageRiskPersonalScoreMax)
			{
				return Risk.BelowAverage;
			}

			return Risk.LowAndMinimum;
		}

		private decimal CalculateLoanOffer()
		{
			Risk risk = GetRisk();
			switch (risk)
			{
				case Risk.LowAndMinimum:
					this.loanTerm = 12;
					return this.annualizedTurnover * 0.03m;
				case Risk.BelowAverage:
					this.loanTerm = 9;
					return this.annualizedTurnover * 0.0225m;
				default: // Risk.AboveAverage
					this.loanTerm = 6;
					return this.annualizedTurnover * 0.015m;

			}
		}
	}
}
