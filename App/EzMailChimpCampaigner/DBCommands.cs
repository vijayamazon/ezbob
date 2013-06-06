namespace EzMailChimpCampaigner
{
	using Ezbob.Logger;
	using Logger;
	using System.Collections.Generic;
	using System;
	using System.Data;
	using Ezbob.Database;
	using SqlConnection = Ezbob.Database.SqlConnection;

	static class DbCommands
	{
		public static List<Subscriber> GetSubscriberList(Constants.CampaignsType campaign)
		{
			try
			{
				string sqlCmdStr = string.Empty;
				var fromDate = new DateTime();
				var toDate = new DateTime();
				var subscriberList = new List<Subscriber>();
				foreach (Constants.ConditionType condition in Enum.GetValues(typeof(Constants.ConditionType)))
				{
					switch (condition)
					{
						case Constants.ConditionType.DayAfter:
							fromDate = DateTime.Today.AddDays(-1);
							toDate = DateTime.Today;
							break;
						case Constants.ConditionType.ThreeDays:
							fromDate = DateTime.Today.AddDays(-4);
							toDate = DateTime.Today.AddDays(-3);
							break;
						case Constants.ConditionType.Week:
							fromDate = DateTime.Today.AddDays(-8);
							toDate = DateTime.Today.AddDays(-7);
							break;
					}

					switch (campaign)
					{
						case Constants.CampaignsType.OnlyRegisteredEmail:
							sqlCmdStr = string.Format(Constants.GetFirstStepCustomersSp, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"));
							break;
						case Constants.CampaignsType.OnlyRegisteredStore:
							sqlCmdStr = string.Format(Constants.GetSecondStepCustomersSp, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"));
							break;
						case Constants.CampaignsType.DidntTakeLoan:
							sqlCmdStr = string.Format(Constants.GetLastStepCustomersSp, fromDate.AddDays(-1).ToString("yyyy-MM-dd"), toDate.AddDays(-1).ToString("yyyy-MM-dd"));
							break;
					}
					

					var log = new LegacyLog();
					var conn = new SqlConnection(log);
					DataTable dt = conn.ExecuteReader(sqlCmdStr);

					foreach (DataRow row in dt.Rows)
					{
						AddSubscreberToList(ref subscriberList, row, condition, campaign);
					}
					Logger.DebugFormat("Added subscrubers from db for campaign:{0} {1}", campaign, condition);
				}

				return subscriberList;
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
				return null;
			}
		}

		private static void AddSubscreberToList(ref List<Subscriber> subscriberList, DataRow row, Constants.ConditionType condition, Constants.CampaignsType type)
		{
			string email = row["eMail"].ToString();

			string firstName = null;
			string lastName = null;
			decimal loanOffer = 0;
			string loanOfferStr = null;
			string approximateLoanOfferStr = null;

			if (row.Table.Columns.Count == 2)
			{
				approximateLoanOfferStr = row["ApproximateLoanOffer"].ToString();
			}
			if (row.Table.Columns.Count == 4)
			{
				firstName = row["FirstName"].ToString();
				lastName = row["SurName"].ToString();
				loanOfferStr = row["MaxApproved"].ToString();
			}

			if (!string.IsNullOrEmpty(approximateLoanOfferStr))
			{
				decimal.TryParse(approximateLoanOfferStr, out loanOffer);
			}

			if (!string.IsNullOrEmpty(loanOfferStr))
			{
				decimal.TryParse(loanOfferStr, out loanOffer);
			}
			var subscriber = new Subscriber
			{
				Email = email,
				FirstName = firstName,
				LastName = lastName,
				LoanOffer = loanOffer,
				Group = type.ToString(),
			};

			switch (condition)
			{
				case Constants.ConditionType.DayAfter:
					subscriber.DayAfter = DateTime.Today;
					break;
				case Constants.ConditionType.ThreeDays:
					subscriber.ThreeDays = DateTime.Today;
					break;
				case Constants.ConditionType.Week:
					subscriber.Week = DateTime.Today;
					break;
			}
			subscriberList.Add(subscriber);
		}

		public static void AddCampaignClickStat(CampaignClickStat campaignClickStat)
		{
			var log = new LegacyLog();

			var conn = new SqlConnection(log);
			conn.ExecuteNonQuery("MC_AddCampaignClickStat",
				new QueryParameter("@Title", campaignClickStat.Title),
				new QueryParameter("@Url", campaignClickStat.Url),
				new QueryParameter("@Email", campaignClickStat.Email),
				new QueryParameter("@EmailsSent", campaignClickStat.EmailsSent),
				new QueryParameter("@Clicks", campaignClickStat.Clicks),
				new QueryParameter("@Date", campaignClickStat.SendTime));
		}

		public static void DeleteCampaignClickStatsTable()
		{
			var log = new LegacyLog();
			var conn = new SqlConnection(log);
			conn.ExecuteNonQuery("DELETE FROM MC_CampaignClicks");
		}
	}
}
