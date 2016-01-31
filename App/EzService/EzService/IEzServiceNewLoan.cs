namespace EzService {
	using System;
	using System.Collections.Generic;
    using System.ServiceModel;
    using Ezbob.Backend.Models.NewLoan;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using EzService.ActionResults;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IEzServiceNewLoan {
        [OperationContract]
        NLLongActionResult AddCashRequest(int userID, NL_CashRequests cashRequest);

        [OperationContract]
        NLLongActionResult AddDecision(int userID, int customerID, NL_Decisions decision, long? oldCashRequest, IEnumerable<NL_DecisionRejectReasons> decisionRejectReasons);

        [OperationContract]
        NLLongActionResult AddOffer(int userID, int customerID, NL_Offers offer, List<NL_OfferFees> fees = null);

        [OperationContract]
        NL_Offers GetLastOffer(int userID, int customerID);

        [OperationContract]
        NLLongActionResult AddLoanLegals(int userID, int customerID, NL_LoanLegals loanLegals);

        [OperationContract]
		NLLongActionResult AddLoanOptions(int userID, int customerID, NL_LoanOptions loanOptions, int? oldLoanId, List<String> PropertiesUpdateList = null);

        [OperationContract]
        ActionMetaData AddLoan(int? userID, int? customerID, NL_Model model);

        [OperationContract]
        NLLongActionResult AddPayment(int customerID, NL_Payments payment, int userID);

        [OperationContract]
        ReschedulingActionResult RescheduleLoan(int userID, int customerID, ReschedulingArgument reschedulingArgument);

        [OperationContract]
        NLModelActionResult BuildLoanFromOffer(int? userID, int? customerID, NL_Model model);

		[OperationContract]
		NLLongActionResult DeactivateLoanInterestFreeze(int userID, int customerID, NL_LoanInterestFreeze loanInterestFreeze);

		[OperationContract]
		NLLongActionResult AddLoanInterestFreeze(int userID, int customerID, NL_LoanInterestFreeze loanInterestFreeze);

		[OperationContract]
        ListNewLoanActionResult GetCustomerLoans(int customerID, int userID);

	    [OperationContract]
	    NLModelActionResult GetLoanState(int customerID, long loanID, DateTime utcNow, int userID, bool getCalculatorState = true);

		[OperationContract]
		NLLongActionResult GetLoanByOldID(int oldId, int customerID = 1, int userID = 1);
		
		[OperationContract]
		NLLongActionResult CancelPayment(int customerID, NL_Payments payment, int userID);

		[OperationContract]
		StringActionResult SaveRollover(int userID, int customerID, NL_LoanRollovers rollover, long loanID);

		[OperationContract]
		StringActionResult AcceptRollover(int customerID, long loanID);

		[OperationContract]
		StringActionResult SaveFee(int userID, int customerID, NL_LoanFees fee);

		[OperationContract]
		StringActionResult CancelFee(int userID, int customerID, NL_LoanFees fee);

	} // interface IEzServiceNewLoan

} // namespace EzService