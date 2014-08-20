namespace EzMailChimpCampaigner
{
	using System.Collections.Generic;

	public static class Campaigns
	{
		public static List<Campaign> CampaignsList { get; set; }

		public static void InitCampaignsList()
		{
			CampaignsList = new List<Campaign>
				{
					new Campaign
						{
							CampaignType = Constants.CampaignsType.OnlyRegisteredEmail,
							DayList = GetDayList(Constants.CampaignsType.OnlyRegisteredEmail),
							ListId = Constants.EzbobCustomersListId,
							Title = Constants.FirstStepCustomersTitle,
						},
					new Campaign
						{
							CampaignType = Constants.CampaignsType.OnlyRegisteredStore,
							DayList = GetDayList(Constants.CampaignsType.OnlyRegisteredStore),
							ListId = Constants.EzbobCustomersListId,
							Title = Constants.SecondStepCustomersTitle,
						},
					new Campaign
						{
							CampaignType = Constants.CampaignsType.DidntTakeLoan,
							DayList = GetDayList(Constants.CampaignsType.DidntTakeLoan),
							ListId = Constants.EzbobCustomersListId,
							Title = Constants.LastStepCustomersTitle,
						}
				};
		}

		private static List<Day> GetDayList(Constants.CampaignsType type)
		{
			var dayList = new List<Day>();
			switch (type)
			{
				case Constants.CampaignsType.OnlyRegisteredEmail:
					//day after
					dayList.Add(new Day
						{
							Condition = Constants.DayAfterConditionField,
							TemplateId = Constants.FirstStepCustomersDayAfterTemplateId,
							Subject = Constants.FirstStepCustomersDayAfterSubject,
						});
					//week
					//dayList.Add(new Day
					//	{
					//		Condition = Constants.WeekConditionField,
					//		TemplateId = Constants.FirstStepCustomersWeekTemplateId,
					//		Subject = Constants.FirstStepCustomersWeekSubject
					//	});
					//dayList.Add(new Day
					//{
					//	Condition = Constants.ThreeDaysConditionField,
					//	TemplateId = Constants.FirstStepCustomersThreeDaysTemplateId,
					//	Subject = Constants.FirstStepCustomersThreeDaysSubject,
					//});
					break;
				case Constants.CampaignsType.OnlyRegisteredStore:
					//day after
					//dayList.Add(new Day
					//	{
					//		Condition = Constants.DayAfterConditionField,
					//		TemplateId = Constants.SecondStepCustomersDayAfterTemplateId,
					//		Subject = Constants.SecondStepCustomersDayAfterSubject,
					//	});
					//threeDays = new Day
					//{
					//	Condition = Constants.ThreeDaysConditionField,
					//	TemplateId = Constants.SecondStepCustomersThreeDaysTemplateId,
					//	Subject = Constants.SecondStepCustomersThreeDaysSubject,
					//};
					//week
					//dayList.Add(new Day
					//	{
					//		Condition = Constants.WeekConditionField,
					//		TemplateId = Constants.SecondStepCustomersWeekTemplateId,
					//		Subject = Constants.SecondStepCustomersWeekSubject,
					//	});
					////two weeks
					//dayList.Add(new Day
					//	{
					//		Condition = Constants.TwoWeeksConditionField,
					//		TemplateId = Constants.SecondStepCustomersTwoWeeksTemplateId,
					//		Subject = Constants.SecondStepCustomersTwoWeeksSubject,
					//	});
					//month
					//dayList.Add(new Day
					//	{
					//		Condition = Constants.MonthConditionField,
					//		TemplateId = Constants.SecondStepCustomersMonthTemplateId,
					//		Subject = Constants.SecondStepCustomersMonthSubject,
					//	});
					break;
				case Constants.CampaignsType.DidntTakeLoan:
					//day after
					dayList.Add(new Day
						{
							Condition = Constants.DayAfterConditionField,
							TemplateId = Constants.LastStepCustomersDayAfterTemplateId,
							Subject = Constants.LastStepCustomersDayAfterSubject,
						});
					//threeDays = new Day
					//{
					//	Condition = Constants.ThreeDaysConditionField,
					//	TemplateId = Constants.LastStepCustomersThreeDaysTemplateId,
					//	Subject = Constants.LastStepCustomersThreeDaysSubject,
					//};
					//week
					dayList.Add(new Day
						{
							Condition = Constants.WeekConditionField,
							TemplateId = Constants.LastStepCustomersWeekTemplateId,
							Subject = Constants.LastStepCustomersWeekSubject,
						});
					//two weeks
					dayList.Add(new Day
						{
							Condition = Constants.TwoWeeksConditionField,
							TemplateId = Constants.LastStepCustomersTwoWeeksTemplateId,
							Subject = Constants.LastStepCustomersTwoWeeksSubject,
						});
					//month
					dayList.Add(new Day
						{
							Condition = Constants.MonthConditionField,
							TemplateId = Constants.LastStepCustomersMonthTemplateId,
							Subject = Constants.LastStepCustomersMonthSubject,
						});
					break;
			}
			return dayList;
		}
	}
}
