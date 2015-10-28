namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts
{
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;

	public interface IInvestorParametersBLL {
        List<InvestorParameters> GetInvestorParametersList();
    }
}
