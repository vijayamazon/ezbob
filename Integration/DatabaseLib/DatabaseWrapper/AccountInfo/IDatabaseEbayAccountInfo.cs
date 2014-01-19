using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EZBob.DatabaseLib.Common;

namespace EZBob.DatabaseLib.DatabaseWrapper.AccountInfo
{
	public interface IDatabaseEbayAccountInfo
	{
		DateTime SubmittedDate { get; }
		string PaymentMethod { get; }
		bool PastDue { get; }
		double? CurrentBalance { get; }
		DateTime? CreditCardModifyDate { get; }
		string CreditCardInfo { get; }
		DateTime? CreditCardExpiration { get; }
		DateTime? BankModifyDate { get; }
		string AccountState { get; }
		AmountInfo AmountPastDueValue { get; }
		string BankAccountInfo { get; }
		string AccountId { get; }
		string Currency { get; }
		DatabaseEbayAdditionalAccount[] AdditionalAccount { get; }
	}
	public class DatabaseEbayAdditionalAccount
	{
		public string Currency { get; set; }
		public AmountInfo Balance { get; set; }
		public string AccountCode { get; set; }
	}
}
