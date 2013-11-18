using System.Linq;
using ApplicationMng.Repository;
using FluentNHibernate.Mapping;
using NHibernate;

namespace EZBob.DatabaseLib {
	#region class UiControl

	public class UiControl {
		#region public

		public virtual int ID { get; set; }
		public virtual string Name { get; set; }

		#endregion public
	} // class UiControl

	#endregion class UiControl

	#region class UiControlMap

	public class UiControlMap : ClassMap<UiControl> {
		#region public

		public UiControlMap() {
			Table("UiControls");
			Id(x => x.ID, "UiControlID").GeneratedBy.Native();
			Map(x => x.Name, "UiControlName").Length(255);
		} // constructor

		#endregion public
	} // class UiControlMap

	#endregion class UiControlMap

	#region class UiControlRepository

	public class UiControlRepository : NHibernateRepositoryBase<UiControl> {
		public UiControlRepository(ISession session) : base(session) {}

		public UiControl FindByName(string sName) {
			return GetAll().FirstOrDefault(x => x.Name == sName);
		} // FindByName
	} // class UiControlRepository

	#endregion class UiControlRepository
} // namespace EZBob.DatabaseLib
