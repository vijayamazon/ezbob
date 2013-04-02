using System;
using EZBob.DatabaseLib.Common;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayUserAccountData
	{
		public MP_EbayUserAccountData()
		{
			EbayUserAdditionalAccountData = new HashedSet<MP_EbayUserAdditionalAccountData>();
		}

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }
	
		public virtual string PaymentMethod { get; set; }
		public virtual bool PastDue { get; set; }
		public virtual double? CurrentBalance { get; set; }
		public virtual DateTime? CreditCardModifyDate { get; set; }
		public virtual string CreditCardInfo { get; set; }
		public virtual DateTime? CreditCardExpiration { get; set; }
		public virtual DateTime? BankModifyDate { get; set; }
		public virtual string AccountState { get; set; }
		public virtual AmountInfo AmountPastDueValue { get; set; }
		public virtual string BankAccountInfo { get; set; }
		public virtual string AccountId { get; set; }
		public virtual string Currency { get; set; }

		public virtual ISet<MP_EbayUserAdditionalAccountData> EbayUserAdditionalAccountData { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
	}
}