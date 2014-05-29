namespace EzBob.Models.Marketplaces {
	using Web.Areas.Underwriter.Models;

	public class PayPalAccountModel {
		public PaymentAccountsModel GeneralInfo { get; set; }
		public PayPalAccountInfoModel PersonalInfo { get; set; }
	} // class PayPalAccountModel
} // namespace
