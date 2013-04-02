using System;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_AmazonFeedback
	{
		public MP_AmazonFeedback()
		{
			FeedbackByPeriodItems = new HashedSet<MP_AmazonFeedbackItem>();
		}

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }

		public virtual ISet<MP_AmazonFeedbackItem> FeedbackByPeriodItems { get; set; }

		public virtual double UserRaining { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
	}
}