using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Common;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order
{
	public class EBayDatabaseExternalTransactionList : List<EBayDatabaseExternalTransactionItem>
	{
		public EBayDatabaseExternalTransactionList()
		{
		}

		public EBayDatabaseExternalTransactionList(IEnumerable<EBayDatabaseExternalTransactionItem>  data)
			:base(data)
		{
			
		}

		public bool HasData
		{
			get { return Count > 0; }
		}
	}

	public class EBayDatabaseExternalTransactionItem
	{
		public string TransactionID { get; set; }

		public DateTime? TransactionTime { get; set; }

		public AmountInfo FeeOrCreditAmount { get; set; }

		public AmountInfo PaymentOrRefundAmount { get; set; }
	}

}