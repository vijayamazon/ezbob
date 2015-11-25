namespace EzService.EzServiceImplementation {
	using System.Collections.Generic;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.Strategies.Investor;
	using EzService.ActionResults.Investor;

	partial class EzServiceImplementation : IEzServiceInvestor {
		public InvestorTypesActionResult InvestorLoadTypes(int underwriterID) {
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

		public IntActionResult CreateInvestor(int underwriterID, InvestorModel investor, IEnumerable<InvestorContactModel> investorContacts, IEnumerable<InvestorBankAccountModel> investorBanks) {
			CreateInvestor strategy;
			var metadata = ExecuteSync(out strategy, null, underwriterID, investor, investorContacts, investorBanks);
			return new IntActionResult{
				MetaData = metadata, 
				Value = strategy.InvestorID
			};
		}
	}
}

