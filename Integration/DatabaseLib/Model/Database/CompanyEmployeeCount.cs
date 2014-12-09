using System;
using System.Linq;

namespace EZBob.DatabaseLib.Model.Database {

	public class CompanyEmployeeCount {
		public static DateTime LongAgo() {
			return new DateTime(1976, 7, 1, 9, 30, 0, DateTimeKind.Utc);
		} // LongAgo

		public CompanyEmployeeCount() {
			Created = LongAgo();
		} // constructor

		public virtual int Id { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual Company Company { get; set; }
		public virtual DateTime Created { get; set; }
		public virtual int EmployeeCount { get; set; }
		public virtual int? TopEarningEmployeeCount { get; set; }
		public virtual int? BottomEarningEmployeeCount { get; set; }
		public virtual int? EmployeeCountChange { get; set; }
		public virtual double TotalMonthlySalary { get; set; }
	} // class CompanyEmployeeCount

	public class CompanyEmployeeCountInfo {
		public CompanyEmployeeCountInfo() {
			HasData = false;
		} // constructor

		public CompanyEmployeeCountInfo(Company oCompany) : this()
		{
			if (oCompany == null) return;

			var cec =
				oCompany.CompanyEmployeeCount.OrderBy(x => x.Created).LastOrDefault()
				?? new CompanyEmployeeCount();

			Created                    = cec.Created;
			BottomEarningEmployeeCount = cec.BottomEarningEmployeeCount;
			EmployeeCount              = cec.EmployeeCount;
			EmployeeCountChange        = cec.EmployeeCountChange;
			TopEarningEmployeeCount    = cec.TopEarningEmployeeCount;
			TotalMonthlySalary         = cec.TotalMonthlySalary;
			HasData                    = true;
		} // constructor

		public int EmployeeCount { get; set; }
		public int? TopEarningEmployeeCount { get; set; }
		public int? BottomEarningEmployeeCount { get; set; }
		public int? EmployeeCountChange { get; set; }
		public double TotalMonthlySalary { get; set; }
		public DateTime Created { get; set; }
		public bool HasData { get; private set; }
	} // class CompanyEmployeeCountInfo 

} // namespace
