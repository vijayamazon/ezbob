namespace EZBob.DatabaseLib.Model.Database {
	public class WhatsNewExcludeCustomerOrigin {
		public virtual long EntryID { get; set; }
		public virtual int WhatsNewId { get; set; }
		public virtual int CustomerOriginID { get; set; }
	} // class WhatsNewExcludeCustomerOrigin
} // namespace

namespace EZBob.DatabaseLib.Model.Database.Mapping {
	using FluentNHibernate.Mapping;

	public class WhatsNewExcludeCustomerOriginMap : ClassMap<WhatsNewExcludeCustomerOrigin> {
		public WhatsNewExcludeCustomerOriginMap() {
			Table("WhatsNewExcludeCustomerOrigin");
			ReadOnly();
			Id(x => x.EntryID, "EntryID");
			Map(x => x.WhatsNewId);
			Map(x => x.CustomerOriginID);
		} // constructor
	} // class WhatsNewExcludeCustomerOriginMap
} // namespace

namespace EZBob.DatabaseLib.Model.Database.Repository {
	using ApplicationMng.Repository;
	using NHibernate;

	public interface IWhatsNewExcludeCustomerOriginRepository : IRepository<WhatsNewExcludeCustomerOrigin> {
	} // interface IWhatsNewExcludeCustomerOriginRepository

	public class WhatsNewExcludeCustomerOriginRepository :
		NHibernateRepositoryBase<WhatsNewExcludeCustomerOrigin>,
		IWhatsNewExcludeCustomerOriginRepository
	{
		public WhatsNewExcludeCustomerOriginRepository(ISession session) : base(session) {}
	} // class WhatsNewExcludeCustomerOriginRepository
} // namespace
