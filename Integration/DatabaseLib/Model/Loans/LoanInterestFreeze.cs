using System;
using System.Globalization;
using EZBob.DatabaseLib.Model.Database.Loans;
using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database.Loans {

	public class LoanInterestFreeze {

		public virtual int Id { get; set; }
		public virtual Loan Loan { get; set; }
		public virtual DateTime? StartDate { get; set; }
		public virtual DateTime? EndDate { get; set; }
		public virtual decimal InterestRate { get; set; }
		public virtual DateTime ActivationDate { get; set; }
		public virtual DateTime? DeactivationDate { get; set; }

		public virtual bool Contains(DateTime oDate) {
			if (!StartDate.HasValue && !EndDate.HasValue)
				return true;

			bool bLTend = (EndDate.HasValue && (oDate <= EndDate));

			if (!StartDate.HasValue && bLTend)
				return true;

			bool bGTstart = (StartDate.HasValue && (StartDate <= oDate));

			if (bGTstart && !EndDate.HasValue)
				return true;

			return bGTstart && bLTend;
		} // Contains

		public override string ToString() {
			return string.Format(
				"{0}|{1}|{2}|{3}|{4}|{5}",
				StartDate.HasValue ? StartDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "",
				EndDate.HasValue ? EndDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "",
				InterestRate,
				ActivationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
				DeactivationDate.HasValue ? DeactivationDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "",
				Id
			);
		} // ToString

	} // class LoanInterestFreeze

} // namespace EZBob.DatabaseLib.Model.Database.Loans

namespace EZBob.DatabaseLib.Model.Database.Mapping {

	public sealed class LoanInterestFreezeMap : ClassMap<LoanInterestFreeze> {
		public LoanInterestFreezeMap() {
			Id(x => x.Id).GeneratedBy.Native();
			Table("LoanInterestFreeze");
			Cache.ReadWrite().Region("LongTerm").ReadWrite();
			References(x => x.Loan, "LoanId");
			Map(x => x.StartDate).CustomType<UtcDateTimeType>();
			Map(x => x.EndDate).CustomType<UtcDateTimeType>();
			Map(x => x.InterestRate);
			Map(x => x.ActivationDate).CustomType<UtcDateTimeType>();
			Map(x => x.DeactivationDate).CustomType<UtcDateTimeType>();
		} // constructor
	} // class LoanInterestFreezeMap

} // namespace EZBob.DatabaseLib.Model.Database.Mapping
