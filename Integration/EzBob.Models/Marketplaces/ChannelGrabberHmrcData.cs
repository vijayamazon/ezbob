using System.Collections.Generic;
using EZBob.DatabaseLib.DatabaseWrapper.Order;

namespace EzBob.Models.Marketplaces {
	#region class ChannelGrabberHmrcData

	public class ChannelGrabberHmrcData : IChannelGrabberData {
		public IEnumerable<VatReturnEntry> VatReturn { get; set; }
		public IEnumerable<RtiTaxMonthEntry> RtiTaxMonths { get; set; }
	} // class ChannelGrabberHmrcData

	#endregion class ChannelGrabberHmrcData
} // namespace EzBob.Models.Marketplaces
