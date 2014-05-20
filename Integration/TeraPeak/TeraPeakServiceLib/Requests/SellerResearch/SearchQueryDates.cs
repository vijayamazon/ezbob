namespace EzBob.TeraPeakServiceLib.Requests.SellerResearch
{
	using System.Xml.Serialization;

	[XmlInclude( typeof( SearchQueryDatesRange ) )]
	public abstract class SearchQueryDates
	{
	}
}