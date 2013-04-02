using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_AmazonFeedbackItemMap : ClassMap<MP_AmazonFeedbackItem>
	{
		public MP_AmazonFeedbackItemMap()
		{
			Table( "MP_AmazonFeedbackItem" );
			Id( x => x.Id );
			Map( x => x.Count );
			Map( x => x.Negative );
			Map( x => x.Neutral );
			Map( x => x.Positive );
			References( x => x.AmazonFeedback, "AmazonFeedbackId" );
            References(x => x.TimePeriod, "TimePeriodId").Fetch.Join();
		}
	}
}