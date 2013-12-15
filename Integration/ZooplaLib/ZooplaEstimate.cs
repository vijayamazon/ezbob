namespace ZooplaLib
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Net.Http;
	using HtmlAgilityPack;

	class ZooplaEstimate
	{
		private static HttpClient Session { get; set; }

		public string GetEstimate(string address)
		{
			var oData = new Dictionary<string, string>();
			oData["q"] = address;
			oData["search_source"] = "home-values";
			oData["view_type"] = "list";

			Session = new HttpClient
			{
				BaseAddress = new Uri("http://www.zoopla.co.uk")
			};
			HttpResponseMessage response = Session.GetAsync("search/" + ToQueryString(oData)).Result;

			HttpResponseMessage response2 = Session.GetAsync(response.RequestMessage.RequestUri.AbsoluteUri.ToString(CultureInfo.InvariantCulture)).Result;
			response2.EnsureSuccessStatusCode();
			string sResponse = response.Content.ReadAsStringAsync().Result;

			var doc = new HtmlDocument();
			doc.LoadHtml(sResponse);

			var estimate = GetEstimate(doc, oData["q"]);
			if (estimate != null)
			{
				return estimate;
			}

			string nextPage;
			do
			{
				nextPage = doc.DocumentNode.SelectSingleNode("//div[@class = 'paginate bg-muted']/a[last()]").InnerText;
				var href = doc.DocumentNode.SelectSingleNode("//div[@class = 'paginate bg-muted']/a[last()]").Attributes["href"].Value;
				if (nextPage == "Next")
				{
					var nextResponse = Session.GetAsync(href).Result;
					sResponse = nextResponse.Content.ReadAsStringAsync().Result;
					doc = new HtmlDocument();
					doc.LoadHtml(sResponse);
					estimate = GetEstimate(doc, oData["q"]);
					if (estimate != null)
					{
						return estimate;
					}
				}
			} while (nextPage == "Next");

			return "Address not found";
		}

		private string GetEstimate(HtmlDocument doc, string addr)
		{
			HtmlNodeCollection tr = doc.DocumentNode.SelectNodes("/html/body//table/tbody/tr[position() mod 2 = 1]");

			foreach (HtmlNode node in tr)
			{
				var address = node.SelectSingleNode("td[2]/h2");
				if (address.InnerText == addr)
				{
					var estimate = node.SelectSingleNode("td[3]/strong");
					if (estimate != null)
					{
						return estimate.InnerText;
					}
					return "No Estimate";
				}
			}

			return null;
		}

		private string ToQueryString(Dictionary<string, string> dict)
		{
			var str = new List<string>();
			foreach (var elem in dict)
			{
				str.Add(elem.Key + "=" + Uri.EscapeUriString(elem.Value));
			}

			return "?" + string.Join("&", str);
		}
	}
}
