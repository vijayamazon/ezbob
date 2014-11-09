namespace EzMailChimpCampaigner {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	class Program {
		static void Main(string[] args) {
			foreach (string sArg in args) {
				if (sArg == "--include-test") {
					ms_bIncludeTest = true;
					break;
				} // if
			} // for each

			ms_oLog = new LegacyLog();
			ms_oLog.NotifyStart();

			ms_oLog.Debug("Include test customers: {0}", ms_bIncludeTest ? "yes" : "no");

			AConnection oDB = new SqlConnection(ms_oLog);

			var oMailChimpApiControler = new MailChimpApiControler(oDB, ms_oLog);

			try {
				bool retrieveStats = Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["retrieveStats"]);

				if (retrieveStats)
					oMailChimpApiControler.LoadClickStatsToDb();

				//MailChimpApiControler.StoreStatsToDbFromXml();
			}
			catch (Exception ex) {
				ms_oLog.Error(ex, "LoadClickStatsToDb failed {0}", ex.StackTrace);
			} // try

			ExecuteAutomaticMailChimp(oMailChimpApiControler);

			ms_oLog.NotifyStop();
			Environment.Exit(0);
		} // Main

		static void ExecuteAutomaticMailChimp(MailChimpApiControler oMailChimpApiControler) {
			Campaigns.InitCampaignsList();

			foreach (Campaign campaign in Campaigns.CampaignsList) {
				List<Subscriber> subscriberList = oMailChimpApiControler.DbCmd.GetSubscriberList(campaign.CampaignType, ms_bIncludeTest);

				if (subscriberList.Count == 0) {
					ms_oLog.Debug("subscriberList is empty {0}", campaign);
					continue;
				} // if

				ms_oLog.Debug("subscriberList has {0} customers", subscriberList.Count);

				PrintSubscribersList(campaign.Title, subscriberList);

				oMailChimpApiControler.ListBatchSubscribe(campaign.ListId, subscriberList);

				foreach (Day day in campaign.DayList) {
					if (day == null)
						continue;

					ms_oLog.Debug("CreateSegmentedCampaign listId:{0}, templateId:{1}, condition:{2}, subject:{3}, title:{4}, type:{5}", campaign.ListId, day.TemplateId, day.Condition, day.Subject, campaign.Title, campaign.CampaignType.ToString());
					string campaignId = oMailChimpApiControler.CreateSegmentedCampaign(campaign.ListId, day.TemplateId, day.Condition, day.Subject, campaign.Title, campaign.CampaignType.ToString());

					if (!string.IsNullOrEmpty(campaignId)) {
						ms_oLog.Debug("Sending campaign {0}, {1} {2}", campaignId, campaign.Title, day.Condition);
						oMailChimpApiControler.SendCampaign(campaignId);
					} // fi
					else {
						ms_oLog.Error("Failed to CreateSegmentedCampaign");
					}
				} // for each day
			} // for each campaign
		} // ExecuteAutomaticMailChimp

		static void PrintSubscribersList(string subject, IEnumerable<Subscriber> list) {
			ms_oLog.Debug(subject);

			foreach (Subscriber subscriber in list) {
				ms_oLog.Debug(subscriber.ToString());
				Console.WriteLine(subscriber);
			} // for each subscriber
		} // PrintSubscribersList

		private static ASafeLog ms_oLog;
		private static bool ms_bIncludeTest = false;

		#region Tests

		/*private static void TestGetSubscribers() {
			Campaigns.InitCampaignsList();

			foreach (Campaign campaign in Campaigns.CampaignsList) {
				List<Subscriber> subscriberList = DbCommands.GetSubscriberList(campaign.CampaignType);
				PrintSubscribersList(campaign.Title, subscriberList);
			}
		}

		static void Tests()
        {
            //ms_oLog.Debug((MailChimpApiControler.GetLists())[0].id);
            //MailChimpApiControler.testSegment();
            //MailChimpApiControler.printListMergeVars(Constants.LastStepCustomers_ListID);
            //MailChimpApiControler.testSegment();
            //MailChimpApiControler.UnsubscribeList((MailChimpApiControler.GetLists())[0].id);
            //MailChimpApiControler.ListBatchSubscribe((MailChimpApiControler.GetLists())[0].id, MailChimpApiControler.GetTestSubscriberList());

            //Campaigns.InitCampaignsList();
            //foreach (Campaign campaign in Campaigns.CampaignsList)
            //{
            //    //MailChimpApiControler.Unsubscribe(campaign.ListId, "stasdes@gmail.com");
            //    //MailChimpApiControler.Unsubscribe(campaign.ListId, "stasdes@bk.ru");
            //    //MailChimpApiControler.ListBatchSubscribe(campaign.ListId, MailChimpApiControler.GetTestSubscriberList());
            //    foreach (Day day in campaign.DayList)
            //    {
            //        //MailChimpApiControler.testSegment(campaign.ListId, day.TemplateId, day.Condition, campaign.Subject, campaign.Title);
            //        string campaignId = MailChimpApiControler.CreateSegmentedCampaign(campaign.ListId, day.TemplateId, day.Condition, campaign.Subject, campaign.Title);
            //        MailChimpApiControler.SendCampaign(campaignId);
            //    }
            //    MailChimpApiControler.Unsubscribe(campaign.ListId, "stasdes@gmail.com");
            //    MailChimpApiControler.Unsubscribe(campaign.ListId, "stasdes@bk.ru");
            //    MailChimpApiControler.Unsubscribe(campaign.ListId, "stasd@ezbob.com");
            //}

            //FileStream fs = new FileStream("log.txt", FileMode.OpenOrCreate);
            //StreamWriter sw = new StreamWriter(fs);
            //Logger.Debug.SetOut(sw);
            //Logger.Debug.WriteLine(DateTime.Now);

            //  MailChimpApiControler.GetLists();
            // MailChimpApiControler.ListBatchSubscribe("0715376399", MailChimpApiControler.GetTestSubscriberList());
            //   MailChimpApiControler.GetGroups(Constants.EzbobCustomersListId);
            // Logger.Debug.WriteLine(Constants.CampaignsType.DidntTakeLoan.ToString());
            //MailChimpApiControler.CreateSegmentedCampaign("0715376399", 32429, "DAYAFTER", "subjrxt", "titlre");
            //MailChimpApiControler.SendCampaign("5f506652c9");

            //sw.AutoFlush = true;
            //sw.Close();
        }*/

		public static void TestAlibaba(MailChimpApiControler oMailChimpApiControler)
		{
			Campaigns.InitCampaignsList();
			foreach (Campaign campaign in Campaigns.CampaignsList)
			{
				if (campaign.CampaignType == Constants.CampaignsType.DidntTakeLoanAlibaba) {
					var subscriberList = new List<Subscriber>() {
						new Subscriber() {
							Email = "stasd+alitest@ezbob.com",
							DayAfter = DateTime.Today,
							FirstName = "Stas",
							Group = Constants.CampaignsType.DidntTakeLoanAlibaba.ToString(),
							LastName = "Dulman",
							LoanOffer = 1000,
							Month = DateTime.Today,
							TwoWeeks = DateTime.Today,
							Week = DateTime.Today
						}
					};

					if (subscriberList.Count == 0) {
						ms_oLog.Debug("subscriberList is empty {0}", campaign);
						continue;
					} // if

					ms_oLog.Debug("subscriberList has {0} customers", subscriberList.Count);

					PrintSubscribersList(campaign.Title, subscriberList);

					oMailChimpApiControler.ListBatchSubscribe(campaign.ListId, subscriberList);

					foreach (Day day in campaign.DayList) {
						if (day == null)
							continue;

						ms_oLog.Debug("CreateSegmentedCampaign listId:{0}, templateId:{1}, condition:{2}, subject:{3}, title:{4}, type:{5}",
							campaign.ListId, day.TemplateId, day.Condition, day.Subject, campaign.Title, campaign.CampaignType.ToString());
						
						string campaignId = oMailChimpApiControler.CreateSegmentedCampaign(campaign.ListId, day.TemplateId, day.Condition,
						                                                                   day.Subject, campaign.Title,
						                                                                   campaign.CampaignType.ToString());

						if (!string.IsNullOrEmpty(campaignId)) {
							ms_oLog.Debug("Sending campaign {0}, {1} {2}", campaignId, campaign.Title, day.Condition);
							//oMailChimpApiControler.SendCampaign(campaignId);
						} // fi
						else {
							ms_oLog.Error("Failed to CreateSegmentedCampaign");
						}
					} // for each day
				}
			} // for each campaign
		}
		#endregion
	} // class Program
} // namesapce
