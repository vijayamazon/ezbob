namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts {
    using System.Collections.Generic;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.Models;

    public interface IInvestorParametersBLL {
        List<int> GetInvestorsIds();
        InvestorParameters GetInvestorParameters(int InvestorId, RuleType ruleType);
        decimal GetGradeAvailableAmount(int InvestorId, InvestorLoanCashRequest investorLoanCashRequest, int ruleType);
        int GetInvestorWithLatestLoanDate(List<int> investorsList);
    }
}
