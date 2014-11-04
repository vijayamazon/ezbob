namespace EzBob.Backend.Strategies.Broker {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using Ezbob.Logger;
	using MedalCalculations;

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
			//TODO retrieve experian company data (cache/new)
			int businessScore = 60; //todo
			decimal tangibleEquity = 20000M; //todo
			DateTime businessSeniority = new DateTime(1990,1,1);

			int consumerScore = 0;
			switch (_request.MainApplicantCreditScore) {
				case "ok":
					consumerScore = 700;//todo real number
					break;
				case "excellent":
					consumerScore = 1000;//todo real number
					break;
				case "low":
					consumerScore = 500;//todo real number
					break;
			}

			var netWorth = 0;
			if (_request.IsHomeOwner) {
				netWorth = 100000; //todo get the net worth
			}
			var calculator = new LimitedMedalCalculator1(DB, Log);
			var medalScore = calculator.CalculateMedalScore(new ScoreResult(businessScore, _request.AnnualProfit, _request.AnnualTurnover,
			                                               tangibleEquity, businessSeniority, consumerScore, netWorth), 0, DateTime.UtcNow);
			
			var rand = new Random(_requestId);
			Response = new BrokerInstantOfferResponse {
				BrokerInstantOfferRequestId = _requestId,
				ApprovedSum = CalcAndCapOffer(calculator.GetOfferedCreditLine(medalScore.Medal, _request.AnnualTurnover, consumerScore)),
				InterestRate = calculator.GetBasicInterestRate(consumerScore),
				RepaymentPeriod = 12,
				UseSetupFee = rand.Next(0,2) > 0,
				UseBrokerSetupFee = true,
				LoanSourceId = rand.Next(1, 3), // todo make use calculator to decide weather to give EU or reg loan
				LoanTypeId = 1 //default loan type regular (not halfway) ?
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

		private int CalcAndCapOffer(int offeredCreditLine) {

			if (_request.IsHomeOwner && ConfigManager.CurrentValues.Instance.MaxCapHomeOwner < offeredCreditLine) {
				return ConfigManager.CurrentValues.Instance.MaxCapHomeOwner;
			}

			if (!_request.IsHomeOwner && ConfigManager.CurrentValues.Instance.MaxCapNotHomeOwner < offeredCreditLine) {
				return ConfigManager.CurrentValues.Instance.MaxCapNotHomeOwner;
			}
			return offeredCreditLine;
		}

		private readonly BrokerInstantOfferRequest _request;
		private int _requestId;

		#endregion private
	}
} // namespace
