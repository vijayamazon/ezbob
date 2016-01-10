namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts
{
    using System.Collections.Generic;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.Models;

    public interface IInvestorParametersBLL {
        Dictionary<int, InvestorParameters> GetInvestorsParameters();
        InvestorParameters GetInvestorParameters(int InvestorId, int ruleType);
        double GetGradeAvailableAmount(int InvestorId, InvestorLoanCashRequest investorLoanCashRequest, int ruleType);
    }
}
