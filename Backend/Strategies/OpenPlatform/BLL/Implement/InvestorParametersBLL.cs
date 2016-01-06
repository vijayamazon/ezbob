namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Implement
{
    using System.Collections.Generic;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using StructureMap.Attributes;

    public class InvestorParametersBLL : IInvestorParametersBLL
    {
        [SetterProperty]
        public IInvestorParametersDAL InvestorParametersDAL { get; set; }

        public  List<InvestorParameters> GetInvestorParametersList() {
            return InvestorParametersDAL.GetInvestorParametersList();
        }
    }
}
