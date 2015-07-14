namespace EzService {
	using System.Collections.Generic;
	using System.ServiceModel;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using EzService.ActionResults;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceNewLoan {
		[OperationContract]
		IntActionResult AddCashRequest(int userID, NL_CashRequests cashRequest);

		[OperationContract]
		IntActionResult AddDecision(int userID, int customerID, NL_Decisions decision, long? oldCashRequest, IEnumerable<NL_DecisionRejectReasons> decisionRejectReasons);

		[OperationContract]
		IntActionResult AddOffer(int userID, int customerID, NL_Offers offer);

		[OperationContract]
		NL_Offers GetLastOffer(int userID, int customerID);

		[OperationContract]
		IntActionResult AddLoanLegals(int userID, int customerID, NL_LoanLegals loanLegals);

		[OperationContract]
		IntActionResult AddLoanOptions(int userID, int customerID, NL_LoanOptions loanOptions);

		[OperationContract]
		IntActionResult AddLoan(NL_Model loanModel);

		[OperationContract]
		NL_Model AddPayment(NL_Model loanModel);

		[OperationContract]
		ReschedulingActionResult RescheduleLoan(int userID, int customerID, ReschedulingArgument reschedulingArgument);

		[OperationContract]
		NewLoanModelActionResult CalculateLoanSchedule(int userID, int customerID, NL_Model model);

	} // interface IEzServiceNewLoan

} // namespace EzService