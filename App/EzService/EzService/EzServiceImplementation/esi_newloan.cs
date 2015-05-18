namespace EzService.EzServiceImplementation {
    using System;
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Backend.Strategies;
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

        public IntActionResult AddDecision(int userID, int customerID, NL_Decisions decision, long? oldCashRequest, IEnumerable<NL_DecisionRejectReasons> decisionRejectReasons) {
            AddDecision stra;
            var amd = ExecuteSync(out stra, customerID, userID, decision, oldCashRequest, decisionRejectReasons);
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

		public DateTimeActionResult ExampleMethod(int userID, int customerID) {
			LoaderStrategy loaderStrategy;

			Action<Loader> action = loader => loader.ExampleMethod(customerID);

			ActionMetaData amd = ExecuteSync(out loaderStrategy, customerID, userID, action);

			return new DateTimeActionResult {
				MetaData = amd,
				Value = loaderStrategy.Model.SomeTime,
			};
		} // ExampleMethod
    }
}
