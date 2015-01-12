namespace Ezbob.Backend.Strategies.Broker {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using MedalCalculations;

	public class BrokerInstantOffer : AStrategy {
		public override string Name {
			get { return "BrokerInstantOffer"; }
		}

		public BrokerInstantOfferResponse Response { get; private set; }

		public BrokerInstantOffer(BrokerInstantOfferRequest request) {
			_request = request;
		} // constructor

		// Name

		public override void Execute() {
			SaveRequest();
			CalculateInstantOffer();
			SaveResponse();
		}
		public decimal GetBasicInterestRate(int experianScore) {
			SafeReader sr = DB.GetFirst("GetConfigTableValue", CommandSpecies.StoredProcedure, new QueryParameter("ConfigTableName", "BasicInterestRate"), new QueryParameter("Key", experianScore));
			return sr["Value"];
		}

		private void SaveRequest() {
			_requestId = DB.ExecuteScalar<int>(
					"SaveBrokerInstantOfferRequest",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<BrokerInstantOfferRequest>("Tbl", new List<BrokerInstantOfferRequest> { _request }));
		}

		private void CalculateInstantOffer() {
			//TODO retrieve experian company data (cache/new)
			//int businessScore = 60; //todo
			//decimal tangibleEquity = 20000M; //todo
			DateTime businessSeniority = new DateTime(1990, 1, 1);

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

			//var netWorth = 0;
			//if (_request.IsHomeOwner) {
			//	netWorth = 100000; //todo get the net worth
			//}

			var rand = new Random(_requestId);
			Response = new BrokerInstantOfferResponse {
				BrokerInstantOfferRequestId = _requestId,
				ApprovedSum = CalcAndCapOffer((int)Math.Round((int)(1/* TODO: put MedalClassification here */ * _request.AnnualTurnover * GetLoanOfferMultiplier(consumerScore) / 100) / 100d, 0, MidpointRounding.AwayFromZero) * 100),
				InterestRate = GetBasicInterestRate(consumerScore),
				RepaymentPeriod = 12,
				UseSetupFee = rand.Next(0, 2) > 0,
				UseBrokerSetupFee = true,
				LoanSourceId = rand.Next(1, 3), // todo make use calculator to decide weather to give EU or reg loan
				LoanTypeId = 1 //default loan type regular (not halfway) ?
			};

			if (Response.LoanSourceId == 2) {
				Response.InterestRate = 0.02M;
			}
		}
		private decimal GetLoanOfferMultiplier(int experianScore) {
			SafeReader sr = DB.GetFirst("GetConfigTableValue", CommandSpecies.StoredProcedure, new QueryParameter("ConfigTableName", "LoanOfferMultiplier"), new QueryParameter("Key", experianScore));
			return sr["Value"];
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

	}
} // namespace
