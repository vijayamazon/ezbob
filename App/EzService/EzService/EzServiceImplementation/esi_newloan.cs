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

        public IntActionResult AddDecision(int userID, int customerID, NL_Decisions decision) {
            AddCashRequest stra;
            var amd = ExecuteSync(out stra, customerID, userID, decision);
            return new IntActionResult {
                MetaData = amd,
                Value = stra.CashRequestID
            };
        }

        public IntActionResult AddOffer(int userID, int customerID, NL_Offers offer) {
            AddCashRequest stra;
            var amd = ExecuteSync(out stra, customerID, userID, offer);
            return new IntActionResult {
                MetaData = amd,
                Value = stra.CashRequestID
            };
        }
    }
}
