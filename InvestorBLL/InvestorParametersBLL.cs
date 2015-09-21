namespace InvestorBLL
{
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.Investor;
    using InvestorBLL.Contracts;
    using InvestorDAL.Contract;
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
