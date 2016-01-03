using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.Loan
{
    public class LoanSource {
        //loankind enum value
        public int Id { get; set; }
        //loankind enum value name
        public string Name { get; set; }
        public decimal? MaxInterest { get; set; }
        public int? DefaultRepaymentPeriod { get; set; }
        public bool IsCustomerRepaymentPeriodSelectionAllowed { get; set; }
        public int? MaxEmployeeCount { get; set; }
        public decimal? MaxAnnualTurnover { get; set; }
        public bool IsDefault { get; set; }
        public int? AlertOnCustomerReasonType { get; set; }
    }
}
