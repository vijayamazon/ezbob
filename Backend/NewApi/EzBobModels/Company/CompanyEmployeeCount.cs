using System;

namespace EzBobModels.Company {
    /// <summary>
    /// Represents 'CompanyEmployeeCount' table.
    /// </summary>
    public class CompanyEmployeeCount {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime Created { get; set; }
        public int EmployeeCount { get; set; }
        public int? TopEarningEmployeeCount { get; set; }
        public int? BottomEarningEmployeeCount { get; set; }
        public int? EmployeeCountChange { get; set; }
        public decimal TotalMonthlySalary { get; set; }
        public int CompanyId { get; set; }
    }
}
