namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts
{
    using System.Collections.Generic;
    using Ezbob.Backend.Models.Investor;

    public interface IInvestorParametersBLL {
        List<InvestorParameters> GetInvestorParametersList();
    }
}
