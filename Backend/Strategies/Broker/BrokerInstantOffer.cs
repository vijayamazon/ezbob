namespace EzBob.Backend.Strategies.Broker {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerInstantOffer : AStrategy {
		#region public

		#region constructor

		public BrokerInstantOffer(BrokerInstantOfferRequest request, AConnection oDB, ASafeLog oLog)
			: base(oDB, oLog) {
			_request = request;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "BrokerInstantOffer"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			SaveRequest();
			CalculateInstantOffer();
			SaveResponse();
		}

		#endregion method Execute

		public BrokerInstantOfferResponse Response { get; private set; }

		#endregion public

		#region private

		private void SaveRequest() {
			_requestId = DB.ExecuteScalar<int>(
					"SaveBrokerInstantOfferRequest",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<BrokerInstantOfferRequest>("Tbl", new List<BrokerInstantOfferRequest> { _request }));
		}

		private void CalculateInstantOffer() {
			//TODO do some magic calculation here
			//TODO experian company check (cache/new)
			var rand = new Random(_requestId);
			Response = new BrokerInstantOfferResponse {
				BrokerInstantOfferRequestId = _requestId,
				ApprovedSum = rand.Next(1, 50) * 1000,
				InterestRate = (decimal)rand.NextDouble()/10,
				RepaymentPeriod = rand.Next(3, 13),
				UseSetupFee = rand.Next(0,2) > 0,
				UseBrokerSetupFee = rand.Next(0, 2) > 0,
				LoanSourceId = rand.Next(1, 3),
				LoanTypeId = rand.Next(1, 3)
			};

			if (Response.LoanSourceId == 2) {
				Response.InterestRate = 0.02M;
			}
		}

		private void SaveResponse() {
			var responseId = DB.ExecuteScalar<int>(
					"SaveBrokerInstantOfferResponse",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<BrokerInstantOfferResponse>("Tbl", new List<BrokerInstantOfferResponse> { Response }));

			Response.Id = responseId;
		}

		private readonly BrokerInstantOfferRequest _request;
		private int _requestId;

		#endregion private
	}
} // namespace
