namespace EZBob.DatabaseLib.Model.Database.Loans {
	using ApplicationMng.Repository;
	using NHibernate;
	using System.Linq;

	public interface IWriteOffReasonRepository : IRepository<WriteOffReason> {
		WriteOffReason Find(string sName);
	}

	public class WriteOffReasonRepository : NHibernateRepositoryBase<WriteOffReason>, IWriteOffReasonRepository {
		public WriteOffReasonRepository(ISession session) : base(session) { } // constructor

		public WriteOffReason Find(string sName) {
			WriteOffReason oResult = string.IsNullOrWhiteSpace(sName) ? null : GetAll().FirstOrDefault(x => x.ReasonName == sName);

			return oResult;
		} // Find
	} // class WiteOffReasonRepository
} // namespace EZBob.DatabaseLib.Model.Database.Loans

