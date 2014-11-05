namespace AutomationCalculator.MedalCalculation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Common;
	using Ezbob.Logger;

	public class SoleTraderMedalCalculator : MedalCalculator
	{
		public SoleTraderMedalCalculator(ASafeLog log) : base(log) { }

		public override MedalInputModel GetInputParameters(int customerId)
		{
			throw new NotImplementedException();
			//TODO retrieve the data!!!!

			var dbHelper = new DbHelper(Log);
			var dbData = dbHelper.GetMedalInputModel(customerId);
			var model = new MedalInputModel();
			if (dbData.HasMoreThanOneHmrc)
			{
				model.HasMoreThanOneHmrc = dbData.HasMoreThanOneHmrc;
				return model;
			}

			var yodlees = dbHelper.GetCustomerYodlees(customerId);
			var mpHelper = new MarketPlacesHelper(Log);
			var yodleeIncome = mpHelper.GetYodleeAnnualized(yodlees, Log);
			var today = DateTime.Today;
			const int year = 365;

			model.HasHmrc = dbData.HasHmrc;
			model.BusinessScore = dbData.BusinessScore;
			model.BusinessSeniority = (decimal)(today - dbData.IncorporationDate).TotalDays / year;
			model.ConsumerScore = dbData.ConsumerScore;
			model.EzbobSeniority = ((today.Year - dbData.RegistrationDate.Year) * 12) + today.Month - dbData.RegistrationDate.Month;
			model.FirstRepaymentDatePassed = dbData.FirstRepaymentDate.HasValue && dbData.FirstRepaymentDate.Value < today;
			model.IsLimited = dbData.TypeOfBusiness == "Limited" || dbData.TypeOfBusiness == "LLP";
			model.NumOfEarlyPayments = dbData.NumOfEarlyPayments;
			model.NumOfLatePayments = dbData.NumOfLatePayments;
			model.NumOfOnTimeLoans = dbData.NumOfOnTimeLoans;
			model.MaritalStatus = (MaritalStatus)Enum.Parse(typeof(MaritalStatus), dbData.MaritalStatus);
			model.AnnualTurnover = dbData.HasHmrc ? dbData.HmrcRevenues : yodleeIncome;
			model.AnnualTurnover = model.AnnualTurnover < 0 ? 0 : model.AnnualTurnover;

			var balance = dbData.CurrentBalanceSum < 0 ? 0 : (dbData.CurrentBalanceSum / dbData.FCFFactor);
			model.FreeCashFlow = model.AnnualTurnover == 0 ? 0 : (dbData.HmrcEbida - balance) / model.AnnualTurnover;
			model.TangibleEquity = model.AnnualTurnover == 0 ? 0 : dbData.TangibleEquity / model.AnnualTurnover;
			model.NetWorth = dbData.ZooplaValue == 0 ? 0 : (dbData.ZooplaValue - dbData.Mortages) / (decimal)dbData.ZooplaValue;

			return model;
		}

		public override MedalOutputModel CalculateMedal(MedalInputModel model)
		{
			Log.Debug(model.ToString());

			var dict = new Dictionary<Parameter, Weight>
				{
					
					{Parameter.BusinessSeniority,        GetBusinessSeniorityWeight(model.BusinessSeniority, model.FirstRepaymentDatePassed, model.HasHmrc)},
					{Parameter.ConsumerScore,            GetConsumerScoreWeight(model.ConsumerScore, model.FirstRepaymentDatePassed, model.HasHmrc)},
					{Parameter.EzbobSeniority,           GetEzbobSeniorityWeight(model.EzbobSeniority, model.FirstRepaymentDatePassed)},
					{Parameter.MaritalStatus,            GetMaritalStatusWeight(model.MaritalStatus)},
					{Parameter.NumOfOnTimeLoans,         GetNumOfOnTimeLoansWeight(model.NumOfOnTimeLoans, model.FirstRepaymentDatePassed)},
					{Parameter.NumOfLatePayments,        GetNumOfLatePaymentsWeight(model.NumOfLatePayments, model.FirstRepaymentDatePassed)},
					{Parameter.NumOfEarlyPayments,       GetNumOfEarlyPaymentsWeight(model.NumOfEarlyPayments, model.FirstRepaymentDatePassed)},
					{Parameter.AnnualTurnover,           GetAnnualTurnoverWeight(model.AnnualTurnover, model.HasHmrc)},
					{Parameter.FreeCashFlow,             GetFreeCashFlowWeight(model.FreeCashFlow, model.HasHmrc)},
					{Parameter.NetWorth,                 GetNetWorthWeight(model.NetWorth)},
				};

			CalcDelta(model, dict);

			MedalOutputModel scoreMedal = CalcScoreMedalOffer(dict);

			return scoreMedal;
		}

		protected override void CalcDelta(MedalInputModel model, Dictionary<Parameter, Weight> dict)
		{

			if (model.ConsumerScore <= LowConsumerScore)
			{
				//Sum of all weights
				var sow = dict.Sum(x => x.Value.FinalWeight);
				//Sum of weights of , NetWorth, MaritalStatus
				var sonf = dict[Parameter.NetWorth].FinalWeight + dict[Parameter.MaritalStatus].FinalWeight;

				var sonfDesired = sonf - sow + 1;
				var ratio = sonfDesired / sonf;
				dict[Parameter.NetWorth].FinalWeight *= ratio;
				dict[Parameter.MaritalStatus].FinalWeight *= ratio;
			}
		}

		#region WeightConstants
		#region Base Weight
		public override decimal BusinessScoreBaseWeight { get { return 0; } } //not in use
		public override decimal FreeCashFlowBaseWeight { get { return 0.25M; } }
		public override decimal AnnualTurnoverBaseWeight { get { return 0.16M; } }
		public override decimal BusinessSeniorityBaseWeight { get { return 0.03M; } }
		public override decimal ConsumerScoreBaseWeight { get { return 0.40M; } }
		public override decimal NetWorthBaseWeight { get { return 0.10M; } }
		public override decimal MaritalStatusBaseWeight { get { return 0.06M; } }
		#endregion

		#region No HMRC Weight
		public override decimal FreeCashFlowNoHmrcWeight { get { return 0; } }

		public override decimal AnnualTurnoverNoHmrcWeightChange { get { return 0.10M; } }
		public override decimal BusinessScoreNoHmrcWeightChange { get { return 0; } } //not in use
		public override decimal ConsumerScoreNoHmrcWeightChange { get { return 0.13M; } }
		public override decimal BusinessSeniorityNoHmrcWeightChange { get { return 0.02M; } }
		#endregion

		#region Low Score Weight
		public override decimal BusinessScoreLowScoreWeight { get { return 0; } } //not in use
		public override decimal ConsumerScoreLowScoreWeight { get { return 0.55M; } }
		#endregion

		#region First Repayment Passed Weight
		public override decimal EzbobSeniorityFirstRepaymentWeight { get { return 0.02M; } }
		public override decimal NumOfOnTimeLoansFirstRepaymentWeight { get { return 0.0333M; } }
		public override decimal NumOfLateRepaymentsFirstRepaymentWeight { get { return 0.0267M; } }
		public override decimal NumOfEarlyRepaymentsFirstRepaymentWeight { get { return 0.02M; } }

		public override decimal ConsumerScoreFirstRepaymentWeightChange { get { return -0.09M; } }
		public override decimal BusinessScoreFirstRepaymentWeightChange { get { return 0; } } //not in use
		public override decimal BusinessSeniorityFirstRepaymentWeightChange { get { return -0.01M; } }
		#endregion
		#endregion
	}
}
