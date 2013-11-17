using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database.Mapping {
	public class LoanTransactionMethodMap : ClassMap<Loans.LoanTransactionMethod> {
		public LoanTransactionMethodMap() {
			Table("LoanTransactionMethod");
			ReadOnly();
			Id(x => x.Id);
			Map(x => x.Name);
			Map(x => x.DisplaySort);
		} // constructor
	} // class LoanTransactionMethodMap
} // namespace EZBob.DatabaseLib.Model.Database.Mapping 
