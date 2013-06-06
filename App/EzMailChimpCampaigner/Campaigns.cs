﻿namespace EzMailChimpCampaigner
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
            Day dayAfter = null;
            Day threeDays = null;
            Day week = null;
            switch (type)
            {
                case Constants.CampaignsType.OnlyRegisteredEmail:
                    dayAfter = new Day
                    {
                        Condition = Constants.DayAfterConditionField,
                        TemplateId = Constants.FirstStepCustomersDayAfterTemplateId,
                        Subject = Constants.FirstStepCustomersDayAfterSubject,
                    };
                    threeDays = new Day
                    {
                        Condition = Constants.ThreeDaysConditionField,
                        TemplateId = Constants.FirstStepCustomersThreeDaysTemplateId,
                        Subject = Constants.FirstStepCustomersThreeDaysSubject,
                    };
                    week = new Day
                    {
                        Condition = Constants.WeekConditionField,
                        TemplateId = Constants.FirstStepCustomersWeekTemplateId,
                        Subject = Constants.FirstStepCustomersWeekSubject,
                    };
                    break;
                case Constants.CampaignsType.OnlyRegisteredStore:
                    dayAfter = new Day
                    {
                        Condition = Constants.DayAfterConditionField,
                        TemplateId = Constants.SecondStepCustomersDayAfterTemplateId,
                        Subject = Constants.SecondStepCustomersDayAfterSubject,
                    };
                    threeDays = new Day
                    {
                        Condition = Constants.ThreeDaysConditionField,
                        TemplateId = Constants.SecondStepCustomersThreeDaysTemplateId,
                        Subject = Constants.SecondStepCustomersThreeDaysSubject,
                    };
                    week = new Day
                    {
                        Condition = Constants.WeekConditionField,
                        TemplateId = Constants.SecondStepCustomersWeekTemplateId,
                        Subject = Constants.SecondStepCustomersWeekSubject,
                    };
                    break;
                case Constants.CampaignsType.DidntTakeLoan:
                    dayAfter = new Day
                    {
                        Condition = Constants.DayAfterConditionField,
                        TemplateId = Constants.LastStepCustomersDayAfterTemplateId,
                        Subject = Constants.LastStepCustomersDayAfterSubject,
                    };
                    threeDays = new Day
                    {
                        Condition = Constants.ThreeDaysConditionField,
                        TemplateId = Constants.LastStepCustomersThreeDaysTemplateId,
                        Subject = Constants.LastStepCustomersThreeDaysSubject,
                    };
                    week = new Day
                    {
                        Condition = Constants.WeekConditionField,
                        TemplateId = Constants.LastStepCustomersWeekTemplateId,
                        Subject = Constants.LastStepCustomersWeekSubject,
                    };
                    break;
            }

            dayList.Add(dayAfter);
            dayList.Add(threeDays);
            dayList.Add(week);
            return dayList;
        }
    }
}
