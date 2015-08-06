namespace EzService {
	using System.Collections.Generic;
	using System.ServiceModel;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using EzService.ActionResults;

    [ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceNewLoan {
		[OperationContract]
		LongActionResult AddCashRequest(int userID, NL_CashRequests cashRequest);

		[OperationContract]
		LongActionResult AddDecision(int userID, int customerID, NL_Decisions decision, long? oldCashRequest, IEnumerable<NL_DecisionRejectReasons> decisionRejectReasons);

		[OperationContract]
		LongActionResult AddOffer(int userID, int customerID, NL_Offers offer, List<NL_OfferFees> fees = null);

		[OperationContract]
		NL_Offers GetLastOffer(int userID, int customerID);

		[OperationContract]
		LongActionResult AddLoanLegals(int userID, int customerID, NL_LoanLegals loanLegals);

		[OperationContract]
		LongActionResult AddLoanOptions(int userID, int customerID, NLLoanOptions loanOptions, int? oldLoanId);

		[OperationContract]
		LongActionResult AddLoan(NL_Model loanModel);

		[OperationContract]
		NL_Model AddPayment(NL_Model loanModel);

		[OperationContract]
		ReschedulingActionResult RescheduleLoan(int userID, int customerID, ReschedulingArgument reschedulingArgument);

		[OperationContract]
		NewLoanModelActionResult CalculateLoanSchedule(int? userID, int? customerID, NL_Model model);

    } // interface IEzServiceNewLoan

} // namespace EzService