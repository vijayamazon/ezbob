using System;
using EzBob.CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_TeraPeakOrderItem
	{
		public virtual int Id { get; set; }

		public virtual MP_TeraPeakOrder Order { get; set; }

		public virtual DateTime StartDate { get; set; }
		public virtual DateTime EndDate { get; set; }
		public virtual double? Revenue { get; set; }
		public virtual int? Listings { get; set; }
		public virtual int? Transactions { get; set; }
		public virtual int? Successful { get; set; }
		public virtual int? Bids { get; set; }
		public virtual int? ItemsOffered { get; set; }
		public virtual int? ItemsSold { get; set; }
		public virtual int? AverageSellersPerDay { get; set; }
		public virtual double? SuccessRate { get; set; }

		public virtual RangeMarkerType RangeMarker { get; set; }
	}
}