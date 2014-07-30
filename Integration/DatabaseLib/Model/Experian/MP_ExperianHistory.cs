namespace EZBob.DatabaseLib.Model.Experian
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Database;
	using Ezbob.Utils.Extensions;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;
	using ApplicationMng.Repository;
	using NHibernate;

	public class MP_ExperianHistory
	{
		public virtual int Id { get; set; }
		public virtual int? CustomerId { get; set; }
		public virtual string Type { get; set; }
		public virtual DateTime Date { get; set; }
		public virtual long ServiceLogId { get; set; }
		public virtual int? Score { get; set; }
		public virtual int? CII { get; set; }
		public virtual decimal? CaisBalance { get; set; }
		public virtual int? DirectorId { get; set; }
		public virtual string CompanyRefNum { get; set; }
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

			Map(x => x.CustomerId);
			Map(x => x.DirectorId);
			Map(x => x.CompanyRefNum);
		}
	}

	public interface IExperianHistoryRepository : IRepository<MP_ExperianHistory>
	{
		IEnumerable<MP_ExperianHistory> GetCustomerConsumerHistory(int customerId);
		IEnumerable<MP_ExperianHistory> GetDirectorConsumerHistory(int directorId);
		IEnumerable<MP_ExperianHistory> GetCompanyHistory(string companyRefNum, bool isLimited);
		void SaveOrUpdateConsumerHistory(long serviceLogId, DateTime date, int? customerId, int? DirectorId, int? score, int? caisBalance, int? cii);
	}

	public class ExperianHistoryRepository : NHibernateRepositoryBase<MP_ExperianHistory>, IExperianHistoryRepository
	{

		public ExperianHistoryRepository(ISession session)
			: base(session)
		{
		}

		public IEnumerable<MP_ExperianHistory> GetCustomerConsumerHistory(int customerId)
		{
			return GetAll().Where(x => x.CustomerId == customerId && x.DirectorId == null && x.Type == ExperianServiceType.Consumer.DescriptionAttr());
		}

		public IEnumerable<MP_ExperianHistory> GetDirectorConsumerHistory(int directorId)
		{
			return GetAll().Where(x => x.DirectorId == directorId && x.Type == ExperianServiceType.Consumer.DescriptionAttr());
		}
		
		public IEnumerable<MP_ExperianHistory> GetCompanyHistory(string companyRefNum, bool isLimited)
		{
			return GetAll().Where(x => x.CompanyRefNum == companyRefNum && x.Type == (isLimited ? ExperianServiceType.LimitedData.DescriptionAttr() : ExperianServiceType.NonLimitedData.DescriptionAttr()));
		}

		public void SaveOrUpdateConsumerHistory(long serviceLogId, DateTime date, int? customerId, int? directorId, int? score, int? caisBalance,
		                                int? cii)
		{
			if (!GetAll().Any(x => x.ServiceLogId == serviceLogId))
			{
				SaveOrUpdate(new MP_ExperianHistory
					{
						ServiceLogId = serviceLogId,
						Date = date,
						CustomerId = customerId,
						DirectorId = directorId,
						Score = score,
						CII = cii,
						CaisBalance = caisBalance,
						Type = ExperianServiceType.Consumer.DescriptionAttr()
					});
			}
		}
	}
}