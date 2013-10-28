using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database.Loans;
using FluentNHibernate.Mapping;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database.Loans {
	#region class LoanSource

	public class LoanSource {
		#region public

		#region properties - loaded from DB

		public virtual int ID { get; set; }
		public virtual string Name { get; set; }
		public virtual decimal? MaxInterest { get; set; }
		public virtual int? DefaultRepaymentPeriod { get; set; }
		public virtual bool IsCustomerRepaymentPeriodSelectionAllowed { get; set; }

		#endregion properties - loaded from DB

		#endregion public
	} // class LoanSource

	#endregion class LoanSource

	#region LoanSourceRepository

	public interface ILoanSourceRepository : IRepository<LoanSource> {
		LoanSource GetDefault();
	} // interface ILoanSourceRepository

	public class LoanSourceRepository : NHibernateRepositoryBase<LoanSource>, ILoanSourceRepository {
		public LoanSourceRepository(ISession session) : base(session) {} // constructor

		public LoanSource GetDefault() { return GetAll().Single(p => p.ID == 1); } // GetDefault
	} // class LoanSourceRepository

	#endregion LoanSourceRepository
} // namespace EZBob.DatabaseLib.Model.Database.Loans

namespace EZBob.DatabaseLib.Model.Database.Mapping {
	#region class LoanSourceMap

	public sealed class LoanSourceMap : ClassMap<LoanSource> {
		public LoanSourceMap() {
			Table("LoanSource");
			Cache.ReadOnly().Region("LongTerm").ReadOnly();

			Id(x => x.ID, "LoanSourceID");
			Map(x => x.Name, "LoanSourceName").Length(50);
			Map(x => x.MaxInterest);
			Map(x => x.DefaultRepaymentPeriod);
			Map(x => x.IsCustomerRepaymentPeriodSelectionAllowed);
		} // constructor
	} // class LoanSourceMap

	#endregion class LoanSourceMap
} // namespace EZBob.DatabaseLib.Model.Database.Mapping
