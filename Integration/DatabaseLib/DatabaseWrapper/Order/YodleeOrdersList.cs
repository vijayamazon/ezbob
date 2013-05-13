using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Common;
using EzBob.CommonLib;
using EzBob.CommonLib.ReceivedDataListLogic;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order
{
    public class YodleeOrdersList : ReceivedDataListTimeMarketTimeDependentBase<YodleeOrderItem>
    {
        public YodleeOrdersList(DateTime submittedDate, IEnumerable<YodleeOrderItem> collection = null)
            : base(submittedDate, collection)
        {
        }

        public override ReceivedDataListTimeDependentBase<YodleeOrderItem> Create(DateTime submittedDate, IEnumerable<YodleeOrderItem> collection)
        {
            return new YodleeOrdersList(submittedDate, collection);
        }
    }

    public class YodleeOrderItem : TimeDependentRangedDataBase
    {
        public Dictionary<BankData, List<BankTransactionData>> Data { get; set; }

        public override DateTime RecordTime
        {
            get { throw new NotImplementedException(); }
        }
    }


}