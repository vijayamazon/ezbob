namespace EZBob.DatabaseLib.Model.Database {
	using System;

	public class WhatsNew {
		public virtual int Id { get; set; }
		public virtual string WhatsNewHtml { get; set; }
		public virtual DateTime ValidFrom { get; set; }
		public virtual DateTime ValidUntil { get; set; }
		public virtual bool Active { get; set; }
	} // class WhatsNew
} // namespace

namespace EZBob.DatabaseLib.Model.Database.Mapping {
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class WhatsNewMap : ClassMap<WhatsNew> {
		public WhatsNewMap() {
			Table("WhatsNew");
			Id(x => x.Id);
			Map(x => x.WhatsNewHtml, "WhatsNew");
			Map(x => x.ValidFrom).CustomType<UtcDateTimeType>();
			Map(x => x.ValidUntil).CustomType<UtcDateTimeType>();
			Map(x => x.Active);
		} // constructor
	} // class WhatsNewMap
} // namespace

namespace EZBob.DatabaseLib.Model.Database.Repository {
	using ApplicationMng.Repository;
	using NHibernate;

	public interface IWhatsNewRepository : IRepository<WhatsNew> {
	} // interface IWhatsNewRepository

	public class WhatsNewRepository : NHibernateRepositoryBase<WhatsNew>, IWhatsNewRepository {
		public WhatsNewRepository(ISession session) : base(session) {}
	} // class WhatsNewRepository
} // namespace
