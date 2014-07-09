namespace EZBob.DatabaseLib.Model.Experian
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Database;
	using Ezbob.Utils.Extensions;
	using FluentNHibernate.Mapping;
	using NHibernate.Mapping;
	using NHibernate.Type;
	using ApplicationMng.Repository;
	using NHibernate;

	public class MP_ExperianHistory
	{
		public virtual int Id { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual string Type { get; set; }
		public virtual DateTime Date { get; set; }
		public virtual long ServiceLogId { get; set; }
		public virtual int Score { get; set; }
		public virtual int? CII { get; set; }
		public virtual decimal? CaisBalance { get; set; }
	}

	public sealed class MP_ExperianHistoryMap : ClassMap<MP_ExperianHistory>
	{
		public MP_ExperianHistoryMap()
		{
			Table("MP_ExperianHistory");
			Id(x => x.Id);
			Map(x => x.Type);
			Map(x => x.Date).CustomType<UtcDateTimeType>();
			Map(x => x.Score);
			Map(x => x.CII);
			Map(x => x.CaisBalance);
			Map(x => x.ServiceLogId);
			References(x => x.Customer, "CustomerId");
		}
	}

	public interface IExperianHistoryRepository : IRepository<MP_ExperianHistory>
	{
		IEnumerable<MP_ExperianHistory> GetConsumerHistory(Customer customer);
		IEnumerable<MP_ExperianHistory> GetCompanyHistory(Customer customer, bool isLimited);
		bool HasHistory(Customer customer, ExperianServiceType type);
	}

	public class ExperianHistoryRepository : NHibernateRepositoryBase<MP_ExperianHistory>, IExperianHistoryRepository
	{

		public ExperianHistoryRepository(ISession session)
			: base(session)
		{
		}

		public IEnumerable<MP_ExperianHistory> GetConsumerHistory(Customer customer)
		{
			return GetAll().Where(x => x.Customer == customer && x.Type == ExperianServiceType.Consumer.DescriptionAttr());
		}

		public IEnumerable<MP_ExperianHistory> GetCompanyHistory(Customer customer, bool isLimited)
		{
			return GetAll().Where(x => x.Customer == customer && x.Type == (isLimited ? ExperianServiceType.LimitedData.DescriptionAttr() : ExperianServiceType.NonLimitedData.DescriptionAttr()));
		}

		public bool HasHistory(Customer customer, ExperianServiceType type)
		{
			return GetAll().Any(x => x.Customer == customer && x.Type == type.DescriptionAttr());
		}
	}
}