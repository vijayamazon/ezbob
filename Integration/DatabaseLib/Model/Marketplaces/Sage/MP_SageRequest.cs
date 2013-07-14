namespace EZBob.DatabaseLib.Model.Marketplaces.Sage
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using Database;
	using Iesi.Collections.Generic;
	using NHibernate;
	using NHibernate.Linq;

	public class MP_SageRequest
	{
		public MP_SageRequest()
		{
			Invoices = new HashedSet<MP_SageSalesInvoice>();
		}

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }

		public virtual Iesi.Collections.Generic.ISet<MP_SageSalesInvoice> Invoices { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
	}

	public interface IMP_SageRequestRepository : IRepository<MP_SageRequest>
    {
    }

    public class MP_SageRequestRepository : NHibernateRepositoryBase<MP_SageRequest>, IMP_SageRequestRepository
    {
		public MP_SageRequestRepository(ISession session)
			: base(session)
        {
        }

		public List<MP_SageRequest> GetRequestsByMakretplaceId(int marketplaceId)
		{
			return _session
				.Query<MP_SageRequest>()
				.Where(oi => oi.CustomerMarketPlace.Id == marketplaceId)
				.ToList();
		}
    }
}
