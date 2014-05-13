namespace EZBob.DatabaseLib.Model.Database
{
	using System;

	public class CustomerAnalytics
	{
		public virtual int Id { get; set; }
		public virtual DateTime AnalyticsDate { get; set; }
		public virtual int PersonalScore { get; set; }
		public virtual int PersonalMinScore { get; set; }
		public virtual int PersonalMaxScore { get; set; }
		public virtual int IndebtednessIndex { get; set; }
		public virtual int ThinFile { get; set; }
		public virtual int NumOfAccounts { get; set; }
		public virtual int NumOfDefaults { get; set; }
		public virtual int NumOfLastDefaults { get; set; }
		public virtual int CompanyScore { get; set; }
		public virtual decimal SuggestedAmount { get; set; }
		public virtual decimal AnnualTurnover { get; set; }
		public virtual DateTime IncorporationDate { get; set; }
	}
}

namespace EZBob.DatabaseLib.Model.Database.Mapping
{
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class CustomerAnalyticsMap : ClassMap<CustomerAnalytics>
	{
		public CustomerAnalyticsMap()
		{
			Not.LazyLoad();
			Table("CustomerAnalytics");
			ReadOnly();
			Id(x => x.Id, "CustomerId");
			Map(x => x.AnalyticsDate).CustomType<UtcDateTimeType>();
			Map(x => x.PersonalScore);
			Map(x => x.PersonalMinScore);
			Map(x => x.PersonalMaxScore);
			Map(x => x.IndebtednessIndex);
			Map(x => x.ThinFile);
			Map(x => x.NumOfAccounts);
			Map(x => x.NumOfDefaults);
			Map(x => x.NumOfLastDefaults);
			Map(x => x.CompanyScore);
			Map(x => x.SuggestedAmount);
			Map(x => x.AnnualTurnover);
			Map(x => x.IncorporationDate).CustomType<UtcDateTimeType>(); 
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using ApplicationMng.Repository;
	using NHibernate;

	public interface ICustomerAnalyticsRepository : IRepository<CustomerAnalytics>
	{
	}

	public class CustomerAnalyticsRepository : NHibernateRepositoryBase<CustomerAnalytics>, ICustomerAnalyticsRepository
	{
		public CustomerAnalyticsRepository(ISession session)
			: base(session)
		{
		}

	}
}
