namespace EzMailChimpCampaigner
{
    using System.Collections.Generic;

    public class Campaign
    {
        public string ListId { get; set; }
        public List<Day> DayList { get; set; }
        public Constants.CampaignsType CampaignType { get; set; }
        public string Title { get; set; }
    }
}
