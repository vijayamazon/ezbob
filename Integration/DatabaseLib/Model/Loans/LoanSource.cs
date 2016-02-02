namespace EZBob.DatabaseLib.Model.Database.Loans {
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using Ezbob.Database;
	using Ezbob.Logger;
	using NHibernate;

	public class LoanSource {
		[FieldName("LoanSourceID")]
		public virtual int ID { get; set; }
		[FieldName("LoanSourceName")]
		public virtual string Name { get; set; }
		public virtual decimal? MaxInterest { get; set; }
		public virtual int? DefaultRepaymentPeriod { get; set; }
		public virtual bool IsCustomerRepaymentPeriodSelectionAllowed { get; set; }
		public virtual int? MaxEmployeeCount { get; set; }
		public virtual decimal? MaxAnnualTurnover { get; set; }
		public virtual int? AlertOnCustomerReasonType { get; set; }
	} // class LoanSource

	public interface ILoanSourceRepository : IRepository<LoanSource> {
		LoanSource GetDefault(int customerID);
	} // interface ILoanSourceRepository

	public class LoanSourceRepository : NHibernateRepositoryBase<LoanSource>, ILoanSourceRepository {
		public LoanSourceRepository(ISession session) : base(session) {} // constructor

		public LoanSource GetDefault(int customerID) {
			SafeReader sr = Library.Instance.DB.GetFirst(
				"GetLoanSource",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@LoanSourceID"),
				new QueryParameter("@CustomerID", customerID)
			);

			int loanSourceID = sr.IsEmpty ? 1 : sr["LoanSourceID"];

			return GetAll().Single(p => p.ID == loanSourceID);
		} // GetDefault
	} // class LoanSourceRepository

	public static class LoanSourceExt {
		public static decimal ValidateInterestRate(this LoanSource loanSource, decimal setInterestRate) {
			if (loanSource == null) { // Should never happen.
				log.Alert("Cannot validate interest rate: loan source not specified.");
				return setInterestRate;
			} // if

			bool interestRateIsGood = !loanSource.MaxInterest.HasValue || (setInterestRate <= loanSource.MaxInterest.Value);

			if (interestRateIsGood)
				return setInterestRate;

			log.Warn(
				"Too big interest ({0}) was assigned for this loan source - adjusting to {1}.",
				setInterestRate,
				loanSource.MaxInterest.Value
			);

			return loanSource.MaxInterest.Value;
		} // ValidateInterestRate

		public static int ValidateRepaymentPeriod(this LoanSource loanSource, int setRepaymentPeriod) {
			if (loanSource == null) { // Should never happen.
				log.Alert("Cannot validate repayment period: loan source not specified.");
				return setRepaymentPeriod;
			} // if

			bool repaymentPeriodIsGood =
				!loanSource.DefaultRepaymentPeriod.HasValue ||
				(setRepaymentPeriod >= loanSource.DefaultRepaymentPeriod);

			if (repaymentPeriodIsGood)
				return setRepaymentPeriod;

			log.Warn(
				"Too small repayment period ({0}) was assigned for this loan source - adjusting to {1}.",
				setRepaymentPeriod,
				loanSource.DefaultRepaymentPeriod
			);

			return loanSource.DefaultRepaymentPeriod.Value;
		} // ValidateRepaymentPeriod

		public static bool ValidatePeriodSelectionAllowed(this LoanSource loanSource, bool setPeriodSelectionAllowed) {
			if (loanSource == null) { // Should never happen.
				log.Alert("Cannot validate period selection allowed: loan source not specified.");
				return setPeriodSelectionAllowed;
			} // if

			bool periodSelectionAllowedIsGood =
				loanSource.IsCustomerRepaymentPeriodSelectionAllowed ||
				!setPeriodSelectionAllowed;

			if (periodSelectionAllowedIsGood)
				return setPeriodSelectionAllowed;

			log.Warn(
				"Wrong period selection option ('enabled') was assigned for this loan source - " +
				"adjusting to ('disabled')."
			);

			return false;
		} // ValidatePeriodSelectionAllowed

		private static readonly ASafeLog log = new SafeILog(typeof(LoanSource));
	} // class LoanSourceExt
} // namespace EZBob.DatabaseLib.Model.Database.Loans

namespace EZBob.DatabaseLib.Model.Database.Mapping {
	using EZBob.DatabaseLib.Model.Database.Loans;
	using FluentNHibernate.Mapping;

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
			Map(x => x.AlertOnCustomerReasonType);
		} // constructor
	} // class LoanSourceMap
} // namespace EZBob.DatabaseLib.Model.Database.Mapping
