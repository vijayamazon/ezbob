using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayFeedbackItemMap : ClassMap<MP_EbayFeedbackItem>
	{
		public MP_EbayFeedbackItemMap()
		{
			Table( "MP_EbayFeedbackItem" );
			Id( x => x.Id );
			Map( x => x.Negative );
			Map( x => x.Neutral );
			Map( x => x.Positive );
			References( x => x.EbayFeedback, "EbayFeedbackId" );
			References( x => x.TimePeriod, "TimePeriodId" ).Fetch.Join();
		}
	}
}