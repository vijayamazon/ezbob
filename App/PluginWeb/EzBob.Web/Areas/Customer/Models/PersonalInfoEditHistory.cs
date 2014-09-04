using System.Collections.Generic;

namespace EzBob.Web.Areas.Customer.Models
{
	using EZBob.DatabaseLib.Model.Database;

	public class PersonalInfoHistoryParameter
    {
        public string DaytimePhone { get; set; }
        public string MobilePhone { get; set; }
        public string BusinessPhone { get; set; }
        public string OverallTurnOver { get; set; }
		public string WebSiteTurnover { get; set; }
		public List<CustomerAddress> PersonalAddress { get; set; }
		public List<CustomerAddress> OtherPropertiesAddresses { get; set; }
        public List<CustomerAddress> CompanyAddress { get; set; }
    }
}