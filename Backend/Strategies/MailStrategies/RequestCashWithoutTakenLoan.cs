﻿namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class RequestCashWithoutTakenLoan : AMailStrategyBase {
		public RequestCashWithoutTakenLoan(int customerId, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
		} // constructor

		public override string Name { get {return "RequestCashWithoutTakenLoan"; } } // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Re-analyzing customer";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndSubjectAndVariables

	} // class RequestCashWithoutTakenLoan
} // namespace
