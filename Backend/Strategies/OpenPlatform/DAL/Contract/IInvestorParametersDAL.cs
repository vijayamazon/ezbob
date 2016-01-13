namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract {
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;

    public interface IInvestorParametersDAL {
        Dictionary<int, double> InvestorsBalance { get; }
        List<int> GetInvestorsIds();
        double GetGradeMonthlyInvestedAmount(int investorId, int grade);
        decimal GetGradeMaxScore(int investorId, int grade, int ruleType);
        double GetInvestorTotalMonthlyDeposits(int investorId);
        double GetInvestorMonthlyFundingCapital(int investorId);
        double GetFundedAmountPeriod(int investorId, InvesmentPeriod invesmentPeriod);
        List<I_InvestorParams> GetInvestorParametersDB(int investorId, RuleType ruleType);
    }
}
