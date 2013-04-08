using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Common;
using EzBob.CommonLib;
using EzBob.CommonLib.ReceivedDataListLogic;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order
{
	public class PayPointOrdersList : ReceivedDataListTimeMarketTimeDependentBase<PayPointOrderItem>
	{
        public PayPointOrdersList(DateTime submittedDate, IEnumerable<PayPointOrderItem> collection = null) 
			: base(submittedDate, collection)
		{
		}

        public override ReceivedDataListTimeDependentBase<PayPointOrderItem> Create(DateTime submittedDate, IEnumerable<PayPointOrderItem> collection)
		{
            return new PayPointOrdersList(submittedDate, collection);
		}
	}

    public class PayPointOrderItem : TimeDependentRangedDataBase
	{
        public int Id {get; set;}
        public int OrderId {get; set;}
        public string acquirer {get; set;}
        public decimal amount {get; set;}
        public string auth_code {get; set;}
        public string authorised {get; set;}
        public string card_type {get; set;}
        public string cid {get; set;}
        public string classType {get; set;}
        public string company_no {get; set;}
        public string country {get; set;}
        public string currency {get; set;}
        public string cv2avs {get; set;}
        public DateTime date {get; set;}
        public string deferred {get; set;}
        public string emvValue {get; set;}
        public DateTime ExpiryDate { get; set; }
        public string fraud_code {get; set;}
        public string FraudScore {get; set;}
        public string ip {get; set;}
        public string lastfive {get; set;}
        public string merchant_no {get; set;}
        public string message {get; set;}
        public string MessageType {get; set;}
        public string mid {get; set;}
        public string name {get; set;}
        public DateTime start_date { get; set; }
        public string options {get; set;}
        public string status {get; set;}
        public string tid {get; set;}
        public string trans_id {get; set;}
        
		public override DateTime RecordTime
		{
			get { return date; }
		}
	}
}