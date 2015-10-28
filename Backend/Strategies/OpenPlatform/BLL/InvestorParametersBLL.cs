namespace Ezbob.Backend.Strategies.OpenPlatform.BLL
{
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
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
