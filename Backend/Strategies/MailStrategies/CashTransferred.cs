﻿namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Globalization;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class CashTransferred : ABrokerMailToo {
		#region constructor

		public CashTransferred(int customerId, decimal amount, string loanRefNum, bool isFirst, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog, true) {
			this.amount = amount;
			this.loanRefNum = loanRefNum;
			this.isFirst = isFirst;
			toTrustPilot = true;
		} // constructor

		#endregion consturctor

		public override string Name { get { return "Cash Transferred"; } }

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName},
				{"Amount", amount.ToString(CultureInfo.InvariantCulture)},
				{"EMAIL", CustomerData.Mail},
				{"LOANREFNUM", loanRefNum}
			};

			if (isFirst)
			{
				if (CustomerData.IsCampaign)
				{
					TemplateName = "Mandrill - Took Loan Campaign (1st loan)";
				}
				else
				{
					TemplateName = CustomerData.IsAlibaba ? "Mandrill - Alibaba - Took Loan (1st loan)" : "Mandrill - Took Loan (1st loan)";
				}
			}
			else
			{
				TemplateName = CustomerData.IsAlibaba ? "Mandrill - Alibaba - Took Loan (not 1st loan)" : "Mandrill - Took Loan (not 1st loan)";
			}
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		private readonly decimal amount;
		private readonly string loanRefNum;
		private readonly bool isFirst;
	} // class CashTransferred
} // namespace
