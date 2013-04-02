using System.Collections;
using System.Collections.Generic;

namespace EzBob.TeraPeakServiceLib.Requests.SellerResearch
{
	public class ResultSellerInfo
	{
		private readonly List<string> _Items = new List<string>();

		public ResultSellerInfo()
		{
		}

		public ResultSellerInfo(ICollection checkedItems)
		{
			foreach (var item in checkedItems)
			{
				_Items.Add( item.ToString() );
			}
		}

		public bool ReturnHourlyData 
		{
			get { return _Items.Contains( "ReturnHourlyData" ); }
			set { _Items.Add( "ReturnHourlyData" ); }
		}
		public bool ReturnPricingInformation
		{
			get { return _Items.Contains( "ReturnPricingInformation" ); }
			set { _Items.Add( "ReturnPricingInformation" ); }
		}
		public bool ReturnFeatureInformation
		{
			get { return _Items.Contains( "ReturnFeatureInformation" ); }
			set { _Items.Add( "ReturnFeatureInformation" ); }
		}
		public bool ReturnDurationData
		{
			get { return _Items.Contains( "ReturnDurationData" ); }
			set { _Items.Add( "ReturnDurationData" ); }
		}
		public bool ReturnListingTypes
		{
			get { return _Items.Contains( "ReturnListingTypes" ); }
			set { _Items.Add( "ReturnListingTypes" ); }
		}
		public bool ReturnPriceBuckets
		{
			get { return _Items.Contains( "ReturnPriceBuckets" ); }
			set { _Items.Add( "ReturnPriceBuckets" ); }
		}
		public bool ReturnDailyData
		{
			get { return _Items.Contains( "ReturnDailyData" ); }
			set { _Items.Add( "ReturnDailyData" ); }
		}
		public bool ReturnCategoryInformation
		{
			get { return _Items.Contains( "ReturnCategoryInformation" ); }
			set { _Items.Add( "ReturnCategoryInformation" ); }
		}
		public bool ReturnItemList
		{
			get { return _Items.Contains( "ReturnItemList" ); }
			set { _Items.Add( "ReturnItemList" ); }
		}
		public bool ReturnKeywordData
		{
			get { return _Items.Contains( "ReturnKeywordData" ); }
			set { _Items.Add( "ReturnKeywordData" ); }
		}
		public bool ReturnShippingData
		{
			get { return _Items.Contains( "ReturnShippingData" ); }
			set { _Items.Add( "ReturnShippingData" ); }
		}
		public bool ReturnDemographicsData
		{
			get { return _Items.Contains( "ReturnDemographicsData" ); }
			set { _Items.Add( "ReturnDemographicsData" ); }
		}
		public bool ReturnSiteIDBreakdown
		{
			get { return _Items.Contains( "ReturnSiteIDBreakdown" ); }
			set { _Items.Add( "ReturnSiteIDBreakdown" ); }
		}

		public bool ReturnAttributeData
		{
			get { return _Items.Contains( "ReturnAttributeData" ); }
			set { _Items.Add( "ReturnAttributeData" ); }
		}
		public bool ReturnAllData
		{
			get { return _Items.Contains( "ReturnAllData" ); }
			set { _Items.Add( "ReturnAllData" ); }
		}

		public bool IsEmpty
		{
			get { return _Items.Count == 0; }
		}
	}
}