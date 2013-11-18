using System.Linq;
using ApplicationMng.Repository;
using FluentNHibernate.Mapping;
using NHibernate;

namespace EZBob.DatabaseLib {
	#region class BrowserVersion

	public class BrowserVersion {
		#region public

		public virtual int ID { get; set; }
		public virtual string UserAgent { get; set; }

		#endregion public
	} // class BrowserVersion

	#endregion class BrowserVersion

	#region class BrowserVersionMap

	public class BrowserVersionMap : ClassMap<BrowserVersion> {
		#region public

		public BrowserVersionMap() {
			Table("BrowserVersions");
			Id(x => x.ID, "BrowserVersionID").GeneratedBy.Native();
			Map(x => x.UserAgent, "BrowserVersion").Length(1024);
		} // constructor

		#endregion public
	} // class BrowserVersionMap

	#endregion class BrowserVersionMap

	#region class BrowserVersionRepository

	public class BrowserVersionRepository : NHibernateRepositoryBase<BrowserVersion> {
		public BrowserVersionRepository(ISession session) : base(session) {}

		public BrowserVersion FindByName(string sName) {
			return GetAll().FirstOrDefault(x => x.UserAgent == sName);
		} // FindByName
	} // class BrowserVersionRepository

	#endregion class BrowserVersionRepository
} // namespace EZBob.DatabaseLib
