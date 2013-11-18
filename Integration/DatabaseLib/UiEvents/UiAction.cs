using System.Linq;
using ApplicationMng.Repository;
using FluentNHibernate.Mapping;
using NHibernate;

namespace EZBob.DatabaseLib {
	#region class UiAction

	public class UiAction {
		#region public

		public virtual int ID { get; set; }
		public virtual string Name { get; set; }

		#endregion public
	} // class UiAction

	#endregion class UiAction

	#region class UiActionMap

	public class UiActionMap : ClassMap<UiAction> {
		#region public

		public UiActionMap() {
			Table("UiActions");
			Id(x => x.ID, "UiActionID").GeneratedBy.Native();
			Map(x => x.Name, "UiActionName").Length(255);
		} // constructor

		#endregion public
	} // class UiActionMap

	#endregion class UiActionMap

	#region class UiActionRepository

	public class UiActionRepository : NHibernateRepositoryBase<UiAction> {
		public UiActionRepository(ISession session) : base(session) {}

		public UiAction FindByName(string sName) {
			return GetAll().FirstOrDefault(x => x.Name == sName);
		} // FindByName
	} // class UiActionRepository

	#endregion class UiActionRepository
} // namespace EZBob.DatabaseLib
