using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database {

	public interface IMP_VatReturnEntryNameRepositry : IRepository<MP_VatReturnEntryName> {} // IMP_VatReturnEntryNameRepositry

	public class MP_VatReturnEntryNameRepositry : NHibernateRepositoryBase<MP_VatReturnEntryName>, IMP_VatReturnEntryNameRepositry {
		public MP_VatReturnEntryNameRepositry(ISession session) : base(session) {} // constructor
	} // class MP_VatReturnEntryNameRepositry

} // namespace
