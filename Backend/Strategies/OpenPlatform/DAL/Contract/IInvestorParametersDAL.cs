namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract
{
    using System.Collections.Generic;
    using Ezbob.Backend.Models.Investor;

    public interface IInvestorParametersDAL {
        List<InvestorParameters> GetInvestorParametersList();
    }
}
