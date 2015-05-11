namespace EzService {
	
	using System.ServiceModel;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

    [ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceNewLoan {
		[OperationContract]
		IntActionResult AddCashRequest(int userID, NL_CashRequests cashRequest);

        [OperationContract]
        IntActionResult AddDecision(int userID, int customerID, NL_Decisions decision, long? oldCashRequest);

        [OperationContract]
        IntActionResult AddOffer(int userID, int customerID, NL_Offers offer);

        [OperationContract]
        NL_Offers GetLastOffer(int userID, int customerID);
    } // interface IEzServiceNewLoan
} // namespace EzService
