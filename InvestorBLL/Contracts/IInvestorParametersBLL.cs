namespace InvestorBLL.Contracts
{
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.Investor;

    public interface IInvestorParametersBLL {
        List<InvestorParameters> GetInvestorParametersList();
    }
}
