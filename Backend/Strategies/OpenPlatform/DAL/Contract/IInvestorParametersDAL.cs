namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract
{
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;

	public interface IInvestorParametersDAL {
        List<InvestorParameters> GetInvestorParametersList();
    }
}
