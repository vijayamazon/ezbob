using System;
using System.Collections.Generic;

namespace EzBob.Web.Areas.Underwriter.Models
{
	public class CampaignModel
	{
		public string Name { get; set; }
		public string Type { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Description { get; set; }
		public int Id { get; set; }
		public List<CampaignCustomerModel> Customers { get; set; }
	}

	public class CampaignCustomerModel
	{
		public int Id { get; set; }
		public string Email { get; set; }
		public string Name { get; set; }
	}
}