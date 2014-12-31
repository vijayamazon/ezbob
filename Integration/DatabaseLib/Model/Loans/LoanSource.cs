using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database.Loans;
using FluentNHibernate.Mapping;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database.Loans {
	public enum LoanSourceName {
		Standard,
		EU,
		COSME,
	}

	public class LoanSource {

		public virtual int ID { get; set; }
		public virtual string Name { get; set; }
		public virtual decimal? MaxInterest { get; set; }
		public virtual int? DefaultRepaymentPeriod { get; set; }
		public virtual bool IsCustomerRepaymentPeriodSelectionAllowed { get; set; }
		public virtual int? MaxEmployeeCount { get; set; }
		public virtual decimal? MaxAnnualTurnover { get; set; }
		public virtual bool IsDefault { get; set; }
		public virtual int? AlertOnCustomerReasonType { get; set; }

	} // class LoanSource

	public interface ILoanSourceRepository : IRepository<LoanSource> {
		LoanSource GetDefault();
		LoanSource GetByName(string name);
	} // interface ILoanSourceRepository

	public class LoanSourceRepository : NHibernateRepositoryBase<LoanSource>, ILoanSourceRepository {
		public LoanSourceRepository(ISession session) : base(session) {} // constructor

		public LoanSource GetDefault() { return GetAll().FirstOrDefault(p => p.IsDefault) ?? GetAll().Single(p => p.ID == 1); }

		public LoanSource GetByName(string name) {
			return GetAll().FirstOrDefault(x => x.Name == name);
		}

// GetDefault
	} // class LoanSourceRepository

} // namespace EZBob.DatabaseLib.Model.Database.Loans

namespace EZBob.DatabaseLib.Model.Database.Mapping {

	public sealed class LoanSourceMap : ClassMap<LoanSource> {
		public LoanSourceMap() {
			Table("LoanSource");
			Cache.ReadOnly().Region("LongTerm").ReadOnly();

			Id(x => x.ID, "LoanSourceID");
			Map(x => x.Name, "LoanSourceName").Length(50);
			Map(x => x.MaxInterest);
			Map(x => x.DefaultRepaymentPeriod);
			Map(x => x.IsCustomerRepaymentPeriodSelectionAllowed);
			Map(x => x.MaxEmployeeCount);
			Map(x => x.MaxAnnualTurnover);
			Map(x => x.IsDefault);
			Map(x => x.AlertOnCustomerReasonType);
		} // constructor
	} // class LoanSourceMap

} // namespace EZBob.DatabaseLib.Model.Database.Mapping
