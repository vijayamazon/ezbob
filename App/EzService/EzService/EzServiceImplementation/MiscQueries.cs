namespace EzService.EzServiceImplementation {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using ActionResults;
	using EzBob.Backend.Models;
	using EzBob.Backend.Strategies.Experian;
	using EzBob.Backend.Strategies.Misc;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	partial class EzServiceImplementation {
		public SerializedDataTableActionResult GetSpResultTable(string spName, params string[] parameters) {
			GetSpResultTable strategyInstance;
			var result = ExecuteSync(out strategyInstance, null, null, spName, parameters);

			string serializedDataTable = JsonConvert.SerializeObject(strategyInstance.Result, new DataTableConverter());
			return new SerializedDataTableActionResult {
				MetaData = result,
				SerializedDataTable = serializedDataTable
			};
		} // GetSpResultTable

		private bool IsValidConfigTableInput(List<ConfigTable> configTableEntries, out SortedDictionary<int, ConfigTable> sortedEntries)
		{
			sortedEntries = new SortedDictionary<int, ConfigTable>();
			var sortedList = new List<ConfigTable>();
			foreach (ConfigTable entry in configTableEntries)
			{
				if (sortedEntries.ContainsKey(entry.Start))
				{
					string errorMessage = string.Format("Start must be unique:{0}", entry.Start);
					Log.Warn(errorMessage);
					return false;
				}
				sortedEntries.Add(entry.Start, entry);
			}

			bool isFirst = true;
			int highestSoFar = 0;
			foreach (int key in sortedEntries.Keys)
			{
				ConfigTable entry = sortedEntries[key];
				sortedList.Add(entry);
				entry.Value /= 100; // Convert to decimal number
				if (isFirst)
				{
					if (entry.Start != 0)
					{
						const string errorMessage = "Start must start at 0";
						Log.Warn(errorMessage);
						return false;
					}
					isFirst = false;
				}
				else
				{
					if (highestSoFar + 1 < entry.Start)
					{
						string errorMessage = string.Format("No range covers the numbers {0}-{1}", highestSoFar + 1, entry.Start - 1);
						Log.Warn(errorMessage);
						return false;
					}
					if (highestSoFar + 1 > entry.Start)
					{
						string errorMessage = string.Format("The numbers {0}-{1} are covered by more than one range", entry.Start, highestSoFar);
						Log.Warn(errorMessage);
						return false;
					}
				}
				highestSoFar = entry.End;
			}

			if (highestSoFar < 10000000)
			{
				string errorMessage = string.Format("No range covers the numbers {0}-10000000", highestSoFar);
				Log.Warn(errorMessage);
				return false;
			}

			if (highestSoFar > 10000000)
			{
				const string errorMessage = "Maximum allowed number is 10000000";
				Log.Warn(errorMessage);
				return false;
			}

			return true;
		}

		public BoolActionResult SaveConfigTable(List<ConfigTable> configTableEntries, ConfigTableType configTableType)
		{
			SortedDictionary<int, ConfigTable> sortedEntries;
			bool isValid = IsValidConfigTableInput(configTableEntries, out sortedEntries);

			if (isValid)
			{
				try
				{
					DB.ExecuteNonQuery(
						"ConfigTable_Refill",
						CommandSpecies.StoredProcedure,
						DB.CreateTableParameter<ConfigTable>("@TheList", sortedEntries.Values, objbir =>
							{
								var bir = (ConfigTable) objbir;
								return new object[] {bir.Start, bir.End, bir.Value};
							}),
						new QueryParameter("TableName", configTableType.ToString())
						);
				}
				catch (Exception e)
				{
					Log.Error("Exception occurred during execution of {0}. The exception:{1}", configTableType, e);
					isValid = false;
				}
			}

			return new BoolActionResult
			{
				Value = !isValid
			};
		}

		public IntActionResult GetCustomerStatusRefreshInterval()
		{
			int res = 1000;
			try
			{
				DataTable dt = DB.ExecuteReader("GetCustomerStatusRefreshInterval", CommandSpecies.StoredProcedure);
				var sr = new SafeReader(dt.Rows[0]);
				res = sr["RefreshInterval"];
			}
			catch (Exception e)
			{
				Log.Error("Exception occurred during calculation of customer's state. The exception:{0}", e);
			}

			return new IntActionResult
			{
				Value = res
			};
		}

		public StringActionResult GetCustomerState(int customerId)
		{
			string res = string.Empty;
			try
			{
				var instance = new GetAvailableFunds(DB, Log);
				instance.Execute();
				decimal availableFunds = instance.AvailableFunds;

				DataTable dt = DB.ExecuteReader("GetCustomerDetailsForStateCalculation", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));
				var sr = new SafeReader(dt.Rows[0]);

				int minLoanAmount = sr["MinLoanAmount"];
				string creditResult = sr["CreditResult"];
				string status = sr["Status"];
				bool isEnabled = sr["IsEnabled"];
				bool hasLateLoans = sr["HasLateLoans"];
				DateTime offerStart = sr["ApplyForLoan"];
				DateTime offerValidUntil = sr["ValidFor"];

				bool hasFunds = availableFunds >= minLoanAmount;

				if (!isEnabled)
				{
					res = "disabled";
				}
				else if (hasLateLoans)
				{
					res = "late";
				}
				else if (string.IsNullOrEmpty(creditResult) || creditResult == "WaitingForDecision")
				{
					res = "wait";
				}
				else if (status == "Rejected")
				{
					res = "bad";
				}
				else if (status == "Manual")
				{
					res = "wait";
				}
				else if (hasFunds && DateTime.UtcNow >= offerStart && DateTime.UtcNow <= offerValidUntil && status == "Approved")
				{
					res = "get";
				}
				else if (hasFunds && DateTime.UtcNow < offerStart && offerStart < offerValidUntil && status == "Approved")
				{
					res = "wait";
				}
				else if (!hasFunds || DateTime.UtcNow > offerValidUntil)
				{
					res = "apply";
				}
			}
			catch (Exception e)
			{
				Log.Error("Exception occurred during calculation of customer's state. The exception:{0}", e);
			}

			return new StringActionResult
			{
				Value = res
			};
		}

		public NullableDateTimeActionResult GetCompanySeniority(int customerId, int underwriterId)
		{
			GetCompanySeniority instance;

			ActionMetaData result = ExecuteSync(out instance, customerId, underwriterId, customerId);

			return new NullableDateTimeActionResult
			{
				MetaData = result,
				Value = instance.CompanyIncorporationDate
			};
		}

		public IntActionResult GetExperianAccountsCurrentBalance(int customerId, int underwriterId)
		{
			GetExperianAccountsCurrentBalance instance;

			ActionMetaData result = ExecuteSync(out instance, customerId, underwriterId, customerId);

			return new IntActionResult
			{
				MetaData = result,
				Value = instance.CurrentBalance
			};
		}

		public ActionMetaData BackfillFinancialAccounts()
		{
			BackfillFinancialAccounts instance;
			return ExecuteSync(out instance, 0, 0);
		}

		public ActionMetaData BackfillNonLimitedCompanies()
		{
			BackfillNonLimitedCompanies instance;
			return ExecuteSync(out instance, 0, 0);
		}
		
		public ActionMetaData BackfillConsumerAnalytics()
		{
			BackfillConsumerAnalytics instance;
			return ExecuteSync(out instance, 0, 0);
		}

		public ActionMetaData BackfillAml() // No analytics yet will be filled into customer table
		{
			BackfillAml instance;
			return ExecuteSync(out instance, 0, 0);
		}

		public ActionMetaData CalculateNewMedals()
		{
			CalculateNewMedals instance;
			return ExecuteSync(out instance, 0, 0);
		}
	} // class EzServiceImplementation
} // namespace EzService
