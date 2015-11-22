namespace EzService {
	using System.ServiceModel;
	using EzService.ActionResults.Investor;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceInvestor {
		[OperationContract]
		InvestorTypesActionResult InvestorLoadTypes(int underwriterID);
	} // interface IEzServiceInvestor
} // namespace
