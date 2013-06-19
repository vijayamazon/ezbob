using System;
using System.Linq;
using ApplicationMng.Repository;
using FluentNHibernate.Mapping;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_ExperianDataCache
	{
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
	}

    public interface IExperianDataCacheRepository : IRepository<MP_ExperianDataCache>
    {
        MP_ExperianDataCache GetPersonFromCache(string firstName, string surname, DateTime? birthDate, string postcode);
    }

    public class ExperianDataCacheRepository : NHibernateRepositoryBase<MP_ExperianDataCache>, IExperianDataCacheRepository
    {
        public ExperianDataCacheRepository(ISession session) : base(session)
        {
        }

        public MP_ExperianDataCache GetPersonFromCache(string firstName, string surname, DateTime? birthDate, string postcode)
        {
            return GetAll().FirstOrDefault(c => c.Name == firstName && c.Surname == surname && c.BirthDate == birthDate && c.PostCode == postcode);
        }
    }


    //-----------------------------------------------------------------------------------
    public sealed class MP_ExperianDataCacheMap : ClassMap<MP_ExperianDataCache>
    {
        public MP_ExperianDataCacheMap()
        {
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
        }
    }
}