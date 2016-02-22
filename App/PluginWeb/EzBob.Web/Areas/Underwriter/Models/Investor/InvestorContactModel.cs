namespace EzBob.Web.Areas.Underwriter.Models.Investor {
    using System;

    public class InvestorContactModel {
		public int InvestorContactID { get; set; }
		public string ContactPersonalName { get; set; }
		public string ContactLastName { get; set; }
		public string ContactEmail { get; set; }
		public string Role { get; set; }
		public string ContactMobile { get; set; }
		public string ContactOfficeNumber { get; set; }
		public string Comment { get; set; }
		public bool IsActive { get; set; }
		public bool IsPrimary { get; set; }
        public bool IsGettingReports { get; set; }
        public bool IsGettingAlerts { get; set; }
        public DateTime TimeStamp { get; set; }
	}
}