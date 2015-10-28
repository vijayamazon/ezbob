namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
    public class InvestorParameters
    {
        public int InvestorID { get; set; }

        public int DailyInvestmentAllowed { get; set; }

        public int WeeklyInvestmentAllowed { get; set; }

        public int MonthlyInvestmentAllowed { get; set; }

        public int GradeMin { get; set; }

        public int GradeMax { get; set; }
    }//class InvestorParameters
}//ns
