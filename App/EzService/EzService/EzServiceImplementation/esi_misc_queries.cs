namespace EzService.EzServiceImplementation {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies;
	using Ezbob.Backend.Strategies.Backfill;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Backend.Strategies.ManualDecision;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Database;
	using EzBob.Backend.Models;
	using EzService.ActionResults;

	partial class EzServiceImplementation {
		public ActionMetaData BackfillAml() {
			BackfillAml instance;
			return ExecuteSync(out instance, 0, 0);
		}

		public ActionMetaData BackfillLandRegistry2PropertyLink() {
			BackfillLandRegistry2PropertyLink instance;
			return ExecuteSync(out instance, 0, 0);
		}

		public ActionMetaData BackfillMedalForAll() {
			return Execute<BackfillMedalForAll>(null, null);
		} // BackfillMedalForAll

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

		public ActionMetaData CalculateMedal(int underwriterId, int customerId, long? cashRequestID, long? nlCashRequestID) {
			CalculateMedal instance;

			var args = new ExecuteArguments(customerId, cashRequestID, nlCashRequestID, DateTime.UtcNow, false, true) {
				CustomerID = customerId,
				UserID = underwriterId,
				OnSuccess = (stra, amd) => {
					try {
						var calcMedalStra = stra as CalculateMedal;

						if (calcMedalStra != null)
							calcMedalStra.Notify();
					} catch (Exception e) {
						Log.Alert(e, "Failed to send notifications from CalculateMedal strategy.");
					} // try
				},
			};

			return ExecuteSync(out instance, args);
		} // CalculateMedal

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
			GetCustomerState instance;
			ActionMetaData metadata = ExecuteSync(out instance, customerId, customerId, customerId);
			return new StringActionResult {
				Value = instance.Result,
				MetaData = metadata
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

		public ActionMetaData LoanStatusAfterPayment(int userId, int customerID, string customerEmail, int loanID, decimal paymentAmount, bool sendMail,decimal? balance = null, bool? isPaidOff = null) {
			return Execute<LoanStatusAfterPayment>(customerID, userId, customerID, customerEmail, loanID, paymentAmount, sendMail, balance, isPaidOff);
		}

		public ActionMetaData BackfillBrokerCommissionInvoice() {
			return Execute<BackFillBrokerCommissionInvoice>(null, null);
		}

		public ActionMetaData BackfillDailyLoanStats() {
			return Execute<BackfillDailyLoanStats>(null, null);
		} // BackfillDailyLoanStats

		public LoanCommissionDefaultsActionResult GetLoanCommissionDefaults(
			int underwriterID,
			long cashRequestID,
			decimal loanAmount
		) {
			GetLoanCommissionDefaults instance;

			ActionMetaData amd = ExecuteSync(out instance, null, underwriterID, cashRequestID, loanAmount);

			return new LoanCommissionDefaultsActionResult {
				MetaData = amd,
				BrokerCommission = instance.Result.BrokerCommission,
				ManualSetupFee = instance.Result.ManualSetupFee,
			};
		} // GetLoanCommissionDefaults

		public ActionMetaData GetIncomeSms(DateTime? date, bool isYesterday) {
			return Execute<GetIncomeSms>(null, null, date, isYesterday);
		} // GetIncomeSms

		public ApplicationInfoResult LoadApplicationInfo(
			int? underwriterID,
			int customerID,
			long? cashRequestID,
			DateTime? now
		) {
			LoadApplicationInfo instance;

			ActionMetaData amd = ExecuteSync(out instance, customerID, underwriterID, customerID, cashRequestID, now);

			return new ApplicationInfoResult {
				MetaData = amd,
				Model = instance.Result,
			};
		} // LoadApplicationInfo

        public ActionMetaData AddHistoryDirector(Esigner Edirector)
        {
            return Execute<AddHistoryDirector>(null,null,Edirector);
        }

		public StringStringMapActionResult SetManualDecision(DecisionModel model) {
			ApplyManualDecision instance;

			int? customerID = (model != null) ? model.customerID : (int?)null;
			int? underwriterID = (model != null) ? model.underwriterID : (int?)null;

			ActionMetaData amd = ExecuteSync(out instance, customerID, underwriterID, model);

			return new StringStringMapActionResult {
				MetaData = amd,
				Map = new SortedDictionary<string, string> {
					{ "error", instance.Error },
					{ "warning", instance.Warning },
				},
			};
		} // SetManualDecision

		public MultiBrandLoanSummaryActionResult BuildMultiBrandLoanSummary(int customerID) {
			BuildMultiBrandLoanSummary instance;

			ActionMetaData amd = ExecuteSync(out instance, customerID, null, customerID);

			return new MultiBrandLoanSummaryActionResult {
				MetaData = amd,
				Summary = instance.Result,
			};
		} // BuildMultiBrandLoanSummary

		public DecisionHistoryResult LoadDecisionHistory(int customerID, int underwriterID) {
			Ezbob.Backend.Strategies.Misc.LoadDecisionHistory instance;

			ActionMetaData amd = ExecuteSync(out instance, customerID, underwriterID, customerID);

			return new DecisionHistoryResult {
				MetaData = amd,
				Model = instance.Result,
			};

		}

		public MessagesListActionResult LoadMessagesSentToUser(int userID) {
			LoadMessagesSentToUser instance;

			ActionMetaData amd = ExecuteSync(out instance, null, null, userID);

			return new MessagesListActionResult {
				MetaData = amd,
				Messages = instance.Result,
			};
		} // LoadMessagesSentToUser
	} // class EzServiceImplementation
} // namespace EzService
