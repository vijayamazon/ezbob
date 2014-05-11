namespace EzService.EzServiceImplementation {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using ActionResults;
	using EzBob.Backend.Models;
	using EzBob.Backend.Strategies;
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

		public BoolActionResult SaveConfigTable(List<ConfigTable> configTableEntries, ConfigTableType configTableType)
		{
			// TODO: Move validation in here instead of StrategySettingsController
			bool isError = false;
			try
			{
				DB.ExecuteNonQuery(
					"ConfigTable_Refill",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<ConfigTable>("@TheList", configTableEntries, objbir =>
					{
						var bir = (ConfigTable)objbir;
						return new object[] { bir.Start, bir.End, bir.Value };
					}),
					new QueryParameter("TableName", configTableType.ToString())
				);
			}
			catch (Exception e)
			{
				Log.Error("Exception occurred during execution of {0}. The exception:{1}", configTableType, e);
				isError = true;
			}

			return new BoolActionResult
			{
				Value = isError
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
				DataTable dt = DB.ExecuteReader("GetAvailableFunds", CommandSpecies.StoredProcedure);
				var sr = new SafeReader(dt.Rows[0]);
				decimal availableFunds = sr["AvailableFunds"];

				dt = DB.ExecuteReader("GetCustomerDetailsForStateCalculation", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));
				sr = new SafeReader(dt.Rows[0]);

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

		public DecimalActionResult GetLatestInterestRate(int customerId, int underwriterId)
		{
			GetLatestInterestRate instance;

			ActionMetaData result = ExecuteSync(out instance, customerId, underwriterId, customerId);

			return new DecimalActionResult
			{
				MetaData = result,
				Value = instance.LatestInterestRate
			};
		}

		public DateTimeActionResult GetCompanySeniority(int customerId, int underwriterId)
		{
			GetCompanySeniority instance;

			ActionMetaData result = ExecuteSync(out instance, customerId, underwriterId, customerId);

			return new DateTimeActionResult
			{
				MetaData = result,
				Value = instance.CompanyIncorporationDate.HasValue ? instance.CompanyIncorporationDate.Value : DateTime.UtcNow
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





		// This code is temporary, and will be only used to backfill data in the 18/2/2014 release - after that it can be deleted
		public ActionMetaData Temp_BackfillCompanyAnalytics()
		{
			Temp_BackfillCompanyAnalytics instance;
			return ExecuteSync(out instance, 0, 0);
		}
	} // class EzServiceImplementation
} // namespace EzService
