namespace EzMailChimpCampaigner
{
	using System.IO;
	using System.Xml.Serialization;
	using Logger;
	using System;
	using System.Collections.Generic;
	using MailChimp;
	using MailChimp.Types;
	using McCampaign = MailChimp.Types.Campaign;

	public static class MailChimpApiControler
	{
		static readonly string Apikey = System.Configuration.ConfigurationManager.AppSettings["apikey"];
		static readonly MCApi Mc = new MCApi(Apikey, true);

		static public MCList<List.ListsDataItem> GetLists()
		{
			List.Lists lists = Mc.Lists();
			Logger.Debug("lists" + lists.Data.Count);
			foreach (List.ListsDataItem list in lists.Data)
			{
				Logger.Debug(list.ListID + " " + list.Name);
			}
			return lists.Data;
		}

		static public void GetGroups(string listId)
		{
			MCList<List.InterestGrouping> groups = Mc.ListInterestGroupings(listId);
			Logger.DebugFormat("lists {0}", groups.Count);
			foreach (List.InterestGrouping list in groups)
			{
				Logger.DebugFormat("{0} {1}", list.ID, list.Name);
				foreach (List.InterestGroup group in list.Groups)
				{
					Logger.DebugFormat("{0} {1}", group.Name, group.Bit);
				}
			}
		}

		static public void ListBatchSubscribe(string listId, List<Subscriber> subscriberList)
		{
			var batch = new List<List.Merges>();
			foreach (Subscriber subscriber in subscriberList)
			{
				var group = new[] { subscriber.Group };
				var groups = new[] { new List.Grouping(int.Parse(Constants.SignUpProcessGroupId), group) };
				var entry = new List.Merges(subscriber.Email, List.EmailType.Html, groups);

				if (!string.IsNullOrEmpty(subscriber.FirstName)) { entry.Add(Constants.FirstNameField, subscriber.FirstName); }
				if (subscriber.LoanOffer > 0) { entry.Add(Constants.LoanOfferField, subscriber.LoanOffer); }
				if (subscriber.DayAfter.HasValue)
				{
					entry.Add(Constants.DayAfterConditionField, subscriber.DayAfter);
				}
				//if (subscriber.ThreeDays.HasValue)
				//{
				//	entry.Add(Constants.ThreeDaysConditionField, subscriber.ThreeDays);
				//}
				if (subscriber.Week.HasValue)
				{
					entry.Add(Constants.WeekConditionField, subscriber.Week);
				}
				if (subscriber.TwoWeeks.HasValue)
				{
					entry.Add(Constants.TwoWeeksConditionField, subscriber.TwoWeeks);
				}
				if (subscriber.Month.HasValue)
				{
					entry.Add(Constants.MonthConditionField, subscriber.Month);
				}
				batch.Add(entry);
			}

			var options = new List.SubscribeOptions
			{
				DoubleOptIn = false,
				EmailType = List.EmailType.Html,
				SendWelcome = false,
				UpdateExisting = true,
				ReplaceInterests = true
			};

			List.BatchSubscribe batchSubscribe = Mc.ListBatchSubscribe(listId, batch, options);
			if (batchSubscribe.ErrorCount > 0)
			{
				foreach (var error in batchSubscribe.Errors)
				{
					Logger.Error("List.BatchSubscribe error: " + error.Message + " email:" + error.Email + " code:" + error.Code);
				}
			}
			else
			{
				Logger.DebugFormat("ListBatchSubscribe successful. {0} subscribed", subscriberList.Count);
			}
		}

		static public string CreateSegmentedCampaign(string listId, int templateId, string condition, string subject, string title, string group)
		{
			try
			{
				var conditions = new MCList<McCampaign.SegmentCondition>
				{
					new McCampaign.SegmentCondition("interests-" + Constants.SignUpProcessGroupId, "one", @group),
					new McCampaign.SegmentCondition(condition, "eq", DateTime.Today.ToString("yyyy-MM-dd"))
				};
				var segmentOptions = new McCampaign.SegmentOptions(MailChimp.Types.Campaign.Match.AND, conditions);
				var options = new McCampaign.Options(listId, subject, Constants.FromEmail, Constants.FromEmailName, "");
				var analytics = new Input { { Constants.GoogleAnalyticsKie, Constants.GoogleAnalyticsValue } };
				options.Analytics = new Opt<Input>(analytics);
				options.TemplateID = templateId;
				options.Title = title + " " + condition + " " + DateTime.Today.ToShortDateString();

				var cBase = new McCampaign.Content.Base { Text = string.Empty };
				if (Mc.CampaignSegmentTest(listId, segmentOptions) > 0)
				{
					string campaignId = Mc.CampaignCreate(McCampaign.Type.Regular, options, cBase, segmentOptions);
					Logger.DebugFormat("Created Segmented Campaign {0}", campaignId);
					return campaignId;
				}

				Logger.Debug("No customers in this segment");
				return string.Empty;
			}
			catch (Exception ex)
			{
				Logger.ErrorFormat("CreateSegmentedCampaign error: " + ex);
				return null;
			}
		}

		static public void SendCampaign(string campaignId)
		{
			bool success = Mc.CampaignSendNow(campaignId);
			Logger.DebugFormat("Campaign {0} sent successfully: {1}", campaignId, success);
		}

		static public void LoadClickStatsToDb()
		{
			McCampaign.Campaigns campaigns;
			int page = 0;
			const int campaignsPerPage = 50;
			const int maxNumOfPages = 1000 / campaignsPerPage;

			var campaignClickStats = new CampaignClickStats();

			do
			{
				campaigns = Mc.Campaigns(
					new McCampaign.Filter { ListID = Constants.EzbobCustomersListId }, limit: campaignsPerPage, start: page);

				Logger.DebugFormat("num of campaigns {0} on page {1}", campaigns.Data.Count, page);

				foreach (MailChimp.Types.Campaign.CampaignsDataItem campaign in campaigns.Data)
				{
					//todo: for unsubscribers and opens: var stats = Mc.CampaignStats(campaign.ID);
					var clickStats = Mc.CampaignClickStats(campaign.ID);
					//Logger.DebugFormat("campaign {0},num of stats {1}", campaign.Title, clickStats.Count);
					foreach (string url in clickStats.Keys)
					{
						MailChimp.Types.Campaign.ReportData.ClickDetailAIM emails = Mc.CampaignClickDetailAIM(campaign.ID, url);
						MailChimp.Types.Campaign.Stats.ClickStats clicks = clickStats[url];
						if (emails.Data.Count == 0)
						{
							//	Logger.DebugFormat("campaign {0} with 0 clicks on url {1}", campaign.Title, url);
							campaignClickStats.AddStat(campaign.Title, url, "", campaign.SendTime.Value.ToString("yyyy-MM-dd"),
													   campaign.EmailsSent, clicks.Clicks);
						}
						foreach (var email in emails.Data)
						{
							//Logger.DebugFormat(
							//	"{0} campaign: {1} sent mails: {2} num of clicks(stats): {3} num of clicks(email): {4} emails clicks: {5}",
							//	url, campaign.Title, campaign.EmailsSent, clicks.Clicks, email.Clicks, email.Email);
							campaignClickStats.AddStat(campaign.Title, url, email.Email, campaign.SendTime.Value.ToString("yyyy-MM-dd"),
													   campaign.EmailsSent, clicks.Clicks);
						}
					}
				}
				page++;
			} while (campaigns.Data.Count > 0 && page < maxNumOfPages);

			/*
			campaignClickStats.AddStat("test", "test", "test", DateTime.Now.ToString("yyyy-MM-dd"),
													   0, 0);
			campaignClickStats.AddStat("test2", "test2", "test2", DateTime.Now.ToString("yyyy-MM-dd"),
													   0, 0);
			*/

			if (campaignClickStats.GetCampaignClickStatListCount() > 0)
			{
				try
				{
					SerializeToXml(campaignClickStats);
				}
				catch (Exception ex)
				{
					Logger.ErrorFormat("Can't serialize to file {0}", ex);
				}
				DbCommands.DeleteCampaignClickStatsTable();
				var campaignClickStatsList = campaignClickStats.GetCampaignClickStatsList();
				foreach (var campaignClickStat in campaignClickStatsList)
				{
					DbCommands.AddCampaignClickStat(campaignClickStat);
				}
			}
		}

		public static void SerializeToXml(CampaignClickStats stats)
		{
			var serializer = new XmlSerializer(typeof(CampaignClickStats));

			var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\stats.xml";
			using (var writer = new StreamWriter(path))
			{
				serializer.Serialize(writer, stats);
			}
		}

		#region Old/Test Code
		static public void GetCampaignAnalytics()
		{
			var page = 0;
			McCampaign.Campaigns campaigns;
			do
			{
				campaigns = Mc.Campaigns(new McCampaign.Filter { ListID = Constants.EzbobCustomersListId },
										 limit: 100, start: page);
				foreach (var c in campaigns.Data)
				{
					var stats = Mc.CampaignAnalytics(c.ID);
					Logger.Debug(c.Title + ": visits: " + stats.Visits + ", new visits: " + stats.NewVisits);
				}
				page++;
			} while (campaigns.Data.Count > 0);
		}

		static public List<Subscriber> GetTestSubscriberList()
		{
			var list = new List<Subscriber>
				{
					new Subscriber
						{
							Email = "stasdes@bk.ru",
							Group = "group1",
							FirstName = "Stas",
							LastName = "Dulman",
							LoanOffer = 15M,
							DayAfter = DateTime.Today, //.ToString("yyyy-MM-dd"),
							//ThreeDays = DateTime.Today,//.ToString("yyyy-MM-dd"),
							//Week = DateTime.Today,//.ToString("yyyy-MM-dd"),
						},
					new Subscriber
						{
							Email = "stasdes@gmail.com",
							Group = "group1",
							FirstName = "Stas",
							LastName = "Dulman",
							LoanOffer = 20M,
							//DayAfter = DateTime.Today,//.ToString("yyyy-MM-dd"),
							TwoWeeks = DateTime.Today, //.ToString("yyyy-MM-dd"),
							//Week = DateTime.Today,//.ToString("yyyy-MM-dd"),
						},
					new Subscriber
						{
							Email = "stasd@ezbob.com",
							Group = "group2",
							FirstName = "Stas",
							LastName = "Dulman",
							LoanOffer = 25M,
							//DayAfter = DateTime.Today,//.ToString("yyyy-MM-dd"),
							//ThreeDays = DateTime.Today,//.ToString("yyyy-MM-dd"),
							Week = DateTime.Today, //.ToString("yyyy-MM-dd"),
						}
				};

			return list;
		}

		//static public void UnsubscribeList(string listId)
		//{
		//    List<listMembersResults> list = getList(listId);
		//    foreach (listMembersResults member in list)
		//    {
		//        Unsubscribe(listId, member.email);
		//    }
		//}

		//static public void Unsubscribe(string listId, string email)
		//{
		//    listUnsubscribe cmd = new listUnsubscribe();
		//    listUnsubscribeParms newlistUnsubscribeParms = new listUnsubscribeParms
		//    {
		//        apikey = apikey,
		//        id = listId,
		//        email_address = email,
		//        delete_member = true,
		//        send_goodbye = false,
		//        send_notify = false,
		//    };
		//    listUnsubscribeInput newlistUnsubscribeInput = new listUnsubscribeInput(newlistUnsubscribeParms);
		//    var unsubscribeSuccess = cmd.Execute(newlistUnsubscribeInput);
		//    if (unsubscribeSuccess.api_Validate)
		//    {
		//        Logger.Debug(unsubscribeSuccess.api_ErrorMessages.ToString());
		//    }
		//    else
		//    {
		//        Logger.Debug(email + " successfully unsubscribed and deleted from list.");
		//    }
		//}

		//static public string CreateCampaign(string listId, int templateId)
		//{
		//    campaignCreateParms cParams = new campaignCreateParms();
		//    cParams.apikey = apikey;
		//    cParams.options = new campaignCreateOptions()
		//    {
		//        from_email = "stasd@ezbob.com",
		//        from_name = "From Stas 2",
		//        list_id = listId,
		//        subject = "Test Subject 2",
		//        template_id = templateId/*32429*/,
		//        title = "Test title 2"
		//    };
		//    cParams.type = EnumValues.campaign_type.regular;
		//    campaignCreateInput cInput = new campaignCreateInput(cParams);
		//    campaignCreate cCreate = new campaignCreate(cInput);
		//    campaignCreateOutput cOut = cCreate.Execute();
		//    if (cOut.api_ErrorMessages.Count > 0)
		//    {
		//        Logger.Debug(cOut.api_ErrorMessages.ToString() + " " + cOut.api_ValidatorMessages);
		//    }
		//    else
		//    {
		//        Logger.Debug("Campaign added successfully " + cOut.result);
		//    }
		//    return cOut.result;
		//}

		//static public List<listMembersResults> getList(string id)
		//{
		//    listMembersInput listIn = new listMembersInput(apikey, id, EnumValues.listMembers_status.subscribed, 0, 100);
		//    listIn.api_AccessType = EnumValues.AccessType.Serial;
		//    listIn.api_OutputType = EnumValues.OutputType.JSON;

		//    listMembers listMem = new listMembers(listIn);
		//    listMembersOutput listOut = listMem.Execute();
		//    Logger.Debug("Members:\nemail, timestamp");
		//    foreach (listMembersResults member in listOut.result)
		//    {
		//        Logger.Debug(member.email + " " + member.timestamp);
		//    }


		//    return listOut.result;
		//}

		//static private string getListTitle(string listId)
		//{
		//    List<listsResults> lists = GetLists() ;
		//    foreach (listsResults list in lists)
		//    {
		//        if (list.id == listId)
		//        {
		//            return list.name;
		//        }
		//    }
		//    return "name no found";

		//}

		//static private void Subscribe(string listId, string email)
		//{

		//    listSubscribe cmd = new listSubscribe();
		//    listSubscribeParms newlistSubscribeParms = new listSubscribeParms
		//    {
		//        apikey = apikey,
		//        id = listId,
		//        email_address = email,
		//        double_optin = false,
		//        email_type = EnumValues.emailType.html,
		//        replace_interests = true,
		//        send_welcome = true,
		//        update_existing = true,



		//    };
		//    listSubscribeInput newlistSubscribeInput = new listSubscribeInput(newlistSubscribeParms);
		//    var subscribeSuccess = cmd.Execute(newlistSubscribeInput);
		//    if (subscribeSuccess.api_Validate)
		//    {
		//        Logger.Debug(subscribeSuccess.api_ErrorMessages.ToString());
		//    }
		//    else
		//    {
		//        Logger.Debug(email + " successfully subscribed.");
		//    }


		//}


		//static public void getTemplate()
		//{
		//    campaignTemplatesInput cTemplateIn = new campaignTemplatesInput(apikey);
		//    campaignTemplates cTemplate = new campaignTemplates(cTemplateIn);
		//    campaignTemplatesOutput cTemplateOut = cTemplate.Execute();
		//    if (cTemplateOut.api_ErrorMessages.Count > 0)
		//    {
		//        Logger.Debug(cTemplateOut.api_ErrorMessages[0].ToString());
		//    }
		//    else
		//    {
		//        List<campaignTemplatesResults> cTemplateResult = cTemplateOut.result;

		//        foreach (campaignTemplatesResults res in cTemplateResult)
		//        {
		//            Logger.Debug(res.id + " " + res.name);
		//        }
		//    }
		//}

		///// <summary>
		///// Op(eration): eq (is) / gt (after) / lt (before)
		///// </summary>
		//static public void testSegment(string listId, int templateId, string condition, string subject, string title)
		//{
		//    Logger.Debug(condition + " " + subject + " " + title);

		//    campaignSegmentCondition con = new campaignSegmentCondition() { field = condition.ToUpper(), op = "eq", value = DateTime.Today.ToString("yyyy-MM-dd") };
		//    //campaignSegmentCondition con = new campaignSegmentCondition() { field = "THREEDAYS", op = "eq", value = DateTime.Today.ToString("yyyy-MM-dd") };
		//    //campaignSegmentCondition con = new campaignSegmentCondition() { field = "WEEEK", op = "eq", value = DateTime.Today.ToString("yyyy-MM-dd") };
		//    campaignSegmentOptions opt = new campaignSegmentOptions();
		//    opt.match = "all";
		//    opt.conditions.Add(con);
		//    campaignSegmentTestParms testParams = new campaignSegmentTestParms(apikey,listId, opt);
		//    campaignSegmentTestInput testIn = new campaignSegmentTestInput(testParams);
		//    campaignSegmentTest test = new campaignSegmentTest(testIn);
		//    campaignSegmentTestOutput testOut = test.Execute();
		//    if (testOut.api_ErrorMessages.Count > 0)
		//    {
		//        Logger.Debug(testOut.api_ErrorMessages[0].error);
		//    }
		//    else
		//    {
		//        Logger.Debug(testOut.result);
		//    }

		//}

		//static public void printListMergeVars(string list_id)
		//{

		//    //            listMergeVar varsIn = new listMergeVarAddInput(parms);
		//    listMergeVarsInput varsIn = new listMergeVarsInput(apikey, list_id);
		//    listMergeVars vars = new listMergeVars(varsIn);
		//    listMergeVarsOutput varsOut = vars.Execute();
		//    foreach (var var in varsOut.result)
		//    {
		//        Logger.Debug(var.name + " " + var.tag + " " + var.field_type);
		//    }

		//} 
		#endregion
	}
}
