namespace EZBob.DatabaseLib.Model.Marketplaces.FreeAgent
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using Database;
	using Iesi.Collections.Generic;
	using NHibernate;
	using NHibernate.Linq;

	public class MP_FreeAgentRequest
	{
		public MP_FreeAgentRequest()
		{
			Invoices = new HashedSet<MP_FreeAgentInvoice>();
			Expenses = new HashedSet<MP_FreeAgentExpense>();
		}

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }

		public virtual Iesi.Collections.Generic.ISet<MP_FreeAgentInvoice> Invoices { get; set; }
		public virtual Iesi.Collections.Generic.ISet<MP_FreeAgentExpense> Expenses { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
	}

	public interface IMP_FreeAgentRequestRepository : IRepository<MP_FreeAgentRequest>
    {
    }

    public class MP_FreeAgentRequestRepository : NHibernateRepositoryBase<MP_FreeAgentRequest>, IMP_FreeAgentRequestRepository
    {
		public MP_FreeAgentRequestRepository(ISession session)
			: base(session)
        {
        }

		public List<MP_FreeAgentRequest> GetRequestsByMakretplaceId(int marketplaceId)
		{
			return Session
				.Query<MP_FreeAgentRequest>()
				.Where(oi => oi.CustomerMarketPlace.Id == marketplaceId)
				.ToList();
		}
    }
}
