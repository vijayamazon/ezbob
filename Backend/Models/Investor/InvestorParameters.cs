namespace Ezbob.Backend.Models.Investor {
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;

    public class InvestorParameters
    {
        public int InvestorID { get; set; }
        public double DailyInvestmentAllowed { get; set; }
        public int WeeklyInvestmentAllowed { get; set; }
        public int MonthlyInvestmentAllowed { get; set; }
        public int GradeMin { get; set; } // TODO TO REMOVE
        public int GradeMax { get; set; } // TODO TO REMOVE        
        public double Balance { get; set; }
    }//class InvestorParameters

}//ns
