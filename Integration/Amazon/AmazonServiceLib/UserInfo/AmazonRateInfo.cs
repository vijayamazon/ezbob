using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace EzBob.AmazonServiceLib.UserInfo
{
	public class AmazonRateInfo
	{
		public static bool IsUserCorrect( AmazonUserInfo userInfo )
		{
			var rater = new AmazonRateInfo();
			var sellerId = userInfo.MerchantId;
			var task = rater.GetRating(rater.GetSellerPage(sellerId));

			try
			{
				var raiting = task.Result;

				return true;
			}
			catch ( AggregateException  )
			{
				return false;
			}
		}
		public static AmazonUserRatingInfo GetUserRatingInfo( AmazonUserInfo userInfo )
		{
			var rater = new AmazonRateInfo();
			string seller = userInfo.MerchantId;
		    var sellerPage = rater.GetSellerPage(seller);
		    var rate = rater.GetRating(sellerPage);
		    var name = rater.GetName(sellerPage);
			var feedbackHistory = rater.FeedbackHistory(sellerPage);

			var info = new AmazonUserRatingInfo
				{
					Rating = rate.Result,
					FeedbackHistory = feedbackHistory.Result,
					Name = name.Result,
					SubmittedDate = DateTime.UtcNow,
				};

			info.IncrementRequests( "GetUserRating", "from WEB" );

			return info;
		}

        private Task<HtmlNode> GetSellerPage(string seller)
        {
            //var url = string.Format( "http://www.amazon.co.uk/shops/{0}/ref=olp_merch_name_1", seller );
            var url = string.Format(@"http://www.amazon.co.uk/gp/aag/main?ie=UTF8&seller={0}", seller);
            var request = WebRequest.Create(url);
            return Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, null)
                .ContinueWith(x =>
                {
                    var response = x.Result;
                    var html = new HtmlDocument();
                    html.Load(response.GetResponseStream());
                    var doc = html.DocumentNode;
                    return doc;
                });
        }

		private Task<double> GetRating(Task<HtmlNode> sellerPage)
		{
			return sellerPage.ContinueWith(tdoc =>
				{
					var stars = 0d;
					var doc = tdoc.Result;
					var extract = doc.QuerySelectorAll("div.feedbackMeanRating b").ToArray();
					if (extract.Length == 0)
					{
						// We don't know how to parse rating from doc
						return stars;

						//var extract2 = doc.QuerySelectorAll( "div.justLaunchedBlock" ).ToArray();
						//if ( extract2.Length == 0 )
						//{
						//	throw new NotImplementedException();
						//}
					}

					var textRating = extract[0].InnerText;
					stars = Convert.ToDouble(textRating, CultureInfo.InvariantCulture);
					return stars;
				}
				);
		}

		private Task<string> GetName(Task<HtmlNode> sellerPage)
		{
            return sellerPage.ContinueWith( tdoc =>
                        {
                            //var stars = 0d;
                            var doc = tdoc.Result;
				            var header = doc.QuerySelectorAll( "#aag_header > h1" ).FirstOrDefault();
					        if ( header == null )
					        {
					            return "NoName";
					        }
                            return header.InnerText;
                        }
                );
		}

		private Task<FeedbackHistoryInfo> FeedbackHistory(Task<HtmlNode> sellerPage)
		{
            return sellerPage.ContinueWith(tdoc =>
				               	{
				               		var doc = tdoc.Result;
				               		var extract = doc.QuerySelectorAll( "table.feedbackTable" ).ToArray();
				               		if ( extract.Length > 0 )
				               		{
				               			return ParceText( extract[0].InnerText );
				               		}
				               		return (FeedbackHistoryInfo)null;
				               	} );
		}

		private FeedbackHistoryInfo ParceText( string innerText )
		{
			var data = new FeedbackHistoryInfo();

			var rawStrings = innerText.Split( new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries ).Where( s => s.Trim().Length > 0 && !s.Contains( "?" ) ).Select( s => s.Trim() ).ToList();
			var count = rawStrings.Count;

			const int tableSize = 5;

			Debug.Assert( count == (int)Math.Pow( tableSize, 2 ) );

			for ( int i = 1; i < tableSize; i++ )
			{
				for ( int j = 1; j < tableSize; j++ )
				{
					var rawString = rawStrings[( i * tableSize ) + j];

					if ( rawString.Contains( "%" ) )
					{
						rawString = rawString.Replace( "%", "" );
					}

					int value = 0;
					int.TryParse( rawString, out value );

					data.Add( (FeedbackType)i, (FeedbackPeriod)j, value );
				}
			}
			return data;
		}

	    public static AmazonUserRatingInfo GetUserRatingInfo(string merchantId)
	    {
	        return GetUserRatingInfo(new AmazonUserInfo {MerchantId = merchantId});
	    }
	}
}
