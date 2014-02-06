﻿namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using System.Globalization;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class PayEarly : AMailStrategyBase {
		#region constructor

		public PayEarly(int customerId, decimal amount, string loanRefNumber, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
			this.amount = amount;
			this.loanRefNumber = loanRefNumber;
		} // constructor

		#endregion constructor

		public override string Name { get { return "Pay Early"; } } // Name

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Repayment confirmation";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName},
				{"AMOUNT", amount.ToString(CultureInfo.InvariantCulture)},
				{"DATE", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss").Replace('-','/')},
				{"RefNum", loanRefNumber}
			};
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndVariables

		private readonly decimal amount;
		private readonly string loanRefNumber;
	} // class PayEarly
} // namespace
