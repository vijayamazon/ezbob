namespace EzMailChimpCampaigner {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class DbCommands {

		public DbCommands(AConnection oDB, ASafeLog oLog) {
			m_oDB = oDB;
			m_oLog = new SafeLog(oLog);
		} // constructor

		public List<Subscriber> GetSubscriberList(Constants.CampaignsType campaign, bool bIncludeTest) {
			try {
				var fromDate = new DateTime();
				var toDate = new DateTime();

				var subscriberList = new List<Subscriber>();

				foreach (Constants.ConditionType condition in Enum.GetValues(typeof(Constants.ConditionType))) {
					var oSubscriberModel = new Subscriber();

					switch (condition) {
					case Constants.ConditionType.DayAfter:
						toDate = DateTime.Today;
						oSubscriberModel.DayAfter = DateTime.Today;
						break;

					case Constants.ConditionType.Week:
						toDate = DateTime.Today.AddDays(-7);
						oSubscriberModel.Week = DateTime.Today;
						break;

					case Constants.ConditionType.TwoWeeks:
						toDate = DateTime.Today.AddDays(-14);
						oSubscriberModel.TwoWeeks = DateTime.Today;
						break;

					case Constants.ConditionType.Month:
						toDate = DateTime.Today.AddMonths(-1);
						oSubscriberModel.Month = DateTime.Today;
						break;
					} // switch

					fromDate = toDate.AddDays(-1);

					var sp = new SpSelectCustomers(campaign, fromDate, toDate, bIncludeTest, m_oDB, m_oLog);

					sp.ForEachRowSafe((sr, bRowsetStart) => {
						decimal loanOffer = 0;

						string loanOfferStr = sr["MaxApproved"];

						if (!string.IsNullOrEmpty(loanOfferStr))
							decimal.TryParse(loanOfferStr, out loanOffer);
						else {
							string approximateLoanOfferStr = sr["ApproximateLoanOffer"];

							if (!string.IsNullOrEmpty(approximateLoanOfferStr))
								decimal.TryParse(approximateLoanOfferStr, out loanOffer);
						} // if

						var subscriber = oSubscriberModel.Clone();

						subscriber.Email = sr["eMail"];
						subscriber.BrokerEmail = sr["BrokerEmail"];
						subscriber.FirstName = sr["FirstName"];
						subscriber.LastName = sr["SurName"];
						subscriber.LoanOffer = loanOffer;
						subscriber.Group = campaign.ToString();

						subscriberList.Add(subscriber);

						return ActionResult.Continue;
					}); // for each result

					m_oLog.Debug("Added subscribers from db for campaign: {0} {1}", campaign, condition);
				} // for each condition

				return subscriberList;
			}
			catch (Exception e) {
				m_oLog.Error(e);
				return null;
			} // try
		} // GetSubscriberList

		public void AddCampaignClickStat(CampaignClickStat campaignClickStat) {
			m_oDB.ExecuteNonQuery("MC_AddCampaignClickStat",
				new QueryParameter("@Title", campaignClickStat.Title),
				new QueryParameter("@Url", campaignClickStat.Url),
				new QueryParameter("@Email", campaignClickStat.Email),
				new QueryParameter("@EmailsSent", campaignClickStat.EmailsSent),
				new QueryParameter("@Clicks", campaignClickStat.Clicks),
				new QueryParameter("@Date", campaignClickStat.SendTime)
			);
		} // AddCampaignClickStat

		public void DeleteCampaignClickStatsTable() {
			m_oDB.ExecuteNonQuery("DELETE FROM MC_CampaignClicks");
		} // DeleteCampaignClickStatsTable

		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

	} // class DbCommands
} // namespace
