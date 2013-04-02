using System;
using EZBob.DatabaseLib.Common;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayOrderItem
	{
		public MP_EbayOrderItem()
		{
			Transactions = new HashedSet<MP_EbayTransaction>();
			ExternalTransactions = new HashedSet<MP_EbayExternalTransaction>();
		}

		public virtual int Id { get; set; }

		public virtual MP_EbayOrder Order { get; set; }

		public virtual AmountInfo AdjustmentAmount { get; set; }
		public virtual AmountInfo AmountPaid { get; set; }
		public virtual AmountInfo SubTotal { get; set; }
		public virtual AmountInfo Total { get; set; }
		public virtual string PaymentStatus { get; set; }
		public virtual string PaymentMethod { get; set; }
		public virtual string CheckoutStatus { get; set; }
		public virtual string OrderStatus { get; set; }
		public virtual string PaymentHoldStatus { get; set; }
		public virtual string PaymentMethodsList { get; set; }

		public virtual MP_EbayUserAddressData ShippingAddress { get; set; }
		
		public virtual DateTime? CreatedTime { get; set; }
		public virtual DateTime? PaymentTime { get; set; }
		public virtual DateTime? ShippedTime { get; set; }

		public virtual string BuyerName { get; set; }

		public virtual ISet<MP_EbayTransaction> Transactions { get; set; }
		public virtual ISet<MP_EbayExternalTransaction> ExternalTransactions { get; set; }
	}
}