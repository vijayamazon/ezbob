namespace EZBob.DatabaseLib.Model.Database
{
	public class BasicInterestRate
	{
		public virtual int Id { get; set; }
		public virtual int FromScore { get; set; }
		public virtual int ToScore { get; set; }
		public virtual decimal LoanInterestBase { get; set; }
	}
}

namespace EZBob.DatabaseLib.Model.DataMapping
{
	using Database;
	using FluentNHibernate.Mapping;

	public class BasicInterestRateMap : ClassMap<BasicInterestRate>
	{
		public BasicInterestRateMap()
		{
			Table("BasicInterestRate");
			Id(x => x.Id);
			Map(x => x.FromScore);
			Map(x => x.ToScore);
			Map(x => x.LoanInterestBase);
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using ApplicationMng.Repository;
	using Database;
	using NHibernate;

	public interface IBasicInterestRateRepository : IRepository<BasicInterestRate>
	{
		
	}

	public class BasicInterestRateRepository : NHibernateRepositoryBase<BasicInterestRate>, IBasicInterestRateRepository
	{
		public BasicInterestRateRepository(ISession session)
			: base(session)
		{
		}
	}
}
