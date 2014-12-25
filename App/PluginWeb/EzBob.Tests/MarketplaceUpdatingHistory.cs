namespace EzBob.Tests {

	using System.Linq;
	using System.Text;
	using EZBob.DatabaseLib.Model.Database;
	using NHibernate;
	using NUnit.Framework;
	using EZBob.DatabaseLib.Model.Database.UserManagement;

	public class MarketplaceUpdatingHistory : InMemoryDbTestFixtureBase {
		[Test]
		public void can_query_updating_history() {
			var mp = new MP_CustomerMarketPlace();
			mp.SecurityData = Encoding.UTF8.GetBytes("no data");
			_session.Save(mp);
			var tx = _session.BeginTransaction();
			tx.Commit();

			_session.Clear();

			var mp2 = _session.Get<MP_CustomerMarketPlace>(mp.Id);

			var r1 = mp2.UpdatingHistory.Where(h => h.UpdatingStart != null && h.UpdatingEnd != null)
				.Select(h => h.EbayFeedback)
				.ToList();
			var r3 = mp2.UpdatingHistory.Where(h => h.UpdatingStart != null && h.UpdatingEnd != null)
				.Select(h => h.AmazonFeedback)
				.ToList();
		}

		[TestFixtureSetUp]
		public void FixtureSetUp() {
			InitialiseNHibernate(typeof(MP_CustomerMarketPlace).Assembly, typeof(User).Assembly);
		}

		[SetUp]
		public void SetUp() {
			_session = CreateSession();
		}

		private ISession _session;
	}

}
