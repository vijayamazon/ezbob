namespace EZBob.DatabaseLib.Model.Database
{
	using System;
	using UserManagement;

	public class SuggestedAmount
	{
		public virtual int Id { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual User Underwriter { get; set; }
		public virtual CashRequest CashRequest { get; set; }
		public virtual DateTime InsertDate { get; set; }
		public virtual string Medal { get; set; }
		public virtual string Method { get; set; }
		public virtual decimal Amount { get; set; }
		public virtual decimal Percents { get; set; }
	}
}

namespace EZBob.DatabaseLib.Model.Database.Mapping
{
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class SuggestedAmountMap : ClassMap<SuggestedAmount>
	{
		public SuggestedAmountMap()
		{
			Table("SuggestedAmount");
			Id(x => x.Id);
			
			Map(x => x.Medal);
			Map(x => x.Method);
			Map(x => x.Amount);
			Map(x => x.Percents);
			Map(x => x.InsertDate).CustomType<UtcDateTimeType>();

			References(x => x.Customer, "CustomerId");
			References(x => x.Underwriter, "UnderwriterId");
			References(x => x.CashRequest, "CashRequestId");
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using ApplicationMng.Repository;
	using NHibernate;

	public interface ISuggestedAmountRepository : IRepository<SuggestedAmount>
	{
	}

	public class SuggestedAmountRepository : NHibernateRepositoryBase<SuggestedAmount>, ISuggestedAmountRepository
	{
		public SuggestedAmountRepository(ISession session)
			: base(session)
		{
		}

	}
}
