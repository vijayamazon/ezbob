﻿namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Globalization;
	using System.Collections.Generic;

	public class PayPointNameValidationFailed : AMailStrategyBase {

		public PayPointNameValidationFailed(int customerId, string cardHodlerName) : base(customerId, false) {
			this.cardHodlerName = cardHodlerName;
		} // constructor

		public override string Name { get { return "PayPoint Name Validation Failed"; } } // Name

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

		private readonly string cardHodlerName;
	} // class PayPointNameValidationFailed
} // namespace
