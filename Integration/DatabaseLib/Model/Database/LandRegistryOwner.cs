namespace EZBob.DatabaseLib.Model.Database {
	using FluentNHibernate.Mapping;

	public class LandRegistryOwner
	{
		public virtual int Id { get; set; }
		public virtual LandRegistry LandRegistry { get; set; }
		public virtual string FirstName { get; set; }
		public virtual string LastName { get; set; }
		public virtual string CompanyName { get; set; }
		public virtual string CompanyRegistrationNumber { get; set; }
	} // class LandRegistryOwner

	public class LandRegistryOwnerMap : ClassMap<LandRegistryOwner>
	{
		public LandRegistryOwnerMap()
		{
			Table("LandRegistryOwner");
			Cache.ReadWrite().Region("LongTerm").ReadWrite();

			Id(x => x.Id);
			References(x => x.LandRegistry, "LandRegistryId");
			Map(x => x.FirstName).Length(100);
			Map(x => x.LastName).Length(100);
			Map(x => x.CompanyName).Length(100);
			Map(x => x.CompanyRegistrationNumber).Length(100);
		} // constructor
	} // class LandRegistryOwnerMap

} // namespace EZBob.DatabaseLib.Model.Database

namespace EZBob.DatabaseLib.Repository {
	using ApplicationMng.Repository;
	using Model.Database;
	using NHibernate;

	public class LandRegistryOwnerRepository : NHibernateRepositoryBase<LandRegistryOwner> {
		public LandRegistryOwnerRepository(ISession session) : base(session) { } // constructor

	} // class LandRegistryOwnerRepository
} // namespace EZBob.DatabaseLib.Repository
