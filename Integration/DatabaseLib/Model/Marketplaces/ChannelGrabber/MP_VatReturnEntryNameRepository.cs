using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database {
	#region interface IMP_VatReturnEntryNameRepositry

	public interface IMP_VatReturnEntryNameRepositry : IRepository<MP_VatReturnEntryName> {} // IMP_VatReturnEntryNameRepositry

	#endregion interface IMP_VatReturnEntryNameRepositry

	#region class MP_VatReturnEntryNameRepositry

	public class MP_VatReturnEntryNameRepositry : NHibernateRepositoryBase<MP_VatReturnEntryName>, IMP_VatReturnEntryNameRepositry {
		public MP_VatReturnEntryNameRepositry(ISession session) : base(session) {} // constructor
	} // class MP_VatReturnEntryNameRepositry

	#endregion class MP_VatReturnEntryNameRepositry
} // namespace
