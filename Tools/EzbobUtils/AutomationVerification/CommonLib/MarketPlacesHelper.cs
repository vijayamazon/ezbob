using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib
{
	public class MarketPlacesHelper
	{
		public static int GetMarketPlacesSeniority(List<MarketPlace> mps)
		{
			var date = mps.Where(x=> x.OriginationDate.HasValue).Min(x => x.OriginationDate.Value);
			TimeSpan ts = DateTime.Today - date;
			return (int)ts.TotalDays;
		}
	}
}
