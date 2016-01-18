namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract {
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;

    public interface IInvestorParametersDAL {
        Dictionary<int, decimal> InvestorsBalance { get; }
        Dictionary<int, I_Parameter> InvestorsParameters { get; }
        List<int> GetInvestorsIds();
        decimal GetGradeMonthlyInvestedAmount(int investorId, Grade grade);
        decimal GetGradePercent(int investorId, int grade, int ruleType);
        decimal GetInvestorTotalMonthlyDeposits(int investorId);
        decimal GetInvestorMonthlyFundingCapital(int investorId);
        decimal GetFundedAmountPeriod(int investorId, InvesmentPeriod invesmentPeriod);
        List<I_InvestorParams> GetInvestorParametersDB(int investorId, RuleType ruleType);
        int GetInvestorWithLatestLoanDate(List<int> investorsList);
    }
}
