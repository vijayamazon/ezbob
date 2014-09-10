using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.CommonLib;
using EzBob.CommonLib.ReceivedDataListLogic;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Transactions
{
	public class PayPalTransactionsList : ReceivedDataListTimeMarketTimeDependentBase<PayPalTransactionItem>
	{

		public PayPalTransactionsList(DateTime submittedDate, IEnumerable<PayPalTransactionItem> collection = null) 
			: base(submittedDate, collection)
		{
		}

		public override ReceivedDataListTimeDependentBase<PayPalTransactionItem> Create(DateTime submittedDate, IEnumerable<PayPalTransactionItem> collection)
		{
			return new PayPalTransactionsList(submittedDate, collection);
		}

		public bool TryAddNewData(List<PayPalTransactionItem> newItems)
		{
			bool rez = false;

			if (newItems == null || !newItems.Any())
			{
				return false;
			}
			Parallel.ForEach(newItems, p =>
			{
				if (this.All(x => x.TransactionId != p.TransactionId))
				{
					this.Add(p);
					rez = true;
				}
			});

			return rez;
		}	
	}

	public class PayPalTransactionItem : TimeDependentRangedDataBase
	{
		public DateTime Created { get; set; }

		public string Timezone { get; set; }

		public string Type { get; set; }

		public string Status { get; set; }

		public AmountInfo FeeAmount { get; set; }

		public AmountInfo GrossAmount { get; set; }

		public AmountInfo NetAmount { get; set; }

		public string TransactionId { get; set; }

		public override DateTime RecordTime
		{
			get { return Created; }
		}
	}

}
