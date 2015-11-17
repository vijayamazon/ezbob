namespace ZooplaLib {
	using System;
	using System.Collections.Generic;
	using System.Net.Http;
	using HtmlAgilityPack;

	class ZooplaEstimate {
		private static HttpClient Session { get; set; }
		private const int MaxPages = 10;

		public string GetEstimate(string address) {
			var oData = new Dictionary<string, string>();
			oData["q"] = address;
			oData["section"] = "house-prices";
			oData["geo_autocomplete_identifier"] = "";

			Session = new HttpClient() {
				BaseAddress = new Uri("http://www.zoopla.co.uk"),
			};

			string sResponse = null;
			using (HttpResponseMessage response = Session.GetAsync("search/" + ToQueryString(oData)).Result) {
				response.EnsureSuccessStatusCode();
				sResponse = response.Content.ReadAsStringAsync().Result;
			}
			var doc = new HtmlDocument();
			doc.LoadHtml(sResponse);

			var estimate = GetEstimate(doc, oData["q"]);
			if (estimate != null) {
				return estimate;
			}

			string nextPage = "one page";
			int pages = 0;
			do {
				var nextPageNode = doc.DocumentNode.SelectSingleNode("//div[@class = 'paginate bg-muted']/a[last()]");
				string href = null;
				if (nextPageNode != null) {
					nextPage = nextPageNode.InnerText;
					href = nextPageNode.Attributes["href"].Value;
				}
				if (nextPage == "Next") {
					var nextResponse = Session.GetAsync(href).Result;
					sResponse = nextResponse.Content.ReadAsStringAsync().Result;
					doc = new HtmlDocument();
					doc.LoadHtml(sResponse);
					estimate = GetEstimate(doc, oData["q"]);
					if (estimate != null) {
						return estimate;
					}
				}
				pages++;
			} while (nextPage == "Next" && pages < MaxPages);

			return "Address not found";
		}

		private string GetEstimate(HtmlDocument doc, string addr) {
			HtmlNodeCollection tr = doc.DocumentNode.SelectNodes("/html/body//table/tbody/tr[contains(@class,'yourresult')]");
			if (tr == null) {
				return "Address not found";
			}
			
			var estimate = tr[0].SelectSingleNode("td[@class='browse-cell-estimate']/span/span[@class='browse-estimate-value']");
			if (estimate != null) {
				return estimate.InnerText;
			}
			return "No Estimate";
		}

		private string ToQueryString(Dictionary<string, string> dict) {
			var str = new List<string>();
			foreach (var elem in dict) {
				str.Add(elem.Key + "=" + Uri.EscapeUriString(elem.Value));
			}

			return "?" + string.Join("&", str);
		}
	}
}
