﻿namespace EzMailChimpCampaigner {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class DbCommands {
		#region public

		#region constructor

		public DbCommands(AConnection oDB, ASafeLog oLog) {
			m_oDB = oDB;
			m_oLog = new SafeLog(oLog);
		} // constructor

		#endregion constructor

		#region method GetSubscriberList

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

					//case Constants.ConditionType.ThreeDays:
					//	fromDate = DateTime.Today.AddDays(-4);
					//	toDate = DateTime.Today.AddDays(-3);
					//	oSubscriberModel.ThreeDays = DateTime.Today;
					//	break;

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
					//subscriberList.Add(new Subscriber()
					//	{
					//		Group = campaign.ToString(),
					//		Email = "adic@ezbob.com",
					//		FirstName = "Adi",
					//		LastName = "Cohen",
					//		LoanOffer = 1500,
					//		DayAfter = DateTime.Today,
					//		Month = DateTime.Today,
					//		TwoWeeks = DateTime.Today,
					//		Week = DateTime.Today
					//	});
				} // for each condition

				return subscriberList;
			}
			catch (Exception e) {
				m_oLog.Error(e);
				return null;
			} // try
		} // GetSubscriberList

		#endregion method GetSubscriberList

		#region method AddCampaignClickStat

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

		#endregion method AddCampaignClickStat

		#region method DeleteCampaignClickStatsTable

		public void DeleteCampaignClickStatsTable() {
			m_oDB.ExecuteNonQuery("DELETE FROM MC_CampaignClicks");
		} // DeleteCampaignClickStatsTable

		#endregion method DeleteCampaignClickStatsTable

		#region private

		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		#endregion private

		#endregion public
	} // class DbCommands
} // namespace
