namespace EzService {
	using System.Collections.Generic;
	using System.ServiceModel;
	using Ezbob.Backend.Models.Investor;
	using EzService.ActionResults.Investor;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceInvestor {
		[OperationContract]
		InvestorTypesActionResult InvestorLoadTypes(int underwriterID);

		[OperationContract]
		IntActionResult CreateInvestor(int underwriterID, InvestorModel investor, IEnumerable<InvestorContactModel> investorContacts, IEnumerable<InvestorBankAccountModel> investorBanks);
	} // interface IEzServiceInvestor
} // namespace
