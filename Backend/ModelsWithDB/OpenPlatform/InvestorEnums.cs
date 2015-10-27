using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezbob.Backend.ModelsWithDB.Investor
{
    public enum Operator
    {
        Or = 0,
        And = 1,
        GreaterThan = 2,
        LessThan = 3,
        Equal = 4,
        NotEqual = 5,
        Not = 6
    };

    public enum Field
    {
        DailyInvestmentAllowed = 0,
        WeeklyInvestmentAllowed = 1,
        MonthlyInvestmentAllowed = 2,
        GradeMin = 3,
        GradeMax = 4,
    };

    public enum Grade
    {
        A,
        B, 
        C, 
        D, 
        E,
        F
    };
}
