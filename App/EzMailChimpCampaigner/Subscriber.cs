namespace EzMailChimpCampaigner
{
    using System;

    public class Subscriber
    {
        public string Email { get; set; }
        public string Group { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal LoanOffer { get; set; }
        public DateTime? DayAfter { get; set; }
        public DateTime? ThreeDays { get; set; }
        public DateTime? Week { get; set; }
    }
}
