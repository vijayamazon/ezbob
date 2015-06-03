﻿namespace EzService.EzServiceImplementation {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Backend.Strategies.OfferCalculation;
	using Ezbob.Database;
	using EzBob.Backend.Models;

	partial class EzServiceImplementation {
		public ActionMetaData BackfillAml() {
			BackfillAml instance;
			return ExecuteSync(out instance, 0, 0);
		}

		public ActionMetaData BackfillLandRegistry2PropertyLink() {
			BackfillLandRegistry2PropertyLink instance;
			return ExecuteSync(out instance, 0, 0);
		}

		public ActionMetaData BackfillNonLimitedCompanies() {
			BackfillNonLimitedCompanies instance;
			return ExecuteSync(out instance, 0, 0);
		}

		public ActionMetaData BackfillTurnover() {
			return Execute<BackfillTurnover>(null, null);
		}

		public ActionMetaData BackfillHmrcBusinessRelevance() {
			return Execute<BackfillHmrcBusinessRelevance>(null, null);
		}

		public ActionMetaData BackfillZooplaValue() {
			BackfillZooplaValue instance;
			return ExecuteSync(out instance, 0, 0);
		}

		public ActionMetaData CalculateMedal(int underwriterId, int customerId) {
			CalculateMedal instance;
			return ExecuteSync(out instance, customerId, underwriterId, customerId, DateTime.UtcNow, false, true);
		}

		public ActionMetaData CalculateOffer(int underwriterId, int customerId, int amount, bool hasLoans, EZBob.DatabaseLib.Model.Database.Medal medalClassification) {
			CalculateOffer instance;
			return ExecuteSync(out instance, customerId, underwriterId, customerId, amount, hasLoans, medalClassification);
		}

		public ActionMetaData ChangeBrokerEmail(string oldEmail, string newEmail, string newPassword) {
			ChangeBrokerEmail instance;
			return ExecuteSync(out instance, 0, 0, oldEmail, newEmail, newPassword);
		}

		public NullableDateTimeActionResult GetCompanySeniority(int customerId, bool isLimited, int underwriterId) {
			GetCompanySeniority instance;

			ActionMetaData result = ExecuteSync(out instance, customerId, underwriterId, customerId, isLimited);

			return new NullableDateTimeActionResult {
				MetaData = result,
				Value = instance.CompanyIncorporationDate
			};
		}

		public ConfigTableActionResult GetConfigTable(int nUnderwriterID, string sTableName) {
			GetConfigTable strategyInstance;

			ActionMetaData oMetaData = ExecuteSync(out strategyInstance, null, nUnderwriterID, sTableName);

			return new ConfigTableActionResult {
				MetaData = oMetaData,
				Table = strategyInstance.Result,
			};
		}

		public DecimalActionResult GetCurrentCustomerAnnualTurnover(int customerID) {
			GetCurrentCustomerAnnualTurnover oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, customerID, null, customerID);

			return new DecimalActionResult {
				MetaData = oMetaData,
				Value = oInstance.Turnover,
			};
		}

		public StringActionResult GetCustomerState(int customerId) {
			string res = string.Empty;
			try {
				var instance = new GetAvailableFunds();
				instance.Execute();
				decimal availableFunds = instance.AvailableFunds;

				SafeReader sr = DB.GetFirst("GetCustomerDetailsForStateCalculation", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));

				int minLoanAmount = sr["MinLoanAmount"];
				string creditResult = sr["CreditResult"];
				string status = sr["Status"];
				bool isEnabled = sr["IsEnabled"];
				bool hasLateLoans = sr["HasLateLoans"];
				DateTime offerStart = sr["ApplyForLoan"];
				DateTime offerValidUntil = sr["ValidFor"];

				bool hasFunds = availableFunds >= minLoanAmount;

				if (!isEnabled)
					res = "disabled";
				else if (hasLateLoans)
					res = "late";
				else if (string.IsNullOrEmpty(creditResult) || creditResult == "WaitingForDecision")
					res = "wait";
				else if (status == "Rejected")
					res = "bad";
				else if (status == "Manual")
					res = "wait";
				else if (hasFunds && DateTime.UtcNow >= offerStart && DateTime.UtcNow <= offerValidUntil && status == "Approved")
					res = "get";
				else if (hasFunds && DateTime.UtcNow < offerStart && offerStart < offerValidUntil && status == "Approved")
					res = "wait";
				else if (!hasFunds || DateTime.UtcNow > offerValidUntil)
					res = "apply";
			} catch (Exception e) {
				Log.Error("Exception occurred during calculation of customer's state. The exception:{0}", e);
			}

			return new StringActionResult {
				Value = res
			};
		}

		public IntActionResult GetExperianAccountsCurrentBalance(int customerId, int underwriterId) {
			GetExperianAccountsCurrentBalance instance;

			ActionMetaData result = ExecuteSync(out instance, customerId, underwriterId, customerId);

			return new IntActionResult {
				MetaData = result,
				Value = instance.CurrentBalance
			};
		}

		public PropertyStatusesActionResult GetPropertyStatuses() {
			GetPropertyStatuses instance;

			ActionMetaData result = ExecuteSync(out instance, 0, 0);

			return new PropertyStatusesActionResult {
				MetaData = result,
				Groups = instance.Groups
			};
		}

		public ActionMetaData GetZooplaData(int customerId, bool reCheck) {
			return ExecuteSync<ZooplaStub>(customerId, null, customerId, reCheck);
		}

		public BoolActionResult SaveConfigTable(List<ConfigTable> configTableEntries, ConfigTableType configTableType) {
			SortedDictionary<int, ConfigTable> sortedEntries;
			bool isValid = IsValidConfigTableInput(configTableEntries, out sortedEntries);

			if (isValid) {
				try {
					DB.ExecuteNonQuery(
						"ConfigTable_Refill",
						CommandSpecies.StoredProcedure,
						DB.CreateTableParameter<ConfigTable>("@TheList", sortedEntries.Values, objbir => {
							var bir = (ConfigTable)objbir;
							return new object[] {
								bir.Start, bir.End, bir.Value
							};
						}),
						new QueryParameter("TableName", configTableType.ToString())
						);
				} catch (Exception e) {
					Log.Error("Exception occurred during execution of {0}. The exception:{1}", configTableType, e);
					isValid = false;
				}
			}

			return new BoolActionResult {
				Value = !isValid
			};
		}

		public ActionMetaData SendPendingMails(int underwriterId, int customerId) {
			SendPendingMails instance;
			return ExecuteSync(out instance, 0, 0, customerId);
		}

		public ActionMetaData Temp_BackFillMedals() {
			return Execute<Temp_BackFillMedals>(null, null);
		}

        public ActionMetaData PayPointAddedWithoutOpenLoan(int customerID, int userID, decimal amount, string paypointTransactionID) {
            return Execute<PayPointAddedWithoutOpenLoan>(customerID, userID, customerID, amount, paypointTransactionID);
		}
        
		private bool IsValidConfigTableInput(List<ConfigTable> configTableEntries, out SortedDictionary<int, ConfigTable> sortedEntries) {
			sortedEntries = new SortedDictionary<int, ConfigTable>();

			foreach (ConfigTable entry in configTableEntries) {
				if (sortedEntries.ContainsKey(entry.Start)) {
					string errorMessage = string.Format("Start must be unique:{0}", entry.Start);
					Log.Warn(errorMessage);
					return false;
				}
				sortedEntries.Add(entry.Start, entry);
			}

			bool isFirst = true;
			int highestSoFar = 0;
			foreach (int key in sortedEntries.Keys) {
				ConfigTable entry = sortedEntries[key];

				entry.Value /= 100; // Convert to decimal number

				if (isFirst) {
					if (entry.Start != 0) {
						const string errorMessage = "Start must start at 0";
						Log.Warn(errorMessage);
						return false;
					}
					isFirst = false;
				} else {
					if (highestSoFar + 1 < entry.Start) {
						string errorMessage = string.Format("No range covers the numbers {0}-{1}", highestSoFar + 1, entry.Start - 1);
						Log.Warn(errorMessage);
						return false;
					}
					if (highestSoFar + 1 > entry.Start) {
						string errorMessage = string.Format("The numbers {0}-{1} are covered by more than one range", entry.Start, highestSoFar);
						Log.Warn(errorMessage);
						return false;
					}
				}
				highestSoFar = entry.End;
			}

			if (highestSoFar < 10000000) {
				string errorMessage = string.Format("No range covers the numbers {0}-10000000", highestSoFar);
				Log.Warn(errorMessage);
				return false;
			}

			if (highestSoFar > 10000000) {
				const string errorMessage = "Maximum allowed number is 10000000";
				Log.Warn(errorMessage);
				return false;
			}

			return true;
		}

		public ActionMetaData LoanStatusAfterPayment(int userId, int customerID, string customerEmail, int loanID, decimal paymentAmount, bool sendMail) {
			return Execute<LoanStatusAfterPayment>(customerID, userId, customerID, customerEmail, loanID, paymentAmount, sendMail);
		}
	} // class EzServiceImplementation
} // namespace EzService
