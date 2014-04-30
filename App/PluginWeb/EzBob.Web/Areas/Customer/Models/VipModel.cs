namespace EzBob.Web.Areas.Customer.Models
{
	public class VipModel
	{
		public bool VipEnabled { get; set; }
		public bool IsRegistered { get; set; }
		public bool RequestedVip { get; set; }
		public string Ip { get; set; }
		public string VipEmail { get; set; }
		public string VipFullName { get; set; }
		public string VipPhone { get; set; }
		
	}
}