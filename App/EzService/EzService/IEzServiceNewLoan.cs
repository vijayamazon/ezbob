namespace EzService {
    using System.Collections.Generic;
    using System.ServiceModel;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

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
		DateTimeActionResult ExampleMethod(int userID, int customerID);
        [OperationContract]
        IntActionResult AddLoanLegals(int userID, int customerID, NL_LoanLegals loanLegals);

		[OperationContract]
		ActionMetaData ExampleOtherMethod();
    } // interface IEzServiceNewLoan
} // namespace EzService
