namespace InvestorBLL.Contracts
{
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.Investor;

    public interface IInvestorService {
        List<int> GetMatchedInvestors(LoanParameters loan);
    }
}
