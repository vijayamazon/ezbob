namespace EzBob.Web.Areas.Broker.Models {
	public class BrokerSignupModel {
		public string FirmName { get; set; }
		public string FirmRegNum { get; set; }
		public string ContactName { get; set; }
		public string ContactEmail { get; set; }
		public string ContactMobile { get; set; }
		public string MobileCode { get; set; }
		public string ContactOtherPhone { get; set; }
		public string Password { get; set; }
		public string Password2 { get; set; }
		public string FirmWebSite { get; set; }
		public int IsCaptchaEnabled { get; set; }
		public int TermsID { get; set; }
		public bool FCARegistered { get; set; }
		public string LicenseNumber { get; set; }
	} // class BrokerHomeModel
} // namespace
