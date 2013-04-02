using System;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayFeedback
	{
		public MP_EbayFeedback()
		{
			FeedbackByPeriodItems = new HashedSet<MP_EbayFeedbackItem>();
			RaitingByPeriodItems = new HashedSet<MP_EbayRaitingItem>();
		}

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }

		public virtual int RepeatBuyerCount { get; set; }
		public virtual double RepeatBuyerPercent { get; set; }
		public virtual double TransactionPercent { get; set; }
		public virtual int UniqueBuyerCount { get; set; }
		public virtual int UniqueNegativeCount { get; set; }
		public virtual int UniquePositiveCount { get; set; }
		public virtual int UniqueNeutralCount { get; set; }

		public virtual ISet<MP_EbayFeedbackItem> FeedbackByPeriodItems { get; set; }
		public virtual ISet<MP_EbayRaitingItem> RaitingByPeriodItems { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
	}
}