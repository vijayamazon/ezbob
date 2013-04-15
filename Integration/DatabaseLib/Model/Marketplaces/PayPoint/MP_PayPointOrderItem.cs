using System;
using EZBob.DatabaseLib.Common;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_PayPointOrderItem
	{
		public virtual int Id { get; set; }
 
		public virtual MP_PayPointOrder Order { get; set; }


        public virtual string acquirer { get; set; }
        public virtual decimal amount { get; set; }
        public virtual string auth_code { get; set; }
        public virtual string authorised { get; set; }
        public virtual string card_type { get; set; }
        public virtual string cid { get; set; }
        public virtual string classType { get; set; }
        public virtual string company_no { get; set; }
        public virtual string country { get; set; }
        public virtual string currency { get; set; }
        public virtual string cv2avs { get; set; }
        public virtual DateTime? date { get; set; }
        public virtual string deferred { get; set; }
        public virtual string emvValue { get; set; }
        public virtual DateTime? ExpiryDate { get; set; }
        public virtual string fraud_code { get; set; }
        public virtual string FraudScore { get; set; }
        public virtual string ip { get; set; }
        public virtual string lastfive { get; set; }
        public virtual string merchant_no { get; set; }
        public virtual string message { get; set; }
        public virtual string MessageType { get; set; }
        public virtual string mid { get; set; }
        public virtual string name { get; set; }
        public virtual DateTime? start_date { get; set; }
        public virtual string options { get; set; }
        public virtual string status { get; set; }
        public virtual string tid { get; set; }
        public virtual string trans_id { get; set; }
		
	}
}