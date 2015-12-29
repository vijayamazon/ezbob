namespace EzService {
	using System.Collections.Generic;
	using System.ServiceModel;
	using Ezbob.Backend.Models.Investor;
	using EzService.ActionResults.Investor;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceInvestor {
		[OperationContract]
		IntActionResult CreateInvestor(int underwriterID, InvestorModel investor, IEnumerable<InvestorContactModel> investorContacts, IEnumerable<InvestorBankAccountModel> investorBanks);

		[OperationContract]
		InvestorActionResult LoadInvestor(int underwriterID, int investorID);

		[OperationContract]
		BoolActionResult ManageInvestorContact(int underwriterID, InvestorContactModel investorContact);

		[OperationContract]
		BoolActionResult ManageInvestorBankAccount(int underwriterID, InvestorBankAccountModel investorBank);
	} // interface IEzServiceInvestor
} // namespace
