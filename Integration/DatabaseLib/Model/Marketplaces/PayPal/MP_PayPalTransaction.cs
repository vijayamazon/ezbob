using System;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_PayPalTransaction
	{
		public MP_PayPalTransaction()
		{
			TransactionItems = new HashedSet<MP_PayPalTransactionItem2>();
		}

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }

		public virtual ISet<MP_PayPalTransactionItem2> TransactionItems { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
	}
}