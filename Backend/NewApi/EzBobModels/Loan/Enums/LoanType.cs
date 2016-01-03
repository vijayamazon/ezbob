using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.Loan.Enums
{
    using System.ComponentModel;

    public enum LoanType
    {
        [Description("Standard Loan")]
        StandardLoanType = 1, // DB table id
        [Description("HalfWay Loan")]
        HalfWayLoanType = 2,
        [Description("Alibaba Loan")]
        AlibabaLoanType = 3
    }
}
