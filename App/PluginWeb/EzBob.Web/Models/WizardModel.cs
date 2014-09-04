namespace EzBob.Web.Models {
	using EZBob.DatabaseLib.Model.Database.Broker;
	using EzBob.Models;

	public class WizardModel {
		public CustomerModel Customer { get; set; }
		public WhiteLabelProvider WhiteLabel { get; set; }
	}
}