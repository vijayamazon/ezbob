namespace EzService.EzServiceImplementation {
    using System;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Backend.Strategies.NewLoan;

    public partial class EzServiceImplementation : IEzServiceAdmin, IEzService, IEzServiceNewLoan, IDisposable {
        public IntActionResult AddCashRequest(int userID, NL_CashRequests cashRequest) {
            AddCashRequest stra;
            var amd = ExecuteSync(out stra, cashRequest.CustomerID, userID, cashRequest);
            return new IntActionResult {
                MetaData = amd,
                Value = stra.CashRequestID
            };
        }

        public IntActionResult AddDecision(int userID, int customerID, NL_Decisions decision, long? oldCashRequest) {
            AddDecision stra;
            var amd = ExecuteSync(out stra, customerID, userID, decision, oldCashRequest);
            return new IntActionResult {
                MetaData = amd,
                Value = stra.DecisionID
            };
        }

        public IntActionResult AddOffer(int userID, int customerID, NL_Offers offer) {
            AddOffer stra;
            var amd = ExecuteSync(out stra, customerID, userID, offer);
            return new IntActionResult {
                MetaData = amd,
                Value = stra.OfferID
            };
        }

        public NL_Offers GetLastOffer(int userID, int customerID) {
            GetLastOffer stra;
            var amd = ExecuteSync(out stra, customerID, userID, customerID);
            return stra.Offer;
        }
    }
}
