using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Common;
using EzBob.CommonLib;
using EzBob.CommonLib.ReceivedDataListLogic;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order
{
    public class YodleeOrderItem
    {
        public Dictionary<BankData, List<BankTransactionData>> Data { get; set; }
    }

    public class YodleeTransactionItem : TimeDependentRangedDataBase
    {
        private BankTransactionData _Data;
        public YodleeTransactionItem(BankTransactionData data)
        {
            this._Data = data;
        }

        public override DateTime RecordTime
        {
            get { return _Data.transactionDate.date.Value; }
        }
    }

    public class YodleeTransactionList : ReceivedDataListTimeMarketTimeDependentBase<YodleeTransactionItem>
    {
        public YodleeTransactionList(DateTime submittedDate, IEnumerable<YodleeTransactionItem> collection = null) : base(submittedDate, collection)
        {
        }

        public static YodleeTransactionList Create(YodleeOrderItem item) 
        {
            //todo: dict to  YodleeTransactionItem
        }

        public override ReceivedDataListTimeDependentBase<YodleeTransactionItem> Create(DateTime submittedDate, IEnumerable<YodleeTransactionItem> collection)
        {
            return new YodleeTransactionList(submittedDate, collection);
        }
    }

}