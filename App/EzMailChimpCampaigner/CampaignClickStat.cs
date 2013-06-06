namespace EzMailChimpCampaigner
{
	using System;
	using System.Collections.Generic;

	[Serializable]
    public class CampaignClickStat
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Email { get; set; }
        public string SendTime { get; set; }
        public int EmailsSent { get; set; }
        public int Clicks { get; set; }
    }

	[Serializable]
    public class CampaignClickStats
    {
        public readonly List<CampaignClickStat> campaignClickStatsList = new List<CampaignClickStat>();

        public void AddStat(string title, string url, string email, string sendTime, int emailsSent, int clicks)
        {
            campaignClickStatsList.Add(new CampaignClickStat
                {
                    Clicks = clicks,
                    Email = email,
                    EmailsSent = emailsSent,
                    SendTime = sendTime,
                    Title = title,
                    Url = url
                });
        }

        public List<CampaignClickStat> GetCampaignClickStatsList()
        {
            return campaignClickStatsList;
        }

        public int GetCampaignClickStatListCount()
        {
            return campaignClickStatsList.Count;
        }
    }
}
