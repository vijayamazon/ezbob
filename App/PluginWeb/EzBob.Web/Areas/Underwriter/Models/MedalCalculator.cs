namespace EzBob.Web.Areas.Underwriter.Models
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;
	using EZBob.DatabaseLib.Model.Database;
	using System.Globalization;
	using EzBob.Web.Infrastructure;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;

	public class Score
	{
		public int Id { get; set; }
		public DateTime Date { get; set; }
		public string Medal { get; set; }
		public string MedalType { get; set; }
		public double Points { get; set; }
		public double Result { get; set; }
		public string Error { get; set; }
        
        public int? OfferedAmount { get; set; }
	    public int? MaxOfferedLoanAmount { get; set; }
	}

	public class MedalHistory
	{
		public IOrderedEnumerable<Score> MedalHistories { get; set; }
	}

	public class MedalDetailedHistory {
		public IOrderedEnumerable<MedalDetail> MedalDetailsHistories { get; set; }
	}

	public class MedalDetail {
		public Score Score { get; set; }
		public IList<MedalCharacteristic> MedalCharacteristics = new List<MedalCharacteristic>();
		public decimal TotalWeightUsed { get; set; }
		public decimal TotalACParameters { get; set; }
		public decimal TotalMaxPoss { get; set; }
		public decimal TotalPointsObtainedPercent { get; set; }
		public decimal TotalScore {get;set;}
		public decimal TotalGrade { get; set; }
	}

	public class MedalCharacteristic
	{
		public string CustomerCharacteristic { get; set; }
		public string Value { get; set; }
		public decimal Score { get; set; }
		public int Grade { get; set; } 
		public decimal WeightUsed { get; set; }
		public decimal ACParameters { get; set; }
		public decimal MaxPoss { get; set; }
		public decimal PointsObtainedPercent { get { return GetPointsObtainedPercent(); } }

		private decimal GetPointsObtainedPercent()
		{
			return MaxPoss == 0 ? 0 : Math.Round((ACParameters / MaxPoss * 100), 1);
		}
	}

	public class MedalCalculators
	{
		public Score Score = new Score();
		
		public MedalHistory History = new MedalHistory();

		public MedalDetailedHistory DetailedHistory = new MedalDetailedHistory();

		public MedalCalculators(EZBob.DatabaseLib.Model.Database.Customer customer) {
			var serviceClient = ObjectFactory.GetInstance<ServiceClient>();
			var newRepo = ObjectFactory.GetInstance<MedalCalculationsRepository>();
			var oldRepo = ObjectFactory.GetInstance<ScoringResultRepository>();
			var context = ObjectFactory.GetInstance<IEzbobWorkplaceContext>();
			var oldMedals = oldRepo.GetAllOldMedals(customer.Id).ToList();
			var newMedals = newRepo.GetAllNewMedals(customer.Id).ToList();

			if (oldMedals.Count == 0 && newMedals.Count == 0) return;

			var newMedalHistories = newMedals.Select(BuildNewScore).ToList();

			DateTime? firstNewMedalDate = null;
			if (newMedalHistories.Any())
			{
				firstNewMedalDate = newMedalHistories.Min(x => x.Date);
			}
			var oldMedalHistories = oldMedals.Where(x => firstNewMedalDate == null || x.ScoreDate < firstNewMedalDate).Select(BuildScore);

			newMedalHistories.AddRange(oldMedalHistories);
			this.History.MedalHistories = newMedalHistories.OrderBy(x => x.Date);
			
			var activeMedal = newMedals.FirstOrDefault(x => x.IsActive);
			if (activeMedal != null) {
				this.Score = BuildNewScore(activeMedal);
			}
			else if (oldMedals.Any()) {
			        var maxdate = oldMedals.Max(s => s.ScoreDate);
			        var scoringResult = oldMedals.FirstOrDefault(s => s.ScoreDate == maxdate);
			        this.Score = BuildScore(scoringResult);
			    
			}else {
			    this.Score = new Score();
			}

			var details = new MedalDetailedHistory();
			var oldMedalDetails = oldMedals.Select(BuildCharecteristic).ToList();
			var newMedalDetails = newMedals.Select(BuildNewCharecteristics).ToList();
			oldMedalDetails.AddRange(newMedalDetails);
			details.MedalDetailsHistories = oldMedalDetails.OrderByDescending(x => x.Score.Date);

			int i = 0;
			foreach (var medalDetail in details.MedalDetailsHistories) {
				medalDetail.Score.Id = i;
				i++;
			}

			this.DetailedHistory = details;
			
			this.LogicalGlue = serviceClient.Instance.LogicalGlueGetLastInference(context.UserId, customer.Id, DateTime.UtcNow, false);
			this.LogicalGlueHistory = serviceClient.Instance.LogicalGlueGetHistory(context.UserId, customer.Id);
		}

		public LogicalGlueResult LogicalGlue { get; set; }
		public IList<LogicalGlueResult> LogicalGlueHistory { get; set; }

		private Score BuildScore(ScoringResult scoringResult)
		{
			return new Score
			{
				MedalType = "Old Medal",
				Medal = scoringResult.Medal,
				Points = scoringResult.ScorePoints,
				Result = scoringResult.ScoreResult,
				Date = scoringResult.ScoreDate
			};
		}

		private Score BuildNewScore(MedalCalculations medalCalculation)
		{
			return new Score
			{
				MedalType = medalCalculation.MedalType,
				Medal = medalCalculation.Medal,
				Points = (double)medalCalculation.TotalScore,
				Result = (double)medalCalculation.TotalScoreNormalized,
				Date = medalCalculation.CalculationTime,
				Error = medalCalculation.Error,
                OfferedAmount = medalCalculation.OfferedLoanAmount,
                MaxOfferedLoanAmount = medalCalculation.MaxOfferedLoanAmount
			};
		}

		private MedalDetail BuildCharecteristic(ScoringResult scoringResult) {
			var medalDetail = new MedalDetail();
			string[] splitACDescription = scoringResult.ACDescription.Split(';');
			string[] splitWeight = scoringResult.Weights.Split(';');
			string[] splitACParameters = scoringResult.ACParameters.Split(';');
			string[] splitMaxPoss = scoringResult.MAXPossiblePoints.Split(';');

			const string pattern = @"([0-9]*\.[0-9]+|[0-9]+)";
			var regex = new Regex(pattern);

			medalDetail.MedalCharacteristics = new List<MedalCharacteristic>();
			for (var i = 0; i <= splitACDescription.Length - 1; i++)
			{
				medalDetail.MedalCharacteristics.Add(new MedalCharacteristic
				{
					CustomerCharacteristic = splitACDescription[i],
					WeightUsed = Convert.ToDecimal(splitWeight[i], CultureInfo.InvariantCulture),
					ACParameters = Convert.ToDecimal(regex.Match(splitACParameters[i]).ToString(), CultureInfo.InvariantCulture),
					MaxPoss = Convert.ToDecimal(splitMaxPoss[i], CultureInfo.InvariantCulture)
				});
			}

			medalDetail.Score = BuildScore(scoringResult);
			GetTotal(medalDetail);

			return medalDetail;
		}
		
		private void GetTotal(MedalDetail detail)
		{
			detail.TotalACParameters = detail.MedalCharacteristics.Sum(m => m.ACParameters);
			detail.TotalMaxPoss = detail.MedalCharacteristics.Sum(m => m.MaxPoss);
			detail.TotalWeightUsed = detail.MedalCharacteristics.Sum(m => m.WeightUsed);
			detail.TotalPointsObtainedPercent = detail.TotalMaxPoss == 0 ? 0 : Math.Round((detail.TotalACParameters / detail.TotalMaxPoss * 100), 1);
			detail.TotalGrade = detail.MedalCharacteristics.Sum(x => x.Grade);
			detail.TotalScore = detail.MedalCharacteristics.Sum(x => x.Score);
		}

		private MedalDetail BuildNewCharecteristics(MedalCalculations medal)
		{
			var detail = new MedalDetail {
				Score = BuildNewScore(medal),
				MedalCharacteristics = new List<MedalCharacteristic>()
			};

			DateTime? businessSeniority = medal.BusinessSeniority;
			string businessSeniorityStr = businessSeniority.HasValue ? businessSeniority.Value.ToString("yyyy-MM-dd") : null;

			DateTime? regDate = medal.EzbobSeniority;
			string ezbobSeniorityStr = regDate.HasValue ? regDate.Value.ToString("yyyy-MM-dd") : null;

			detail.MedalCharacteristics.Add(new MedalCharacteristic { CustomerCharacteristic = "Business Score",          Value = medal.BusinessScore.ToString("N0"),        WeightUsed = medal.BusinessScoreWeight,        Score = medal.BusinessScoreScore,        Grade = (int)medal.BusinessScoreGrade });
			detail.MedalCharacteristics.Add(new MedalCharacteristic { CustomerCharacteristic = "Free Cash Flow",          Value = medal.FreeCashFlow.ToString("N2"),         WeightUsed = medal.FreeCashFlowWeight,         Score = medal.FreeCashFlowScore,         Grade = (int)medal.FreeCashFlowGrade });
			detail.MedalCharacteristics.Add(new MedalCharacteristic { CustomerCharacteristic = "Annual Turnover",         Value = medal.AnnualTurnover.ToString("N2"),       WeightUsed = medal.AnnualTurnoverWeight,       Score = medal.AnnualTurnoverScore,       Grade = (int)medal.AnnualTurnoverGrade });
			detail.MedalCharacteristics.Add(new MedalCharacteristic { CustomerCharacteristic = "Tangible Equity",         Value = medal.TangibleEquity.ToString("N2"),       WeightUsed = medal.TangibleEquityWeight,       Score = medal.TangibleEquityScore,       Grade = (int)medal.TangibleEquityGrade });
			detail.MedalCharacteristics.Add(new MedalCharacteristic { CustomerCharacteristic = "Business Seniority",      Value = businessSeniorityStr,                      WeightUsed = medal.BusinessSeniorityWeight,    Score = medal.BusinessSeniorityScore,    Grade = (int)medal.BusinessSeniorityGrade });
			detail.MedalCharacteristics.Add(new MedalCharacteristic { CustomerCharacteristic = "Consumer Score",          Value = medal.ConsumerScore.ToString("N0"),        WeightUsed = medal.ConsumerScoreWeight,        Score = medal.ConsumerScoreScore,        Grade = (int)medal.ConsumerScoreGrade });
			detail.MedalCharacteristics.Add(new MedalCharacteristic { CustomerCharacteristic = "Net Worth",               Value = medal.NetWorth.ToString("N2"),             WeightUsed = medal.NetWorthWeight,             Score = medal.NetWorthScore,             Grade = (int)medal.NetWorthGrade });
			detail.MedalCharacteristics.Add(new MedalCharacteristic { CustomerCharacteristic = "Marital Status",          Value = medal.MaritalStatus,                       WeightUsed = medal.MaritalStatusWeight,        Score = medal.MaritalStatusScore,        Grade = (int)medal.MaritalStatusGrade });
			detail.MedalCharacteristics.Add(new MedalCharacteristic { CustomerCharacteristic = "Num Of Loans",            Value = medal.NumOfLoans.ToString("N0"),           WeightUsed = medal.NumOfLoansWeight,           Score = medal.NumOfLoansScore,           Grade = (int)medal.NumOfLoansGrade });
			detail.MedalCharacteristics.Add(new MedalCharacteristic { CustomerCharacteristic = "Num Of Early Repayments", Value = medal.NumOfEarlyRepayments.ToString("N0"), WeightUsed = medal.NumOfEarlyRepaymentsWeight, Score = medal.NumOfEarlyRepaymentsScore, Grade = (int)medal.NumOfEarlyRepaymentsGrade });
			detail.MedalCharacteristics.Add(new MedalCharacteristic { CustomerCharacteristic = "Num Of Late Repayments",  Value = medal.NumOfLateRepayments.ToString("N0"),  WeightUsed = medal.NumOfLateRepaymentsWeight,  Score = medal.NumOfLateRepaymentsScore,  Grade = (int)medal.NumOfLateRepaymentsGrade });
			detail.MedalCharacteristics.Add(new MedalCharacteristic { CustomerCharacteristic = "Num Of Stores",           Value = medal.NumberOfStores.ToString("N0"),       WeightUsed = medal.NumberOfStoresWeight,       Score = medal.NumberOfStoresScore,       Grade = (int)medal.NumberOfStoresGrade });
			detail.MedalCharacteristics.Add(new MedalCharacteristic { CustomerCharacteristic = "Positive Feedbacks",      Value = medal.PositiveFeedbacks.ToString("N0"),    WeightUsed = medal.PositiveFeedbacksWeight,    Score = medal.PositiveFeedbacksScore,    Grade = (int)medal.PositiveFeedbacksGrade });
			detail.MedalCharacteristics.Add(new MedalCharacteristic { CustomerCharacteristic = "Ezbob Seniority",         Value = ezbobSeniorityStr,                         WeightUsed = medal.EzbobSeniorityWeight,       Score = medal.EzbobSeniorityScore,       Grade = (int)medal.EzbobSeniorityGrade });

			GetTotal(detail);

			return detail;
		}
	}
}