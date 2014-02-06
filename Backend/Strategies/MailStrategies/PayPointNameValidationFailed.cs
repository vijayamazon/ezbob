namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Globalization;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class PayPointNameValidationFailed : AMailStrategyBase {
		#region constructor

		public PayPointNameValidationFailed(int customerId, string cardHodlerName, AConnection oDb, ASafeLog oLog) : base(customerId, false, oDb, oLog) {
			this.cardHodlerName = cardHodlerName;
		} // constructor

		#endregion constructor

		public override string Name { get { return "PayPoint Name Validation Failed"; } } // Name

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - PayPoint data differs";

			Variables = new Dictionary<string, string> {
				{"E-mail", CustomerData.Mail},
				{"UserId", CustomerId.ToString(CultureInfo.InvariantCulture)},
				{"Name", CustomerData.FirstName},
				{"Surname", CustomerData.Surname},
				{"PayPointName", cardHodlerName}
			};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		private readonly string cardHodlerName;
	} // class PayPointNameValidationFailed
} // namespace
