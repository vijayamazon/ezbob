using System;

namespace EZBob.DatabaseLib.Model.Database {
	public class MP_ChannelGrabberOrder {
		public MP_ChannelGrabberOrder() {
			OrderItems = new Iesi.Collections.Generic.HashedSet<MP_ChannelGrabberOrderItem>();
		} // constructor

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }

		public virtual Iesi.Collections.Generic.ISet<MP_ChannelGrabberOrderItem> OrderItems { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
	} // class MP_ChannelGrabberOrder
} // namespace
