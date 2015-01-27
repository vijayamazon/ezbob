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
			SalesInvoices = new HashedSet<MP_SageSalesInvoice>();
			Incomes = new HashedSet<MP_SageIncome>();
			PurchaseInvoices = new HashedSet<MP_SagePurchaseInvoice>();
			Expenditures = new HashedSet<MP_SageExpenditure>();
		}

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }

		public virtual Iesi.Collections.Generic.ISet<MP_SageSalesInvoice> SalesInvoices { get; set; }
		public virtual Iesi.Collections.Generic.ISet<MP_SageIncome> Incomes { get; set; }
		public virtual Iesi.Collections.Generic.ISet<MP_SagePurchaseInvoice> PurchaseInvoices { get; set; }
		public virtual Iesi.Collections.Generic.ISet<MP_SageExpenditure> Expenditures { get; set; }

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
			return Session
				.Query<MP_SageRequest>()
				.Where(oi => oi.CustomerMarketPlace.Id == marketplaceId)
				.ToList();
		}
    }
}
