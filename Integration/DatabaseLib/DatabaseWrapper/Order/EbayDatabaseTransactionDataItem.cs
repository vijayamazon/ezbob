using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.Model.Database;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order
{
	public class EbayDatabaseTransactionDataList : List<EbayDatabaseTransactionDataItem>
	{
		public EbayDatabaseTransactionDataList()
		{
		}

		public EbayDatabaseTransactionDataList(IEnumerable<EbayDatabaseTransactionDataItem>  data)
			:base(data)
		{
			
		}

		public bool HasData
		{
			get { return Count > 0; }
		}
	}
	
	public class EbayDatabaseTransactionDataItem
	{
		public DateTime CreatedDate { get; set; }
		public int? QuantityPurchased { get; set; }
		public string PaymentHoldStatus { get; set; }
		public string PaymentMethodUsed { get; set; }
		public AmountInfo TransactionPrice { get; set; }
		public string ItemID { get; set; }
		public string ItemPrivateNotes { get; set; }
		public string ItemSellerInventoryID { get; set; }
		public string ItemSKU { get; set; }
		public string eBayTransactionId { get; set; }
		public MP_EBayOrderItemDetail OrderItemDetail { get; set; }
	}

	public class EbayDatabaseOrderItemInfo
	{
		public string ItemID { get; set; }
		public MP_EbayAmazonCategory PrimaryCategory { get; set; }
		public MP_EbayAmazonCategory SecondaryCategory { get; set; }
		public MP_EbayAmazonCategory FreeAddedCategory { get; set; }

		public string Title { get; set; }
	}

}