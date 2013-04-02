using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;
using NHibernate.Linq;

namespace EzBob.AmazonDbLib
{
    public class FeedbackItemsRepository
    {
        private readonly ISession _session;

        public FeedbackItemsRepository(ISession session)
        {
            _session = session;
        }

        public IQueryable<MP_AmazonFeedbackItem> GetItemsForAnalysisValuesByCustomerMarketPlace()
        {
            return _session.Query<MP_AmazonFeedbackItem>();
        }
    }
}