using System.Xml;
using System.Xml.Serialization;

namespace EzBob.TeraPeakServiceLib.Requests.ResearchResult
{
	public class SearchQuery 
	{
		public SearchQuery()
		{
			Currency = "3"; // Pound Sterling
			SiteId = "1"; // All eBay Sites
		}

		public SearchQuery(string keywords)
			:this()
		{
			Keywords = keywords;
		}

		[XmlIgnore]
		public string Keywords { get; set; }

		[XmlElement( "Keywords" )]
		public XmlCDataSection Message
		{
			get { return string.IsNullOrWhiteSpace(Keywords)? null: new XmlDocument().CreateCDataSection( Keywords ); }
			set { Keywords = value.Value; }
		}

		/// <summary>
		/// Currency Codes
		/// </summary>
		/// <see cref="https://developer.terapeak.com/docs/researchapi/codes/Currency_Codes"/>
		public string Currency { get; set; }

		/// <summary>
		/// Site Codes
		/// </summary>
		/// <see cref="https://developer.terapeak.com/docs/researchapi/codes/Site_Codes"/>
		[XmlElement( "SiteID" )]
		public string SiteId { get; set; }
	}
}