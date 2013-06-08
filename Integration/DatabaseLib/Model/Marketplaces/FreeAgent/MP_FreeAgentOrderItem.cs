namespace EZBob.DatabaseLib.Model.Marketplaces.FreeAgent
{
	using System;

	public class MP_FreeAgentOrderItem
	{
		public virtual int Id { get; set; }

		public virtual MP_FreeAgentOrder Order { get; set; }

		public virtual string url { get; set; }
		public virtual string contact { get; set; }
		public virtual DateTime dated_on { get; set; }
		public virtual DateTime due_on { get; set; }
		public virtual string reference { get; set; }
		public virtual string currency { get; set; }
		public virtual decimal exchange_rate { get; set; }
		public virtual decimal net_value { get; set; }
		public virtual decimal total_value { get; set; }
		public virtual decimal paid_value { get; set; }
		public virtual decimal due_value { get; set; }
		public virtual string status { get; set; }
		public virtual bool omit_header { get; set; }
		public virtual int payment_terms_in_days { get; set; }
		public virtual DateTime paid_on { get; set; }
	}
}