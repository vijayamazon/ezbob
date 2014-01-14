using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database {
	public class CompanyEmployeeCountMap : ClassMap<CompanyEmployeeCount> {
		public CompanyEmployeeCountMap() {
			Table("CompanyEmployeeCount");
			Cache.ReadWrite().Region("LongTerm").ReadWrite();

			Id(x => x.Id).GeneratedBy.Native();
			References(x => x.Customer, "CustomerId");
			References(x => x.Company, "CompanyId");
			Map(x => x.Created).CustomType<UtcDateTimeType>();
			Map(x => x.EmployeeCount);
			Map(x => x.TopEarningEmployeeCount);
			Map(x => x.BottomEarningEmployeeCount);
			Map(x => x.EmployeeCountChange);
			Map(x => x.TotalMonthlySalary);
		} // constructor
	} // class CompanyEmployeeCountMap
} // namespace
