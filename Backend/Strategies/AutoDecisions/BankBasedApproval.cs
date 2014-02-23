namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using System.Xml;
	using EzBob.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.XmlUtils;

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
		private bool hasDefaultInLast2Years;
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
		private decimal vat;
		private decimal annualizedTurnover;
		private decimal minAnnualizedTurnover;
		private int minNumberOfPayers;
		private readonly Dictionary<string,bool> specialPayers = new Dictionary<string, bool>();
		private readonly List<string> payerNames = new List<string>();
		private bool hasNonYodleeMarketplace;
		private readonly StrategiesMailer mailer;
		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private bool isSilent;
		private string silentTemplateName;
		private string silentToAddress;
		private readonly ExperianUtils experianUtils;
		private DateTime dateOfBirth;

		public BankBasedApproval(int customerId, AConnection db, ASafeLog log)
		{
			this.db = db;
			this.log = log;
			this.customerId = customerId;
			experianUtils = new ExperianUtils(log);
			mailer = new StrategiesMailer(db, log);
			ReadConfigurations();
			GetPersonalInfo();
			GetSpecialPayers();
		}

		private void GetPersonalInfo()
		{
			log.Info("Getting personal info for customer:{0}", customerId);
			GetYodleePersonalData();

			annualizedTurnover = (decimal)strategyHelper.GetTotalSumOfOrdersForLoanOffer(customerId);

			DataTable dt = db.ExecuteReader("GetPersonalInfoForBankBasedApproval", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));
			var sr = new SafeReader(dt.Rows[0]);

			string amlData = sr["AmlData"];
			string firstName = sr["FirstName"];
			string surame = sr["Surame"];
			string companyData = sr["CompanyData"];
			hasDefaultInLast2Years = sr["HasDefaultAccounts"];
			isCustomerViaBroker = sr["IsCustomerViaBroker"];
			hasNonYodleeMarketplace = sr["HasNonYodleeMarketplace"];
			isOffline = sr["IsOffline"];
			dateOfBirth = sr["DateOfBirth"];
			isUnderAge = dateOfBirth.AddYears(minAge) <= DateTime.UtcNow;
			isHomeOwner = sr["IsHomeOwner"];
			personalScore = sr["ExperianScore"];

			// Parse experian data
			decimal totalCurrentAssets;
			amlScore = experianUtils.DetectAml(amlData);
			XmlNode companyInfo = Xml.ParseRoot(companyData);
			isDirectorInExperian = experianUtils.IsDirector(companyInfo, firstName, surame);
			experianUtils.DetectTangibleEquity(companyInfo, out tangibleEquity, out totalCurrentAssets);
			DateTime? companyIncorporationDate = experianUtils.DetectIncorporationDate(companyInfo);
			companySeniorityDays = companyIncorporationDate.HasValue ? (decimal)(DateTime.UtcNow - companyIncorporationDate.Value).TotalDays : 0;
			businessScore = experianUtils.DetectBusinessScore(companyInfo);
		}

		private void GetYodleePersonalData()
		{
			// TODO: complete implementation
			// yodlee specific data
			earliestTransactionDate = DateTime.UtcNow;
			sumOfLoanTransactions = 12345;
			vat = 55; // The vat is in the cashflow tab

			// Create SP that fetches all payers
			numberOfPayers = 4; // The payers should be parsed from the Description field in the 'bank account transactions' tab
			// fill payerNames
		}

		private void GetSpecialPayers()
		{
			DataTable dt = db.ExecuteReader("GetSpecialPayers", CommandSpecies.StoredProcedure);
			foreach (DataRow row in dt.Rows)
			{
				var sr = new SafeReader(row);
				string name = sr["Name"];

				if (!string.IsNullOrEmpty(name) && !specialPayers.ContainsKey(name))
				{
					specialPayers.Add(name, true);
				}
			}
		}

		public bool MakeDecision(AutoDecisionResponse response)
		{
			if (!CheckConditionsForApproval())
			{
				return false;
			}
			
			loanOffer = CalculateLoanOffer();
			if (loanOffer < minOffer)
			{
				log.Info("No bank based approval since the loan offer is too low:{0} while the minimum is:{1}", loanOffer, minOffer);
				return false;
			}

			CapOffer();

			if (isSilent)
			{
				NotifyAutoApproveSilentMode();

				response.CreditResult = "WaitingForDecision";
				response.UserStatus = "Manual";
				response.SystemDecision = "Manual";
			}
			else
			{
				SetApproval(response);
			}

			return true;
		}

		private void NotifyAutoApproveSilentMode()
		{
			var vars = new Dictionary<string, string>
			{
				{"customerId", customerId.ToString(CultureInfo.InvariantCulture)},
				{"ApproveAmount", loanOffer.ToString(CultureInfo.InvariantCulture)}
			};

			log.Info("Sending silent bank based auto approval mail for: customerId={0} ApproveAmount={1}", customerId, loanOffer);
			mailer.SendMailViaMandrill(vars, silentToAddress, string.Empty, silentTemplateName, false);
		}

		private void SetApproval(AutoDecisionResponse response)
		{
			response.CreditResult = "Approved";
			response.UserStatus = "Approved";
			response.SystemDecision = "Approve";
			response.LoanOfferUnderwriterComment = "Auto bank based approval";
			response.IsAutoBankBasedApproval = true;
			response.AppValidFor = DateTime.UtcNow.AddHours(offerValidForHours);
			response.RepaymentPeriod = loanTerm;
			response.BankBasedAutoApproveAmount = (int)loanOffer;
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
			log.Info("Getting configurations");
			DataTable dt = db.ExecuteReader("GetBankBasedApprovalConfigs", CommandSpecies.StoredProcedure);
			DataRow results = dt.Rows[0];
			var sr = new SafeReader(results);

			personalScoreThresholdWhenNoCompanyScore = sr["BankBasedApprovalPersonalScoreThresholdWhenNoCompanyScore"];
			personalScoreThreshold = sr["BankBasedApprovalPersonalScoreThreshold"];
			minAge = sr["BankBasedApprovalMinAge"];
			minAmlScore = sr["BankBasedApprovalMinAmlScore"];
			minCompanySeniorityDays = sr["BankBasedApprovalMinCompanySeniorityDays"];
			minBusinessScore = sr["BankBasedApprovalMinBusinessScore"];
			belowAverageRiskBusinessScoreMin = sr["BankBasedApprovalBelowAverageRiskMinBusinessScore"];
			belowAverageRiskBusinessScoreMax = sr["BankBasedApprovalBelowAverageRiskMaxBusinessScore"];
			belowAverageRiskPersonalScoreMin = sr["BankBasedApprovalBelowAverageRiskMinPersonalScore"];
			belowAverageRiskPersonalScoreMax = sr["BankBasedApprovalBelowAverageRiskMaxPersonalScore"];
			minOffer = sr["BankBasedApprovalMinOffer"];
			homeOwnerCap = sr["BankBasedApprovalHomeOwnerCap"];
			notHomeOwnerCap = sr["BankBasedApprovalNotHomeOwnerCap"];
			euCap = sr["BankBasedApprovalEuCap"];
			offerValidForHours = sr["OfferValidForHours"];
			minNumberOfDays = sr["BankBasedApprovalMinNumberOfDays"];
			isSilent = sr["BankBasedApprovalIsSilent"];
			silentTemplateName = sr["BankBasedApprovalSilentTemplateName"];
			silentToAddress = sr["BankBasedApprovalSilentToAddress"];
			minNumberOfPayers = sr["BankBasedApprovalMinNumberOfPayers"];
			minAnnualizedTurnover = sr["BankBasedApprovalMinAnnualizedTurnover"];
		}

		private bool CheckConditionsForApproval()
		{
			if (hasNonYodleeMarketplace)
			{
				log.Info("No bank based approval since the customer:{0} has non Yodlee marketplaces", customerId);
				return false;
			}

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

			if (isUnderAge)
			{
				log.Info("No bank based approval since the customer:{0} is under {1}. Date of birth:{2}", customerId, minAge, dateOfBirth);
				return false;
			}

			if (hasDefaultInLast2Years)
			{
				log.Info("No bank based approval since the customer:{0} has defaults in the past 2 years", customerId);
				return false;
			}

			if (amlScore < minAmlScore)
			{
				log.Info("No bank based approval since the customer:{0} has aml score of:{1} and the minimum is:{2}", customerId, amlScore, minAmlScore);
				return false;
			}

			if (businessScore != 0)
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
					log.Info("No bank based approval since the company has seniority of:{0} and the minimum is:{1}", companySeniorityDays, minCompanySeniorityDays);
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
			else if (personalScore < personalScoreThresholdWhenNoCompanyScore)
			{
				log.Info("No bank based approval since there is no business score and the customer score is:{1} which is lower than minimum value:{2}", customerId, personalScore, personalScoreThresholdWhenNoCompanyScore);
				return false;
			}

			if ((DateTime.UtcNow - earliestTransactionDate).TotalDays < minNumberOfDays)
			{
				log.Info("No bank based approval since the earliest transaction is less than {0} days old. It is from {1}", minNumberOfDays, earliestTransactionDate);
				return false;
			}

			if (sumOfLoanTransactions > 0)
			{
				log.Info("No bank based approval since the sum of loan transactions is positive:{0}", sumOfLoanTransactions);
				return false;
			}

			if (numberOfPayers < minNumberOfPayers && !payerNames.Any(payerName => specialPayers.ContainsKey(payerName)))
			{
				log.Info("No bank based approval since there are no special payers, and num of payers:{0} is less than minimum:{1}", numberOfPayers, minNumberOfPayers);
				return false;
			}

			if (vat > 0)
			{
				log.Info("No bank based approval since the customer has positive vat:{0}", vat);
				return false;
			}

			if (annualizedTurnover < minAnnualizedTurnover)
			{
				log.Info("No bank based approval since the annualizeded turnover is {0}. Which is less than minimum:{1}", annualizedTurnover, minAnnualizedTurnover);
				return false;
			}

			return true;
		}

		private Risk GetRisk()
		{
			if (businessScore != 0)
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
				case Risk.LowAndMinimum:
					loanTerm = 12;
					return annualizedTurnover * 0.03m;
				case Risk.BelowAverage:
					loanTerm = 9;
					return annualizedTurnover * 0.0225m;
				default: // Risk.AboveAverage
					loanTerm = 6;
					return annualizedTurnover * 0.015m;

			}
		}
	}
}
