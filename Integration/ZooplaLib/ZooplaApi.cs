﻿namespace ZooplaLib
{
	using System;
	using System.Globalization;
	using System.Xml;
	using Newtonsoft.Json.Linq;
	using RestSharp;

	public class ZooplaApi
	{
		private const string ApiKey = "wrjsacazcrxfzm4gj5q56b58";
		private readonly RestClient _client = new RestClient();
		/// <summary>
		///Description: Retrieve the average sale price for houses in a particular area.
		///Access URI: http://api.zoopla.co.uk/api/v1/average_area_sold_price
		/// </summary>
		/// <param name="postcode"></param>
		/// <returns></returns>
		public AverageSoldPrices GetAverageSoldPrices(string postcode, bool retry = false)
		{
			var req = new RestRequest("http://api.zoopla.co.uk/api/v1/average_area_sold_price");
			req.AddParameter("postcode", postcode);
			req.AddParameter("output_type", "outcode");
			req.AddParameter("area_type", "postcodes");
			req.AddParameter("api_key", ApiKey);
			IRestResponse res = _client.Get(req);

			var x = new XmlDocument();
			x.LoadXml(res.Content);
			var error = x.SelectSingleNode("/response/error_string");
			if (error != null)
			{
				throw new Exception(error.InnerText);
			}
			try
			{
				int averageSoldPrice1Year = 0;
				int.TryParse(x.SelectSingleNode("/response/average_sold_price_1year").InnerText, out averageSoldPrice1Year);
				int averageSoldPrice3Year = 0;
				int.TryParse(x.SelectSingleNode("/response/average_sold_price_3year").InnerText, out averageSoldPrice3Year);
				int averageSoldPrice5Year = 0;
				int.TryParse(x.SelectSingleNode("/response/average_sold_price_5year").InnerText, out averageSoldPrice5Year);
				int averageSoldPrice7Year = 0;
				int.TryParse(x.SelectSingleNode("/response/average_sold_price_7year").InnerText, out averageSoldPrice7Year);
				if (!retry && averageSoldPrice1Year == 0 && averageSoldPrice3Year == 0 && averageSoldPrice5Year == 0 &&
				    averageSoldPrice7Year == 0)
				{
					return GetAverageSoldPrices(postcode.Substring(0, 3), true);
				}
				
				int numerOfSales1Year = 0;
				int.TryParse(x.SelectSingleNode("/response/number_of_sales_1year").InnerText, out numerOfSales1Year);
				int numerOfSales3Year = 0;
				int.TryParse(x.SelectSingleNode("/response/number_of_sales_3year").InnerText, out numerOfSales3Year);
				int numerOfSales5Year = 0;
				int.TryParse(x.SelectSingleNode("/response/number_of_sales_5year").InnerText, out numerOfSales5Year);
				int numerOfSales7Year = 0;
				int.TryParse(x.SelectSingleNode("/response/number_of_sales_7year").InnerText, out numerOfSales7Year);
				double turnOver = 0;
				double.TryParse(x.SelectSingleNode("/response/turnover").InnerText, out turnOver);

				return new AverageSoldPrices
				{
					AverageSoldPrice1Year = averageSoldPrice1Year,
					AverageSoldPrice3Year = averageSoldPrice3Year,
					AverageSoldPrice5Year = averageSoldPrice5Year,
					AverageSoldPrice7Year = averageSoldPrice7Year,
					NumerOfSales1Year = numerOfSales1Year,
					NumerOfSales3Year = numerOfSales3Year,
					NumerOfSales5Year = numerOfSales5Year,
					NumerOfSales7Year = numerOfSales7Year,
					PricesUrl = x.SelectSingleNode("/response/prices_url").InnerText,
					TurnOver = turnOver,
					AreaName = x.SelectSingleNode("/response/area_name").InnerText
				};
			}
			catch (Exception)
			{
				throw new Exception("Couldn't parse the response");
			}
			
		}

		#region not in use

		/// <summary>
		/// Description: Retrieve the average sale price for a particular sub-area type within a particular area.
		///	Access URI: http://api.zoopla.co.uk/api/v1/average_sold_prices
		/// </summary>
		/// <param name="postcode"></param>
		/// <returns></returns>
		public int GetAverageSoldPriceOneYear2(string postcode)
		{
			var outcode = postcode.Split(' ')[0];
			int pages;
			int page = 1;
			const int pagesize = 100;
			do
			{
				var req2 = new RestRequest("http://api.zoopla.co.uk/api/v1/average_sold_prices");
				req2.AddParameter("postcode", postcode);
				req2.AddParameter("output_type", "county");
				req2.AddParameter("area_type", "outcodes");
				req2.AddParameter("page_size", pagesize);
				req2.AddParameter("page_number", page);
				req2.AddParameter("api_key", ApiKey);

				IRestResponse res2 = _client.Get(req2);
				var x = new XmlDocument();
				x.LoadXml(res2.Content);
				var error = x.SelectSingleNode("/response/error_string");
				if (error != null)
				{
					throw new Exception(error.InnerText);
				}
				var collection = x.SelectNodes("/response/areas");
				int results = int.Parse(x.SelectSingleNode("/response/result_count").InnerText);

				pages = results / pagesize + (results % pagesize > 0 ? 1 : 0);
				foreach (XmlNode node in collection)
				{
					if (node.SelectSingleNode("prices_url").InnerText.EndsWith("/" + outcode, ignoreCase: true, culture: CultureInfo.InvariantCulture))
					{
						return int.Parse(node.SelectSingleNode("average_sold_price_1year").InnerText);
					}
				}
				page++;
			} while (page <= pages);

			throw new Exception(string.Format("out code {0} not found", outcode));
		}

		#endregion

		/// <summary>
		/// Description: Generate a graph of values for an outcode over the previous 3 months and return the URL to the generated image. 
		/// Please note that the output type must always be "outcode" for this method and therefore an area sufficient to produce an outcode is required
		/// </summary>
		/// <param name="postcode"></param>
		/// <returns></returns>
		public ZooplaGraphs GetAreaValueGraphs(string postcode)
		{
			var req = new RestRequest("http://api.zoopla.co.uk/api/v1/area_value_graphs.js");
			req.AddParameter("api_key", ApiKey);
			req.AddParameter("postcode", postcode);
			req.AddParameter("output_type", "outcode");
			req.AddParameter("size", "large");

			var res = _client.Get(req);

			JToken token = JObject.Parse(res.Content);
			if (token.SelectToken("error_string") != null)
			{
				throw new Exception(token.SelectToken("error_string").Value<string>());
			}

			return new ZooplaGraphs
				{
					AverageValuesGraphUrl = token.SelectToken("average_values_graph_url").Value<string>(),
					ValueRangesGraphUrl = token.SelectToken("value_ranges_graph_url").Value<string>(),
					ValueTrendGraphUrl = token.SelectToken("value_trend_graph_url").Value<string>(),
					HomeValuesGraphUrl = token.SelectToken("home_values_graph_url").Value<string>(),
				};
		}

	}
}
