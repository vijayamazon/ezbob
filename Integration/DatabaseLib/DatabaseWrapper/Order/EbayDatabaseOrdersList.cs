using System;
using System.Collections;
using System.Collections.Generic;
using EZBob.DatabaseLib.Common;
using EzBob.CommonLib;
using EzBob.CommonLib.ReceivedDataListLogic;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order
{
	public class EbayDatabaseOrdersList : ReceivedDataListTimeMarketTimeDependentBase<EbayDatabaseOrderItem>
	{
		public EbayDatabaseOrdersList(DateTime submittedDate, IEnumerable<EbayDatabaseOrderItem> collection = null) 
			: base(submittedDate, collection)
		{
		}

		public override ReceivedDataListTimeDependentBase<EbayDatabaseOrderItem> Create(DateTime submittedDate, IEnumerable<EbayDatabaseOrderItem> collection)
		{
			return new EbayDatabaseOrdersList(submittedDate, collection);
		}
	}

	public enum EBayOrderStatusCodeType
	{
		Active,
		Inactive,
		Completed,
		Cancelled,
		Shipped,
		Default,
		Authenticated,
		InProcess,
		Invalid,
		CustomCode,
		All,
	}

	public class EbayDatabaseOrderItem : TimeDependentRangedDataBase
	{
		public virtual AmountInfo AdjustmentAmount { get; set; }
		public virtual AmountInfo AmountPaid { get; set; }
		public virtual AmountInfo SubTotal { get; set; }
		public virtual AmountInfo Total { get; set; }
		public virtual string PaymentStatus { get; set; }
		public virtual string PaymentMethod { get; set; }
		public virtual string CheckoutStatus { get; set; }
		public virtual EBayOrderStatusCodeType OrderStatus { get; set; }
		public virtual string PaymentHoldStatus { get; set; }
		public virtual string PaymentMethods { get; set; }

		public virtual DatabaseShipingAddress ShippingAddressData { get; set; }
		public virtual EbayDatabaseTransactionDataList TransactionData { get; set; }
		
		public virtual EBayDatabaseExternalTransactionList ExternalTransactionData { get; set; }

		public DateTime? CreatedTime { get; set; }

		public DateTime? ShippedTime { get; set; }

		public DateTime? PaymentTime { get; set; }

		public string BuyerName { get; set; }

		public override DateTime RecordTime
		{
			get { return CreatedTime.Value; }
		}
	}
}
