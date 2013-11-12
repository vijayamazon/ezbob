namespace EZBob.DatabaseLib.Model.Database
{
	using System;

	public class WhatsNewCustomerMap
	{
		public WhatsNewCustomerMap(){}

		public virtual int Id { get; set; }
		public virtual WhatsNew WhatsNew { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual DateTime Date { get; set; }
		public virtual bool Understood { get; set; }
	}
}

namespace EZBob.DatabaseLib.Model.Database.Mapping
{
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class WhatsNewCustomerMapMap : ClassMap<WhatsNewCustomerMap>
	{
		public WhatsNewCustomerMapMap()
		{
			Table("WhatsNewCustomerMap");
			Id(x => x.Id);
			References(x => x.WhatsNew, "WhatsNewId").Cascade.All();
			References(x => x.Customer, "CustomerId").Cascade.All();
			Map(x => x.Date).CustomType<UtcDateTimeType>();
			Map(x => x.Understood);
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using ApplicationMng.Repository;
	using NHibernate;

	public interface IWhatsNewCustomerMapRepository : IRepository<WhatsNewCustomerMap>
	{
	}

	public class WhatsNewCustomerMapRepository : NHibernateRepositoryBase<WhatsNewCustomerMap>, IWhatsNewCustomerMapRepository
	{
		public WhatsNewCustomerMapRepository(ISession session)
			: base(session)
		{
		}

	}
}
