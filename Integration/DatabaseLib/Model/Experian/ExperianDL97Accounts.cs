namespace EZBob.DatabaseLib.Model.Experian
{
	using System;
	using Database;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;
	using ApplicationMng.Repository;
	using NHibernate;

	public class ExperianDL97Accounts
	{
		public virtual int Id { get; set; }
		public virtual MP_ExperianDataCache DataCache { get; set; }
		public virtual string State { get; set; }
		public virtual string Type { get; set; }
		public virtual string Status12Months { get; set; }
		public virtual DateTime? LastUpdated { get; set; }
		public virtual string CompanyType { get; set; }
		public virtual int CurrentBalance { get; set; }
		public virtual int MonthsData { get; set; }
		public virtual int Status1To2 { get; set; }
		public virtual int Status3To9 { get; set; }
	}

	public sealed class ExperianDL97AccountsMap : ClassMap<ExperianDL97Accounts>
	{
		public ExperianDL97AccountsMap()
		{
			Table("ExperianDL97Accounts");
			Id(x => x.Id);
			References(x => x.DataCache, "DataCacheId");
			Map(x => x.State).Length(1);
			Map(x => x.Type).Length(2);
			Map(x => x.Status12Months).Length(12);
			Map(x => x.LastUpdated).CustomType<UtcDateTimeType>();
			Map(x => x.CompanyType).Length(2);
			Map(x => x.CurrentBalance);
			Map(x => x.MonthsData);
			Map(x => x.Status1To2);
			Map(x => x.Status3To9);
		}
	}

	public interface IExperianDL97AccountsRepository : IRepository<ExperianDL97Accounts>
	{
	}

	public class ExperianDL97AccountsRepository : NHibernateRepositoryBase<ExperianDL97Accounts>, IExperianDL97AccountsRepository
	{

		public ExperianDL97AccountsRepository(ISession session)
			: base(session)
		{
		}
	}
}