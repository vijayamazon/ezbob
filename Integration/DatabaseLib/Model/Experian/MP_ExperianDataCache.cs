namespace EZBob.DatabaseLib.Model.Database {
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using Ezbob.Database;
	using Ezbob.Logger;
	using log4net;

	public class MP_ExperianDataCache {
		public virtual long Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string Surname { get; set; }
		public virtual string PostCode { get; set; }
		public virtual DateTime LastUpdateDate { get; set; }
		public virtual DateTime? BirthDate { get; set; }
		public virtual string ExperianResult { get; set; }
		public virtual string JsonPacket { get; set; }
		public virtual string JsonPacketInput { get; set; }
		public virtual string CompanyRefNumber { get; set; }
		public virtual long? CustomerId { get; set; }
		public virtual long? DirectorId { get; set; }
		public virtual int? ExperianScore { get; set; }
	} // class MP_ExperianDataCache

	public interface IExperianDataCacheRepository : IRepository<MP_ExperianDataCache> {
		MP_ExperianDataCache GetPersonFromCache(string firstName, string surname, DateTime? birthDate, string postcode);
	} // interface IExperianDataCacheRepository

	public class ExperianDataCacheRepository : NHibernateRepositoryBase<MP_ExperianDataCache>, IExperianDataCacheRepository {
		public ExperianDataCacheRepository(ISession session) : base(session) {
			m_oRetryer = new SqlRetryer(oLog: new SafeILog(LogManager.GetLogger(typeof(ExperianDataCacheRepository))));
		} // constructor

		public MP_ExperianDataCache GetPersonFromCache(string firstName, string surname, DateTime? birthDate, string postcode) {
			return m_oRetryer.Retry(() =>
				GetAll()
					.OrderByDescending(x => x.LastUpdateDate)
					.FirstOrDefault(c => c.Name == firstName && c.Surname == surname && c.BirthDate == birthDate && c.PostCode == postcode)
			);
		} // GetPersonFromCache

		public MP_ExperianDataCache GetCustomerFromCache(int customerId, string firstName, string surname, DateTime? birthDate, string postcode)
		{
			return m_oRetryer.Retry(() =>
				GetAll()
					.OrderByDescending(x => x.LastUpdateDate)
					.FirstOrDefault(c => (c.Name == firstName && c.Surname == surname && c.BirthDate == birthDate && c.PostCode == postcode) || (c.CustomerId == customerId && (c.DirectorId == null || c.DirectorId==0)))
			);
		} // GetPersonFromCache

		public MP_ExperianDataCache GetDirectorFromCache(int directorId, string firstName, string surname, DateTime? birthDate, string postcode)
		{
			return m_oRetryer.Retry(() =>
			                        GetAll()
				                        .OrderByDescending(x => x.LastUpdateDate)
				                        .FirstOrDefault(c => (c.Name == firstName && c.Surname == surname && c.BirthDate == birthDate && c.PostCode == postcode) || (c.DirectorId == directorId)));
		} // GetPersonFromCache

		private readonly SqlRetryer m_oRetryer;
	} // class ExperianDataCacheRepository

	public sealed class MP_ExperianDataCacheMap : ClassMap<MP_ExperianDataCache> {
		public MP_ExperianDataCacheMap() {
			Table("MP_ExperianDataCache");
			Id(x => x.Id);
			Map(x => x.Name);
			Map(x => x.Surname);
			Map(x => x.PostCode);
			Map(x => x.LastUpdateDate);
			Map(x => x.BirthDate);
			Map(x => x.ExperianResult);
			Map(x => x.CompanyRefNumber);
			Map(x => x.JsonPacket).CustomType("StringClob").LazyLoad();
			Map(x => x.JsonPacketInput).CustomType("StringClob").LazyLoad();
			Map(x => x.DirectorId);
			Map(x => x.CustomerId);
			Map(x => x.ExperianScore);
		} // constructor
	} // class MP_ExperianDataCacheMap
} // namespace
