using System;
using System.Collections.Generic;
using EzBob.CommonLib.ReceivedDataListLogic;

namespace EzBob.CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData
{
	public class TeraPeakDatabaseSellerData : ReceivedDataListTimeDependentBase<TeraPeakDatabaseSellerDataItem>
	{
		public TeraPeakDatabaseSellerData( TeraPeakDatabaseSellerData data )
			: base( data.SubmittedDate, data )
		{
		}

		public TeraPeakDatabaseSellerData(DateTime submitted, IEnumerable<TeraPeakDatabaseSellerDataItem> collection = null)
			:base(submitted, collection)
		{
			Submitted = submitted;
		}

		public DateTime Submitted { get; set; }

		public string Error { get; set; }

		public bool HasError
		{
			get { return !string.IsNullOrWhiteSpace( Error ); }
		}

		public override ReceivedDataListTimeDependentBase<TeraPeakDatabaseSellerDataItem> Create(DateTime submittedDate, IEnumerable<TeraPeakDatabaseSellerDataItem> collection)
		{
			return new TeraPeakDatabaseSellerData( submittedDate, collection );
		}
	}	
}
