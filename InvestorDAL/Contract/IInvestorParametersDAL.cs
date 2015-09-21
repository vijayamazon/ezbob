namespace InvestorDAL.Contract
{
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.Investor;

    public interface IInvestorParametersDAL {
        List<InvestorParameters> GetInvestorParametersList();
    }
}
