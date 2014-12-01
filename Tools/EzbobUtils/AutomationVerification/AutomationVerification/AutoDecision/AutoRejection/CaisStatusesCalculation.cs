namespace AutomationCalculator.AutoDecision.AutoRejection
{
	using System;
	using System.Collections.Generic;
	using Common;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class CaisStatusesCalculation
	{
		public CaisStatusesCalculation(AConnection db, ASafeLog log)
		{
			Log = log;
			DB = db;
		}

		public List<CaisStatus> GetConsumerCaisStatuses(int customerId) {
			var dbHelper = new DbHelper(DB, Log);
			return dbHelper.GetCustomerCaisStatuses(customerId);
		}

		public List<CaisStatus> GetBusinessCaisStatuses(int customerId)
		{
			var dbHelper = new DbHelper(DB, Log);
			return dbHelper.GetBusinessCaisStatuses(customerId);
		}

		public ConsumerLatesModel GetLates(int customerId, DateTime asOfDate, int minLateStatus, int lastMonthStatuses, List<CaisStatus> caisStatuses = null) {
			if (caisStatuses == null) {
				caisStatuses = GetConsumerCaisStatuses(customerId);
			}
			const decimal monthDays = 365.0M / 12.0M;

			var numOfLates = 0;
			var lateDays = 0;

			foreach (var caisStatus in caisStatuses) {
				var monthSinceUpdate = (decimal)(asOfDate - caisStatus.LastUpdatedDate).TotalDays / monthDays;
				int useLastStatusMonths = 0;
				if (monthSinceUpdate < 2) {
					useLastStatusMonths = lastMonthStatuses;
				}
				else {
					if (monthSinceUpdate > lastMonthStatuses) {
						useLastStatusMonths = 0;
					}
					if ((int) monthSinceUpdate == lastMonthStatuses) {
						useLastStatusMonths = 1;
					}
					if (monthSinceUpdate < lastMonthStatuses) {
						useLastStatusMonths = lastMonthStatuses - (int)monthSinceUpdate;
					}
				}
				bool isLateInAccount = false;
				for (int i = 0; i < useLastStatusMonths; ++i) {
					if (caisStatus.AccountStatusCodes.Length - i > 0) {
						string status = caisStatus.AccountStatusCodes[caisStatus.AccountStatusCodes.Length - i - 1].ToString();
						var accountStatus = AccountStatusDictionary.GetAccountStatus(status);
						if (accountStatus.IsLate && accountStatus.LateStatus > minLateStatus) {
							isLateInAccount = true;
							lateDays = lateDays < accountStatus.LateDays ? accountStatus.LateDays : lateDays;
						}
					}
				}

				if (isLateInAccount) {
					numOfLates++;
				}
			}

			return new ConsumerLatesModel
			{
				LateDays = lateDays,
				NumOfLates = numOfLates
			};

		}

		public DefaultsModel GetDefaults(int customerId, DateTime asOfDate, int minAmount, int lastMonthStatuses, List<CaisStatus> caisStatuses = null)
		{
			if (caisStatuses == null)
			{
				caisStatuses = GetConsumerCaisStatuses(customerId);
			}
			const decimal monthDays = 365.0M / 12.0M;

			var numOfDefaults = 0;
			var defaultsAmount = 0;

			foreach (var caisStatus in caisStatuses)
			{
				var monthSinceUpdate = (decimal)(asOfDate - caisStatus.LastUpdatedDate).TotalDays / monthDays;
				int useLastStatusMonths = 0;
				if (monthSinceUpdate > lastMonthStatuses)
				{
					useLastStatusMonths = 0;
				}
				else {
					useLastStatusMonths = lastMonthStatuses - (int)monthSinceUpdate;
				}
				bool isDefaultInAccount = false;
				
				for (int i = 0; i < useLastStatusMonths; ++i)
				{
					if (caisStatus.AccountStatusCodes.Length - i > 0)
					{
						string status = caisStatus.AccountStatusCodes[caisStatus.AccountStatusCodes.Length - i - 1].ToString();
						var accountStatus = AccountStatusDictionary.GetAccountStatus(status);
						if (accountStatus.IsDefault && caisStatus.Balance > minAmount)
						{
							isDefaultInAccount = true;
						}
					}
				}

				if (isDefaultInAccount)
				{
					numOfDefaults++;
					defaultsAmount += caisStatus.Balance;
				}
			}

			return new DefaultsModel
			{
				DefaultsAmount = defaultsAmount,
				NumOfDefaults = numOfDefaults
			};

		}

		protected readonly ASafeLog Log;
		protected readonly AConnection DB;
	}

	public class ConsumerLatesModel
	{
		public int NumOfLates { get; set; }
		public int LateDays { get; set; }
	}

	public class DefaultsModel
	{
		public int NumOfDefaults { get; set; }
		public int DefaultsAmount { get; set; }
	}

	public class CaisAccountStatus
	{
		public string ShortDescription { get; set; }
		public int LateStatus { get; set; }
		public int LateDays { get; set; }
		public bool IsLate { get; set; }
		public bool IsDefault { get; set; }
	}

	public static class AccountStatusDictionary
	{
		static AccountStatusDictionary()
		{
			ms_oAccountStatuses = new SortedDictionary<string, CaisAccountStatus> {
				{" ", new CaisAccountStatus{ShortDescription = "&nbsp;", }},
				{"0", new CaisAccountStatus{ShortDescription = "OK", } },
				{"1", new CaisAccountStatus{ShortDescription = "30",  IsLate = true, LateStatus = 1, LateDays = 30}},
				{"2", new CaisAccountStatus{ShortDescription = "60",  IsLate = true, LateStatus = 2, LateDays = 60 }},
				{"3", new CaisAccountStatus{ShortDescription = "90",  IsLate = true, LateStatus = 3, LateDays = 90 }},
				{"4", new CaisAccountStatus{ShortDescription = "120", IsLate = true, LateStatus = 4, LateDays = 120 }},
				{"5", new CaisAccountStatus{ShortDescription = "150", IsLate = true, LateStatus = 5, LateDays = 150 }},
				{"6", new CaisAccountStatus{ShortDescription = "180", IsLate = true, LateStatus = 6, LateDays = 180 }},
				{"8", new CaisAccountStatus{ShortDescription = "Def", IsDefault = true}},
				{"9", new CaisAccountStatus{ShortDescription = "Bad", IsDefault = true}},
				{"S", new CaisAccountStatus{ShortDescription = "Slow" }},
				{"U", new CaisAccountStatus{ShortDescription = "U" }},
				{"D", new CaisAccountStatus{ShortDescription = "Dorm" }},
				{"?", new CaisAccountStatus{ShortDescription = "Unknown" }}
			};
		} // static constructor

		public static CaisAccountStatus GetAccountStatus(string accStatusIndicator)
		{
			return LookFor(accStatusIndicator, ms_oAccountStatuses);
		} // GetAccountStatusString


		private static CaisAccountStatus LookFor(string sNeedle, SortedDictionary<string, CaisAccountStatus> oHaystack)
		{
			if (string.IsNullOrEmpty(sNeedle))
			{
				return new CaisAccountStatus { ShortDescription = string.Format("Status: ({0}) not found", sNeedle) };
			}
			return oHaystack.ContainsKey(sNeedle) ? oHaystack[sNeedle] : new CaisAccountStatus { ShortDescription = string.Format("Status: ({0}) not found", sNeedle) };
		} // LookFor


		private static readonly SortedDictionary<string, CaisAccountStatus> ms_oAccountStatuses;

	} // class AccountStatusDictionary
}
