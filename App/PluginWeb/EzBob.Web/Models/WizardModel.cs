namespace EzBob.Web.Models {
	using EzBob.Models;

	public class WizardModel {
		public CustomerModel Customer { get; set; }
		public WhiteLabelModel WhiteLabel { get; set; }
	}

	public class WhiteLabelModel {
		public string Name { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string Logo { get; set; }
		public string LeadingColor { get; set; }
		public string SecondoryColor { get; set; }
		public string FinishWizardText { get; set; }
		public string MobilePhoneTextMessage { get; set; }
		public string FooterText { get; set; }
		public string ConnectorsToEnable { get; set; }
	}
}