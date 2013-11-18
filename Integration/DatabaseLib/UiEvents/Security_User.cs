using System.Linq;
using ApplicationMng.Repository;
using FluentNHibernate.Mapping;
using NHibernate;

namespace EZBob.DatabaseLib {
	#region class SecurityUser

	public class SecurityUser {
		#region public

		public virtual int ID { get; set; }
		public virtual string Name { get; set; }

		#endregion public
	} // class SecurityUser

	#endregion class SecurityUser

	#region class SecurityUserMap

	public class SecurityUserMap : ClassMap<SecurityUser> {
		#region public

		public SecurityUserMap() {
			Table("Security_User");
			ReadOnly();
			Id(x => x.ID, "UserID").GeneratedBy.Native();
			Map(x => x.Name, "UserName").Length(250);
		} // constructor

		#endregion public
	} // class SecurityUserMap

	#endregion class SecurityUserMap

	#region class SecurityUserRepository

	public class SecurityUserRepository : NHibernateRepositoryBase<SecurityUser> {
		public SecurityUserRepository(ISession session) : base(session) {}

		public SecurityUser FindByName(string sName) {
			return GetAll().FirstOrDefault(x => x.Name == sName);
		} // FindByName
	} // class SecurityUserRepository

	#endregion class SecurityUserRepository
} // namespace EZBob.DatabaseLib
