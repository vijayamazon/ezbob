namespace EzMailChimpCampaigner {
	using System.IO;
	using System.Xml.Serialization;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using MailChimp;
	using MailChimp.Types;
	using McCampaign = MailChimp.Types.Campaign;

	using Ezbob.Database;
	using Ezbob.Logger;
	using Microsoft.FSharp.Core;

    public class MailChimpApiControler {
		public MailChimpApiControler(AConnection oDB, ASafeLog oLog) {
			string sApikey = System.Configuration.ConfigurationManager.AppSettings["apikey"];
			m_oMcApi = new MCApi(sApikey, true);

			ms_oDB = oDB;
			ms_oLog = new SafeLog(oLog);

			DbCmd = new DbCommands(ms_oDB, ms_oLog);
		} // Init

		public DbCommands DbCmd { get; private set; }

		public MCList<List.ListsDataItem> GetLists() {
			List.Lists lists = m_oMcApi.Lists();

			ms_oLog.Debug("lists" + lists.Data.Count);

			foreach (List.ListsDataItem list in lists.Data)
				ms_oLog.Debug(list.ListID + " " + list.Name);

			return lists.Data;
		} // GetLists

		public void GetGroups(string listId) {
			MCList<List.InterestGrouping> groups = m_oMcApi.ListInterestGroupings(listId);

			ms_oLog.Debug("lists {0}", groups.Count);

			foreach (List.InterestGrouping list in groups) {
				ms_oLog.Debug("{0} {1}", list.ID, list.Name);

				foreach (List.InterestGroup group in list.Groups)
					ms_oLog.Debug("{0} {1}", group.Name, group.Bit);
			} // for each
		} // GetGroups

		private List.Merges SubscriberToEntry(Subscriber subscriber, Func<Subscriber, string> oExtractEmail) {
			var group = new[] { subscriber.Group };
			var groups = new[] { new List.Grouping(int.Parse(Constants.SignUpProcessGroupId), group) };

			var entry = new List.Merges(oExtractEmail(subscriber), List.EmailType.Html, groups);

			if (!string.IsNullOrEmpty(subscriber.FirstName))
				entry.Add(Constants.FirstNameField, subscriber.FirstName);

			if (subscriber.LoanOffer > 0)
				entry.Add(Constants.LoanOfferField, subscriber.LoanOffer);

			if (subscriber.DayAfter.HasValue)
				entry.Add(Constants.DayAfterConditionField, subscriber.DayAfter);

			//if (subscriber.ThreeDays.HasValue)
			//	entry.Add(Constants.ThreeDaysConditionField, subscriber.ThreeDays);

			if (subscriber.Week.HasValue)
				entry.Add(Constants.WeekConditionField, subscriber.Week);

			if (subscriber.TwoWeeks.HasValue)
				entry.Add(Constants.TwoWeeksConditionField, subscriber.TwoWeeks);

			if (subscriber.Month.HasValue)
				entry.Add(Constants.MonthConditionField, subscriber.Month);

			return entry;
		} // SubscriberToEntry

		public void ListBatchSubscribe(string listId, List<Subscriber> subscriberList) {
			var batch = new List<List.Merges>();

			foreach (Subscriber subscriber in subscriberList) {
				batch.Add(SubscriberToEntry(subscriber, s => s.Email));

				if (!string.IsNullOrWhiteSpace(subscriber.BrokerEmail))
					batch.Add(SubscriberToEntry(subscriber, s => s.BrokerEmail));
			} // for each subscriber

			var options = new List.SubscribeOptions {
				DoubleOptIn = false,
				EmailType = List.EmailType.Html,
				SendWelcome = false,
				UpdateExisting = true,
				ReplaceInterests = true,
			};

			List.BatchSubscribe batchSubscribe = m_oMcApi.ListBatchSubscribe(listId, batch, options);

			ms_oLog.Debug("ListBatchSubscribe Result: AddCount: {0} UpdateCount: {1} ErrorCount{2}.", batchSubscribe.AddCount, batchSubscribe.UpdateCount, batchSubscribe.ErrorCount);

			if (batchSubscribe.ErrorCount > 0) {
				foreach (var error in batchSubscribe.Errors)
					ms_oLog.Error("List.BatchSubscribe error: " + error.Message + " email:" + error.Email + " code:" + error.Code);
			}

		} // ListBatchSubscribe

		public string CreateSegmentedCampaign(string listId, int templateId, string condition, string subject, string title, string group) {
			try {
				var conditions = new MCList<McCampaign.SegmentCondition> {
					new McCampaign.SegmentCondition("interests-" + Constants.SignUpProcessGroupId, "one", @group),
					new McCampaign.SegmentCondition(condition, "eq", DateTime.Today.ToString("yyyy-MM-dd"))
				};

				var segmentOptions = new McCampaign.SegmentOptions(MailChimp.Types.Campaign.Match.AND, conditions);

				var options = new McCampaign.Options(listId, subject, Constants.FromEmail, Constants.FromEmailName, "");

				var analytics = new Input { { Constants.GoogleAnalyticsKie, Constants.GoogleAnalyticsValue } };

				options.Analytics = new Opt<Input>(analytics);
				options.TemplateID = templateId;
				options.Title = title + " " + condition + " " + DateTime.Today.ToShortDateString();

				var cBase = new McCampaign.Content.Base {
					Text = string.Empty
				};

				if (m_oMcApi.CampaignSegmentTest(listId, segmentOptions) > 0) {
					string campaignId = m_oMcApi.CampaignCreate(McCampaign.Type.Regular, options, cBase, segmentOptions);
					ms_oLog.Debug("Created Segmented Campaign {0} for {1} {2} {3}", campaignId, templateId, title, condition);
					return campaignId;
				} // if

				ms_oLog.Debug("No customers in this segment: {0} {1} {2}", templateId, title, condition);
				return string.Empty;
			}
			catch (Exception ex) {
				ms_oLog.Alert(ex, "CreateSegmentedCampaign error.");
				return null;
			} // try
		}

		public void SendCampaign(string campaignId) {

			if (m_oMcApi.CampaignSendNow(campaignId)) {
				ms_oLog.Debug("Campaign {0} sent successfully", campaignId);
			}
			else {
				ms_oLog.Error("Campaign {0} wasn't sent successfully", campaignId);
			}
		} // SendCampaign

		public void LoadClickStatsToDb() {
			McCampaign.Campaigns campaigns;
			int page = 0;
			const int campaignsPerPage = 50;
			const int maxNumOfPages = 1000 / campaignsPerPage;

			var campaignClickStats = new CampaignClickStats();

			do {
				campaigns = m_oMcApi.Campaigns(
					new McCampaign.Filter { ListID = Constants.EzbobCustomersListId }, limit: campaignsPerPage, start: page);

				ms_oLog.Debug("num of campaigns {0} on page {1}", campaigns.Data.Count, page);

				foreach (MailChimp.Types.Campaign.CampaignsDataItem campaign in campaigns.Data) {
					try {
						//todo: for unsubscribers and opens: var stats = m_oMcApi.CampaignStats(campaign.ID);
						var clickStats = m_oMcApi.CampaignClickStats(campaign.ID);

						//ms_oLog.Debug("campaign {0},num of stats {1}", campaign.Title, clickStats.Count);

						foreach (string url in clickStats.Keys) {
							MailChimp.Types.Campaign.ReportData.ClickDetailAIM emails = m_oMcApi.CampaignClickDetailAIM(campaign.ID, url);

							MailChimp.Types.Campaign.Stats.ClickStats clicks = clickStats[url];

							if (emails.Data.Count == 0) {
								//	ms_oLog.Debug("campaign {0} with 0 clicks on url {1}", campaign.Title, url);

								campaignClickStats.AddStat(
									campaign.Title,
									url,
									"",
									campaign.SendTime.Value.ToString("yyyy-MM-dd"),
									campaign.EmailsSent, clicks.Clicks
								);
							} // if

							foreach (var email in emails.Data) {
								//ms_oLog.Debug(
								//	"{0} campaign: {1} sent mails: {2} num of clicks(stats): {3} num of clicks(email): {4} emails clicks: {5}",
								//	url, campaign.Title, campaign.EmailsSent, clicks.Clicks, email.Clicks, email.Email);

								campaignClickStats.AddStat(campaign.Title, url, email.Email,
														   campaign.SendTime.Value.ToString("yyyy-MM-dd"),
														   campaign.EmailsSent, clicks.Clicks);
							} // for each
						} // for each
					}
					catch (Exception ex) {
						ms_oLog.Debug(ex, "can't load campaign stats for campaign {0}", campaign.ID);
					} // try
				} // for each

				page++;
			} while (campaigns.Data.Count > 0 && page < maxNumOfPages);

			/*
			campaignClickStats.AddStat("test", "test", "test", DateTime.Now.ToString("yyyy-MM-dd"), 0, 0);
			campaignClickStats.AddStat("test2", "test2", "test2", DateTime.Now.ToString("yyyy-MM-dd"), 0, 0);
			*/

			if (campaignClickStats.GetCampaignClickStatListCount() > 0) {
				try {
					SerializeToXml(campaignClickStats);
				}
				catch (Exception ex) {
					ms_oLog.Alert(ex, "Can't serialize to file.");
				} // try

				DbCmd.DeleteCampaignClickStatsTable();

				var campaignClickStatsList = campaignClickStats.GetCampaignClickStatsList();

				foreach (var campaignClickStat in campaignClickStatsList)
					DbCmd.AddCampaignClickStat(campaignClickStat);
			} // if
		} // LoadClickStatsToDb

		public void SerializeToXml(CampaignClickStats stats) {
			var serializer = new XmlSerializer(typeof(CampaignClickStats));

			var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\stats.xml";

			using (var writer = new StreamWriter(path)) {
				serializer.Serialize(writer, stats);
			} // using
		} // SerializeToXml

		public void StoreStatsToDbFromXml() {
			var serializer = new XmlSerializer(typeof(CampaignClickStats));

			var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\stats.xml";

			using (var reader = new StreamReader(path)) {
				var stats = (CampaignClickStats) serializer.Deserialize(reader);

				DbCmd.DeleteCampaignClickStatsTable();

				var campaignClickStatsList = stats.GetCampaignClickStatsList();

				foreach (var campaignClickStat in campaignClickStatsList)
					DbCmd.AddCampaignClickStat(campaignClickStat);
			} // using
		} // StoreStatsToDbFromXml

		public void GetCampaignAnalytics() {
			var page = 0;

			McCampaign.Campaigns campaigns;

			do {
				campaigns = m_oMcApi.Campaigns(new McCampaign.Filter { ListID = Constants.EzbobCustomersListId }, limit: 100, start: page);

				foreach (var c in campaigns.Data) {
					var stats = m_oMcApi.CampaignAnalytics(c.ID);
					ms_oLog.Debug(c.Title + ": visits: " + stats.Visits + ", new visits: " + stats.NewVisits);
				} // for each

				page++;
			} while (campaigns.Data.Count > 0);
		} // GetCampaignAnalytics

        public void UnsubscribeFromAllLists(string email) {
            MCList<string> lists = m_oMcApi.ListsForEmail(email);
            if(lists != null && lists.Any()){
                foreach (var list in lists) {
                    m_oMcApi.ListUnsubscribe(list, email, new FSharpOption<List.UnsubscribeOptions>(new List.UnsubscribeOptions {
                        DeleteMember = true,
                        SendGoodby = false,
                        SendNotify = false
                    }));
                }
            }
            
        }

		public List<Subscriber> GetTestSubscriberList() {
			return GetTestSubscriberListFor(
				"group1",
				"Alex",
				"Boursouk",
				"alexbo+campaign-__PATTERN__@ezbob.com",
				"alexbo+broker-campaing-__PATTERN__@ezbob.com"
			);
		} // GetTestSubscriberList

		private List<Subscriber> GetTestSubscriberListFor(
			string sGroup,
			string sFirstName,
			string sLastName,
			string sEmailTemplate,
			string sBrokerEmailTemplate
		) {
			var list = new List<Subscriber> {
				new Subscriber {
					Email = sEmailTemplate.Replace("__PATTERN__", "15M"),
					BrokerEmail = sBrokerEmailTemplate.Replace("__PATTERN__", "15M"),
					Group = sGroup,
					FirstName = sFirstName,
					LastName = sLastName,
					LoanOffer = 15M,
					DayAfter = DateTime.Today, //.ToString("yyyy-MM-dd"),
					//ThreeDays = DateTime.Today,//.ToString("yyyy-MM-dd"),
					//Week = DateTime.Today,//.ToString("yyyy-MM-dd"),
				},

				new Subscriber {
					Email = sEmailTemplate.Replace("__PATTERN__", "20M"),
					BrokerEmail = sBrokerEmailTemplate.Replace("__PATTERN__", "15M"),
					Group = sGroup,
					FirstName = sFirstName,
					LastName = sLastName,
					LoanOffer = 20M,
					//DayAfter = DateTime.Today,//.ToString("yyyy-MM-dd"),
					TwoWeeks = DateTime.Today, //.ToString("yyyy-MM-dd"),
					//Week = DateTime.Today,//.ToString("yyyy-MM-dd"),
				},

				new Subscriber {
					Email = sEmailTemplate.Replace("__PATTERN__", "25M"),
					BrokerEmail = sBrokerEmailTemplate.Replace("__PATTERN__", "15M"),
					Group = sGroup,
					FirstName = sFirstName,
					LastName = sLastName,
					LoanOffer = 25M,
					//DayAfter = DateTime.Today,//.ToString("yyyy-MM-dd"),
					//ThreeDays = DateTime.Today,//.ToString("yyyy-MM-dd"),
					Week = DateTime.Today, //.ToString("yyyy-MM-dd"),
				},
			};

			return list;
		} // GetTestSubscriberListFor

		//public void UnsubscribeList(string listId)
		//{
		//    List<listMembersResults> list = getList(listId);
		//    foreach (listMembersResults member in list)
		//    {
		//        Unsubscribe(listId, member.email);
		//    }
		//}

		//public void Unsubscribe(string listId, string email)
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
		//        ms_oLog.Debug(unsubscribeSuccess.api_ErrorMessages.ToString());
		//    }
		//    else
		//    {
		//        ms_oLog.Debug(email + " successfully unsubscribed and deleted from list.");
		//    }
		//}

		//public string CreateCampaign(string listId, int templateId)
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
		//        ms_oLog.Debug(cOut.api_ErrorMessages.ToString() + " " + cOut.api_ValidatorMessages);
		//    }
		//    else
		//    {
		//        ms_oLog.Debug("Campaign added successfully " + cOut.result);
		//    }
		//    return cOut.result;
		//}

		//public List<listMembersResults> getList(string id)
		//{
		//    listMembersInput listIn = new listMembersInput(apikey, id, EnumValues.listMembers_status.subscribed, 0, 100);
		//    listIn.api_AccessType = EnumValues.AccessType.Serial;
		//    listIn.api_OutputType = EnumValues.OutputType.JSON;

		//    listMembers listMem = new listMembers(listIn);
		//    listMembersOutput listOut = listMem.Execute();
		//    ms_oLog.Debug("Members:\nemail, timestamp");
		//    foreach (listMembersResults member in listOut.result)
		//    {
		//        ms_oLog.Debug(member.email + " " + member.timestamp);
		//    }

		//    return listOut.result;
		//}

		//private string getListTitle(string listId)
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

		//private void Subscribe(string listId, string email)
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
		//        ms_oLog.Debug(subscribeSuccess.api_ErrorMessages.ToString());
		//    }
		//    else
		//    {
		//        ms_oLog.Debug(email + " successfully subscribed.");
		//    }

		//}

		//public void getTemplate()
		//{
		//    campaignTemplatesInput cTemplateIn = new campaignTemplatesInput(apikey);
		//    campaignTemplates cTemplate = new campaignTemplates(cTemplateIn);
		//    campaignTemplatesOutput cTemplateOut = cTemplate.Execute();
		//    if (cTemplateOut.api_ErrorMessages.Count > 0)
		//    {
		//        ms_oLog.Debug(cTemplateOut.api_ErrorMessages[0].ToString());
		//    }
		//    else
		//    {
		//        List<campaignTemplatesResults> cTemplateResult = cTemplateOut.result;

		//        foreach (campaignTemplatesResults res in cTemplateResult)
		//        {
		//            ms_oLog.Debug(res.id + " " + res.name);
		//        }
		//    }
		//}

		///// <summary>
		///// Op(eration): eq (is) / gt (after) / lt (before)
		///// </summary>
		//public void testSegment(string listId, int templateId, string condition, string subject, string title)
		//{
		//    ms_oLog.Debug(condition + " " + subject + " " + title);

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
		//        ms_oLog.Debug(testOut.api_ErrorMessages[0].error);
		//    }
		//    else
		//    {
		//        ms_oLog.Debug(testOut.result);
		//    }

		//}

		//public void printListMergeVars(string list_id)
		//{

		//    //            listMergeVar varsIn = new listMergeVarAddInput(parms);
		//    listMergeVarsInput varsIn = new listMergeVarsInput(apikey, list_id);
		//    listMergeVars vars = new listMergeVars(varsIn);
		//    listMergeVarsOutput varsOut = vars.Execute();
		//    foreach (var var in varsOut.result)
		//    {
		//        ms_oLog.Debug(var.name + " " + var.tag + " " + var.field_type);
		//    }

		//} 

		private readonly MCApi m_oMcApi;

		private ASafeLog ms_oLog;
		private AConnection ms_oDB;
	} // class MailChimpApiController
}
