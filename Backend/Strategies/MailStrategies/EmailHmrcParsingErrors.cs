namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using System.Text;
	using ConfigManager;

	public class EmailHmrcParsingErrors : AMailStrategyBase {
		public EmailHmrcParsingErrors(
			int nCustomerID,
			int nCustomerMarketplaceID,
			SortedDictionary<string, string> oErrorsToEmail
		) : base(nCustomerID, false) {
			m_nCustomerMarketplaceID = nCustomerMarketplaceID;
			m_oErrorsToEmail = oErrorsToEmail;
		} // constructor

		public override string Name {
			get { return "EmailHmrcParsingErrors"; }
		} // Name

		protected override void SetTemplateAndVariables() {
			StringBuilder os = new StringBuilder();

			os.Append("<dl>");

			foreach (var pair in m_oErrorsToEmail)
				os.AppendFormat("<dt>{0}</dt><dd>{1}</dd>", pair.Key, pair.Value);

			os.Append("</dl>");

			Variables = new Dictionary<string, string> {
				{ "CustomerProfileLink", "https://" + CurrentValues.Instance.UnderwriterSite + "/Underwriter/Customers#profile/" + CustomerData.Id },
				{ "FirstName", CustomerData.FirstName },
				{ "LastName", CustomerData.Surname },
				{ "CustomerID", CustomerData.Id.ToString() },
				{ "CustomerMarketplaceID", m_nCustomerMarketplaceID.ToString() },
				{ "Errors", os.ToString() },
			};

			TemplateName = "Email HMRC parsing errors";
		} // SetTemplateAndVariables

		private readonly int m_nCustomerMarketplaceID;
		private readonly SortedDictionary<string, string> m_oErrorsToEmail;

	} // class EmailHmrcParsingErrors
} // namespace
