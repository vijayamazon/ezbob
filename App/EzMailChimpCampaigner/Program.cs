namespace EzMailChimpCampaigner
{
	using System;
	using System.Collections.Generic;
	using Logger;

	class Program
	{
		static void Main()
		{
			try
			{
				MailChimpApiControler.LoadClickStatsToDb();
			}
			catch (Exception ex)
			{
				Logger.ErrorFormat("LoadClickStatsToDb failed {0} {1}", ex, ex.StackTrace);
			}
			finally
			{
				ExecuteAutomaticMailChimp();
			}
			Environment.Exit(0);
		}

		static void ExecuteAutomaticMailChimp()
		{
			Campaigns.InitCampaignsList();

			foreach (Campaign campaign in Campaigns.CampaignsList)
			{
				List<Subscriber> subscriberList = DbCommands.GetSubscriberList(campaign.CampaignType);
				if (subscriberList.Count == 0)
				{
					Logger.DebugFormat("subscriberList is empty {0}", campaign);
					continue;
				}

				Logger.DebugFormat("subscriberList is has {0} customers", subscriberList.Count);
				PrintSubscrebsList(campaign.Title, subscriberList);
				MailChimpApiControler.ListBatchSubscribe(campaign.ListId, subscriberList);
				foreach (Day day in campaign.DayList)
				{
					string campaignId = MailChimpApiControler.CreateSegmentedCampaign(campaign.ListId, day.TemplateId, day.Condition, day.Subject, campaign.Title, campaign.CampaignType.ToString());
					if (!string.IsNullOrEmpty(campaignId))
					{
						Logger.DebugFormat("Sending campaign {0}, {1} {2}", campaignId, campaign.Title, day.Condition);
						MailChimpApiControler.SendCampaign(campaignId);
					}
				}
			}
		}

		static void PrintSubscrebsList(string subject, IEnumerable<Subscriber> list)
		{
			Logger.Debug(subject);
			foreach (Subscriber subscriber in list)
			{
				Logger.DebugFormat("Email: {0}\n Loan: {1} Name: {2} DayAfter:{3} Week:{4} TwoWeeks:{5} Month: {6}", subscriber.Email, subscriber.LoanOffer, subscriber.FirstName, subscriber.DayAfter, subscriber.Week, subscriber.TwoWeeks, subscriber.Month);
				Console.WriteLine("Email: {0}\n Loan: {1} Name: {2} DayAfter:{3} Week:{4} TwoWeeks:{5} Month: {6}", subscriber.Email, subscriber.LoanOffer, subscriber.FirstName, subscriber.DayAfter, subscriber.Week, subscriber.TwoWeeks, subscriber.Month);
			}
		}

		#region Tests
		/*static void Tests()
        {
            //Logger.Debug.WriteLine((MailChimpApiControler.GetLists())[0].id);
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
		#endregion
	}
}
