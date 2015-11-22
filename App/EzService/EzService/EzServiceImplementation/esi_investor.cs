namespace EzService.EzServiceImplementation
{
	using System.Collections.Generic;
	using EzService.ActionResults.Investor;

	partial class EzServiceImplementation
	{
		public InvestorTypesActionResult InvestorLoadTypes(int underwriterID)
		{
			return new InvestorTypesActionResult {
				InvestorBankAccountTypes = new Dictionary<string, string>{
					{"1","Funding"},
					{"2","Repayments"},
					{"3","Bridging"},
				},
				InvestorTypes = new Dictionary<string, string>{
					{"1","Institutional"},
					{"2","Private"},
					{"3","Hedge Fund"},
				}
			};
		}
	}
}

