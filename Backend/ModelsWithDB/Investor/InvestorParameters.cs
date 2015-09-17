namespace Ezbob.Backend.ModelsWithDB.Investor {
    using System;

    public class InvestorParameters
    {
        public int DailyInvestmentAllowed { get; set; }

        public int WeeklyInvestmentAllowed { get; set; }

        public int MonthlyInvestmentAllowed { get; set; }

        public int GradeMin { get; set; }

        public int GradeMax { get; set; }

    }//class InvestorParameters
}//ns
