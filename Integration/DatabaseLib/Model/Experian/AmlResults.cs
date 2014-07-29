namespace EZBob.DatabaseLib.Model.Experian
{
	using System;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using Iesi.Collections.Generic;
	using NHibernate;
	using NHibernate.Type;

	public class AmlResults
	{
		public virtual int Id { get; set; }
		public virtual string LookupKey { get; set; }
		public virtual int CustomerId { get; set; }
		public virtual long ServiceLogId { get; set; }
		public virtual DateTime Created { get; set; }
		public virtual string AuthenticationDecision { get; set; }
		public virtual int AuthenticationIndex { get; set; }
		public virtual string AuthIndexText { get; set; }
		public virtual decimal NumPrimDataItems { get; set; }
		public virtual decimal NumPrimDataSources { get; set; }
		public virtual decimal NumSecDataItems { get; set; }
		public virtual string StartDateOldestPrim { get; set; }
		public virtual string StartDateOldestSec { get; set; }
		public virtual string Error { get; set; }
		public virtual bool IsActive { get; set; }

		public virtual ISet<AmlResultsHighRiskRules> HighRiskRules { get; set; }

		public AmlResults()
		{
			HighRiskRules = new HashedSet<AmlResultsHighRiskRules>();
		}
	}

	public sealed class AmlResultsMap : ClassMap<AmlResults>
	{
		public AmlResultsMap()
		{
			Table("AmlResults");
			Id(x => x.Id);
			Map(x => x.LookupKey).Length(500);
			Map(x => x.CustomerId);
			Map(x => x.ServiceLogId);
			Map(x => x.Created).CustomType<UtcDateTimeType>().Not.Nullable();
			Map(x => x.AuthenticationDecision).Length(20);
			Map(x => x.AuthenticationIndex);
			Map(x => x.AuthIndexText).Length(500);
			Map(x => x.NumPrimDataItems);
			Map(x => x.NumPrimDataSources);
			Map(x => x.NumSecDataItems);
			Map(x => x.StartDateOldestPrim).Length(500);
			Map(x => x.StartDateOldestSec).Length(500);
			Map(x => x.Error);
			Map(x => x.IsActive);
			HasMany(x => x.HighRiskRules).
				KeyColumn("AmlResultId")
				.Cascade.All();
		}
	}

	public interface IAmlResultsRepository : IRepository<AmlResults>
	{
	}

	public class AmlResultsRepository : NHibernateRepositoryBase<AmlResults>, IAmlResultsRepository
	{
		public AmlResultsRepository(ISession session)
			: base(session)
		{
		}
	}
}