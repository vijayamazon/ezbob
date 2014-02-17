namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

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
		private decimal minAge;
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
		private decimal age;
		private bool hasDefaultInLast2Years;
		private decimal amlScore;
		private bool businessScoreExists;
		private decimal personalScore;
		private bool isDirectorInExperian;
		private decimal companySeniorityDays;
		private decimal tangibleEquity;
		private int businessScore;
		private int companyId;
		private bool isHomeOwner;
		private int loanTerm;
		private decimal lastQuarterRevenues;
		private decimal loanOffer;

		public BankBasedApproval(int customerId, AConnection db, ASafeLog log)
		{
			this.db = db;
			this.log = log;
			this.customerId = customerId;
			ReadConfigurations();
			GetPersonalInfo();
		}

		public bool MakeDecision(AutoDecisionResponse response)
		{
			return false;// Temporary - should not approve until implementation is completed
			if (!CheckConditionsForApproval())
			{
				return false;
			}

			
			// Implement these as conditions that return false?
			//no loans received during the period, IF sum of all loan transactions in the period is positive - mandatory
			//no offer if # of clients/payers (credit transacations) is below 3 - mandatory
			//no offer if there are more than 1 refunds of VAT (credit VAT transaction) in the period of 3 months - mandatory
			//Turnover min 37,500 per Quarter - mandatory



			// Calculate offer
			loanOffer = CalculateLoanOffer();
			if (loanOffer < minOffer)
			{
				log.Info("No bank based approval since the loan offer is too low:{0} while the minimum is:{1}", loanOffer, minOffer);
				return false;
			}

			CapOffer();

			response.CreditResult = "Approved";
			response.UserStatus = "Approved";
			response.SystemDecision = "Approve";
			response.LoanOfferUnderwriterComment = "Auto bank based approval";
			response.IsAutoBankBasedApproval = true;
			//response.AppValidFor = DateTime.UtcNow.AddDays(response.LoanOfferOfferValidDays); // Should be based on config

			// set these on the response and use in the main strat
			//response.LoanTerm = loanTerm;
			//response.Interest = interest;

			return true;
		}

		private void CapOffer()
		{
			if (loanOffer > homeOwnerCap && isHomeOwner)
			{
				log.Info("Capping offer to {0} (Original was:{1}) [home owner]", homeOwnerCap, loanOffer);
				loanOffer = homeOwnerCap;
			}
			else if (loanOffer > notHomeOwnerCap && !isHomeOwner)
			{
				log.Info("Capping offer to {0} (Original was:{1}) [not home owner]", notHomeOwnerCap, loanOffer);
				loanOffer = notHomeOwnerCap;
			}

			if (loanOffer > euCap)
			{
				log.Info("Capping offer to {0} (Original was:{1}) [EU]", euCap, loanOffer);
				loanOffer = euCap;
			}
		}

		private void ReadConfigurations()
		{
			personalScoreThresholdWhenNoCompanyScore = 850;
			personalScoreThreshold = 560;
			minAge = 18;
			minAmlScore = 70;
			minCompanySeniorityDays = 1095;
			minBusinessScore = 31;
			belowAverageRiskBusinessScoreMin = 51;
			belowAverageRiskBusinessScoreMax = 80;
			belowAverageRiskPersonalScoreMin = 901;
			belowAverageRiskPersonalScoreMax = 1000;
			minOffer = 2250;
			homeOwnerCap = 20000;
			notHomeOwnerCap = 10000;
			euCap = 20000;
		}

		private void GetPersonalInfo()
		{
			isCustomerViaBroker = false;
			isOffline = false;
			age = 19;
			hasDefaultInLast2Years = false;
			amlScore = 71;
			businessScoreExists = false;
			personalScore = 851;
			isDirectorInExperian = true;
			companySeniorityDays = 1200;
			tangibleEquity = 1;
			businessScore = 31;
			companyId = 9;
			isHomeOwner = true;
			lastQuarterRevenues = 167000;
		}

		private bool CheckConditionsForApproval()
		{
			if (isCustomerViaBroker)
			{
				log.Info("No bank based approval since the customer:{0} is via broker", customerId);
				return false;
			}

			if (!isOffline)
			{
				log.Info("No bank based approval since the customer:{0} is online", customerId);
				return false;
			}

			if (age < minAge)
			{
				log.Info("No bank based approval since the customer:{0} is under {1}. Age:{2}", customerId, minAge, age);
				return false;
			}

			if (hasDefaultInLast2Years)
			{
				log.Info("No bank based approval since the customer:{0} has defaults in the past 2 years", customerId);
				return false;
			}

			if (amlScore <= minAmlScore)
			{
				log.Info("No bank based approval since the customer:{0} has aml score of:{1} and the minimum is:{2}", customerId, amlScore, minAmlScore);
				return false;
			}

			if (businessScoreExists)
			{
				if (!isDirectorInExperian)
				{
					log.Info("No bank based approval since the customer:{0} is not a director", customerId);
					return false;
				}

				if (personalScore < personalScoreThreshold)
				{
					log.Info("No bank based approval since the customer:{0} has personal score of:{1} and the minimum is:{2}", customerId, personalScore, personalScoreThreshold);
					return false;
				}

				if (companySeniorityDays < minCompanySeniorityDays)
				{
					log.Info("No bank based approval since the company:{0} has seniority of:{1} and the minimum is:{2}", companyId, companySeniorityDays, minCompanySeniorityDays);
					return false;
				}

				if (tangibleEquity <= 0)
				{
					log.Info("No bank based approval since the customer:{0} has non-positive tangible equity:{1}", customerId, tangibleEquity);
					return false;
				}

				if (businessScore < minBusinessScore)
				{
					log.Info("No bank based approval since the customer:{0} has business score of:{1} and the minimum is:{2}", customerId, businessScore, minBusinessScore);
					return false;
				}
			}
			else
			{
				if (personalScore < personalScoreThresholdWhenNoCompanyScore)
				{
					log.Info("No bank based approval since there is no business score and the customer score is:{1} which is lower than minimum value:{2}", customerId, personalScore, personalScoreThresholdWhenNoCompanyScore);
					return false;
				}
			}

			return true;
		}

		private Risk GetRisk()
		{
			if (businessScoreExists)
			{
				if (businessScore >= minBusinessScore && businessScore < belowAverageRiskBusinessScoreMin)
				{
					return Risk.AboveAverage;
				}

				if (businessScore >= belowAverageRiskBusinessScoreMin && businessScore <= belowAverageRiskBusinessScoreMax)
				{
					return Risk.BelowAverage;
				}

				return Risk.LowAndMinimum;
			}

			if (personalScore >= personalScoreThresholdWhenNoCompanyScore && personalScore < belowAverageRiskPersonalScoreMin)
			{
				return Risk.AboveAverage;
			}

			if (personalScore >= belowAverageRiskPersonalScoreMin && personalScore <= belowAverageRiskPersonalScoreMax)
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
				/*Should determine interest too*/
				case Risk.LowAndMinimum:
					loanTerm = 12;
					return lastQuarterRevenues * 0.12m;
				case Risk.BelowAverage:
					loanTerm = 9;
					return lastQuarterRevenues * 0.09m;
				default: // Risk.AboveAverage
					loanTerm = 6;
					return lastQuarterRevenues * 0.06m;

			}
		}
	}
}
