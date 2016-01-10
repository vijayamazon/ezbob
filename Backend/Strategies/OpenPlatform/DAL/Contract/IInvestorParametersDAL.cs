namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract {
    using System.Collections.Generic;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.Models;

    public interface IInvestorParametersDAL {
        List<int> GetInvestorsIds();
        InvestorParameters GetInvestorParameters(int InvestorId, RuleType ruleType);
     
        double GetGradeMonthlyInvestedAmount(int investorId, Grade grade);
        decimal GetGradeMaxScore(int investorId, Grade grade);
        double GetInvestorTotalMonthlyDeposits(int investorId);
        double GetInvestorBalanceMonthAgo(int investorId);
        double GetInvestorMonthlyFundingCapital(int investorId);
    }
}
