namespace EZBob.DatabaseLib.Model.Loans {
	using System;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class LoanCharge {
		public virtual int Id { get; set; }
		public virtual decimal Amount { get; set; }
		public virtual decimal AmountPaid { get; set; }
		public virtual string State { get; set; }
		public virtual Loan Loan { get; set; }
		public virtual ConfigurationVariable ChargesType { get; set; }
		public virtual DateTime Date { get; set; }
		public virtual string Description { get; set; }

		public override string ToString() {
			return string.Format("Amount: {0}, Date: {1}, Amount paid: {2}, State: {3} Type: {4}", Amount, Date, AmountPaid, State, ChargesType.Name);
		} // ToString

		public virtual string GetDescription() {
			if (ChargesType == null)
				return Description;

			return string.IsNullOrEmpty(Description) ? ChargesType.Description : Description;
		} // GetDescription
	} // class LoanCharge

	public sealed class LoanChargesMap : ClassMap<LoanCharge> {
		public LoanChargesMap() {
			Id(x => x.Id).GeneratedBy.Native();
			Table("LoanCharges");
			Map(x => x.Amount);
			Map(x => x.AmountPaid);
			Map(x => x.State, "`State`");
			References(x => x.ChargesType, "ConfigurationVariableId");
			References(x => x.Loan, "LoanId");
			Map(x => x.Date, "`Date`").CustomType<UtcDateTimeType>();
			Map(x => x.Description);
		} // constructor
	} // class LoanChargesMap
} // namespace
