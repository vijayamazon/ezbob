namespace EzBob.Backend.Strategies.ScoreCalculation
{
	using System.Collections.Generic;
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Utils;

	public abstract class MedalParameter
	{
		public decimal Value { get; set; }
		public int Grade { get; set; }
		public decimal Weight { get; set; }
		public decimal Score { get; set; }
		public bool IsWeightFixed { get; set; } // Parameters with "fixed" weight don't participate in some of the weight distribution

		public int MinGrade { get; set; }
		public int MaxGrade { get; set; }

		public virtual void CalculateWeight() { }
		public abstract void CalculateGrade();
		public void CalculateScore()
		{
			Score = Weight * Grade;
		}
	}

	public class BusinessScoreMedalParameter : MedalParameter
	{
		private readonly bool hasHmrc;
		private readonly bool firstRepaymentDatePassed;
		public BusinessScoreMedalParameter(int businessScore, bool hasHmrc, bool firstRepaymentDatePassed)
		{
			Value = businessScore;
			Weight = 30;
			this.hasHmrc = hasHmrc;
			this.firstRepaymentDatePassed = firstRepaymentDatePassed;
			IsWeightFixed = true;
			MinGrade = 0;
			MaxGrade = 9;
		}

		public override void CalculateWeight()
		{
			if (Value <= 30)
			{
				Weight = 41.25m;
			}

			if (!hasHmrc)
			{
				Weight += 5;
			}

			if (firstRepaymentDatePassed)
			{
				Weight -= 6.25m;
			}
		}

		public override void CalculateGrade()
		{
			if (Value < 11)
			{
				Grade = 0;
			}
			else if (Value < 21)
			{
				Grade = 1;
			}
			else if (Value < 31)
			{
				Grade = 2;
			}
			else if (Value < 41)
			{
				Grade = 3;
			}
			else if (Value < 51)
			{
				Grade = 4;
			}
			else if (Value < 61)
			{
				Grade = 5;
			}
			else if (Value < 71)
			{
				Grade = 6;
			}
			else if (Value < 81)
			{
				Grade = 7;
			}
			else if (Value < 91)
			{
				Grade = 8;
			}
			else
			{
				Grade = 9;
			}
		}
	}

	public class TangibleEquityMedalParameter : MedalParameter
	{
		public TangibleEquityMedalParameter(decimal tangibleEquity)
		{
			Value = tangibleEquity;
			Weight = 8;
			IsWeightFixed = false;
			MinGrade = 0;
			MaxGrade = 4;
		}

		public override void CalculateGrade()
		{
			if (Value < -0.05m)
			{
				Grade = 0;
			}
			else if (Value < 0)
			{
				Grade = 1;
			}
			else if (Value < 0.1m)
			{
				Grade = 2;
			}
			else if (Value < 0.3m)
			{
				Grade = 3;
			}
			else
			{
				Grade = 4;
			}
		}
	}

	public class BusinessSeniorityMedalParameter : MedalParameter
	{
		private readonly bool hasHmrc;
		private readonly bool firstRepaymentDatePassed;
		public BusinessSeniorityMedalParameter(DateTime? businessSeniority, bool hasHmrc, bool firstRepaymentDatePassed)
		{
			Value = CalculateBusinessSeniorityInFullYears(businessSeniority);
			Weight = 8;
			IsWeightFixed = true;
			this.hasHmrc = hasHmrc;
			this.firstRepaymentDatePassed = firstRepaymentDatePassed;
			MinGrade = 0;
			MaxGrade = 4;
		}

		private int CalculateBusinessSeniorityInFullYears(DateTime? businessSeniority)
		{
			int numOfMonths = 0;
			if (businessSeniority.HasValue)
			{
				DateTime now = DateTime.UtcNow;
				numOfMonths = now.Year - businessSeniority.Value.Year;
				if (now.Month < businessSeniority.Value.Month ||
				    (now.Month == businessSeniority.Value.Month && now.Day < businessSeniority.Value.Day))
				{
					numOfMonths--;
				}
				if (Value < 0)
				{
					numOfMonths = 0;
				}
			}

			return numOfMonths;
		}

		public override void CalculateWeight()
		{
			if (!hasHmrc)
			{
				Weight += 4;
			}

			if (firstRepaymentDatePassed)
			{
				Weight -= 1.7m;
			}
		}

		public override void CalculateGrade()
		{
			if (Value < 1)
			{
				Grade = 0;
			}
			else if (Value < 3)
			{
				Grade = 1;
			}
			else if (Value < 5)
			{
				Grade = 2;
			}
			else if (Value < 10)
			{
				Grade = 3;
			}
			else
			{
				Grade = 4;
			}
		}
	}

	public class ConsumerScoreMedalParameter : MedalParameter
	{
		private readonly bool hasHmrc;
		private readonly bool firstRepaymentDatePassed;
		public ConsumerScoreMedalParameter(int consumerScore, bool hasHmrc, bool firstRepaymentDatePassed)
		{
			Weight = 10;
			IsWeightFixed = true;
			Value = consumerScore;
			this.hasHmrc = hasHmrc;
			this.firstRepaymentDatePassed = firstRepaymentDatePassed;
			MinGrade = 0;
			MaxGrade = 8;
		}

		public override void CalculateWeight()
		{
			if (Value <= 800)
			{
				Weight = 13.75m;
			}

			if (!hasHmrc)
			{
				Weight += 3;
			}

			if (firstRepaymentDatePassed)
			{
				Weight -= 2.1m;
			}
		}

		public override void CalculateGrade()
		{
			if (Value < 481)
			{
				Grade = 0;
			}
			else if (Value < 561)
			{
				Grade = 1;
			}
			else if (Value < 641)
			{
				Grade = 2;
			}
			else if (Value < 721)
			{
				Grade = 3;
			}
			else if (Value < 801)
			{
				Grade = 4;
			}
			else if (Value < 881)
			{
				Grade = 5;
			}
			else if (Value < 961)
			{
				Grade = 6;
			}
			else if (Value < 1041)
			{
				Grade = 7;
			}
			else
			{
				Grade = 8;
			}
		}
	}

	public class MaritalStatusMedalParameter : MedalParameter
	{
		private readonly MaritalStatus maritalStatus;
		public MaritalStatusMedalParameter(MaritalStatus maritalStatus)
		{
			Weight = 5;
			IsWeightFixed = false;
			this.maritalStatus = maritalStatus;
			MinGrade = 0;
			MaxGrade = 4;
		}

		public override void CalculateGrade()
		{
			if (maritalStatus == MaritalStatus.Married || maritalStatus == MaritalStatus.Widowed)
			{
				Grade = 4;
			}
			else if (maritalStatus == MaritalStatus.Divorced)
			{
				Grade = 3;
			}
			else if (maritalStatus == MaritalStatus.Single)
			{
				Grade = 2;
			}
			else // LivingTogether, Separated, Other
			{
				Grade = 0;
			}
		}
	}

	public class FreeCashFlowMedalParameter : MedalParameter
	{
		private readonly bool hasHmrc;
		public FreeCashFlowMedalParameter(decimal freeCashFlow, bool hasHmrc)
		{
			Weight = 19;
			IsWeightFixed = true;
			Value = freeCashFlow;
			this.hasHmrc = hasHmrc;
			MinGrade = 0;
			MaxGrade = 6;
		}

		public override void CalculateWeight()
		{
			if (!hasHmrc)
			{
				Weight = 0;
			}
		}

		public override void CalculateGrade()
		{
			if (Value < -0.1m)
			{
				Grade = 0;
			}
			else if (Value < 0)
			{
				Grade = 1;
			}
			else if (Value < 0.1m)
			{
				Grade = 2;
			}
			else if (Value < 0.2m)
			{
				Grade = 3;
			}
			else if (Value < 0.3m)
			{
				Grade = 4;
			}
			else if (Value < 0.4m)
			{
				Grade = 5;
			}
			else
			{
				Grade = 6;
			}
		}
	}

	public class AnnualTurnoverMedalParameter : MedalParameter
	{
		private readonly bool hasHmrc;
		public AnnualTurnoverMedalParameter(decimal annualTurnover, bool hasHmrc)
		{
			Weight = 10;
			IsWeightFixed = true;
			Value = annualTurnover;
			this.hasHmrc = hasHmrc;
			MinGrade = 0;
			MaxGrade = 6;
		}

		public override void CalculateWeight()
		{
			if (!hasHmrc)
			{
				Weight += 7;
			}
		}

		public override void CalculateGrade()
		{
			if (Value < 30000)
			{
				Grade = 0;
			}
			else if (Value < 100000)
			{
				Grade = 1;
			}
			else if (Value < 200000)
			{
				Grade = 2;
			}
			else if (Value < 400000)
			{
				Grade = 3;
			}
			else if (Value < 800000)
			{
				Grade = 4;
			}
			else if (Value < 2000000)
			{
				Grade = 5;
			}
			else
			{
				Grade = 6;
			}
		}
	}

	public class NetWorthMedalParameter : MedalParameter
	{
		public NetWorthMedalParameter(decimal netWorth)
		{
			Weight = 10;
			IsWeightFixed = false;
			Value = netWorth;
			MinGrade = 0;
			MaxGrade = 3;
		}

		public override void CalculateGrade()
		{
			if (Value < 0.15m)
			{
				Grade = 0;
			}
			else if (Value < 0.5m)
			{
				Grade = 1;
			}
			else if (Value < 1)
			{
				Grade = 2;
			}
			else
			{
				Grade = 3;
			}
		}
	}

	public class EzbobSeniorityMedalParameter : MedalParameter
	{
		private readonly bool firstRepaymentDatePassed;
		public EzbobSeniorityMedalParameter(DateTime? ezbobSeniority, bool firstRepaymentDatePassed)
		{
			Weight = 0;
			IsWeightFixed = true;
			if (ezbobSeniority.HasValue)
			{
				int ezbobSeniorityMonthsOnly, ezbobSeniorityYearsOnly;
				MiscUtils.GetFullYearsAndMonths(ezbobSeniority.Value, out ezbobSeniorityMonthsOnly, out ezbobSeniorityYearsOnly);
				Value = ezbobSeniorityMonthsOnly + 12 * ezbobSeniorityYearsOnly;
			}
			this.firstRepaymentDatePassed = firstRepaymentDatePassed;
			MinGrade = 0;
			MaxGrade = 4;
		}

		public override void CalculateWeight()
		{
			if (firstRepaymentDatePassed)
			{
				Weight = 2;
			}
		}

		public override void CalculateGrade()
		{
			if (Value < 1)
			{
				Grade = 0;
			}
			else if (Value < 6)
			{
				Grade = 2;
			}
			else if (Value < 18)
			{
				Grade = 3;
			}
			else
			{
				Grade = 4;
			}
		}
	}

	public class OnTimeLoansMedalParameter : MedalParameter
	{
		private readonly bool firstRepaymentDatePassed;
		public OnTimeLoansMedalParameter(int numOfOnTimeLoans, bool firstRepaymentDatePassed)
		{
			Weight = 0;
			IsWeightFixed = true;
			Value = numOfOnTimeLoans;
			this.firstRepaymentDatePassed = firstRepaymentDatePassed;
			MinGrade = 1;
			MaxGrade = 4;
		}
		
		public override void CalculateWeight()
		{
			if (firstRepaymentDatePassed)
			{
				Weight = 3.33m;
			}
		}

		public override void CalculateGrade()
		{
			if (Value < 2)
			{
				Grade = 1;
			}
			else if (Value < 4)
			{
				Grade = 3;
			}
			else
			{
				Grade = 4;
			}
		}
	}

	public class LateRepaymentsMedalParameter : MedalParameter
	{
		private readonly bool firstRepaymentDatePassed;
		public LateRepaymentsMedalParameter(int numOfLateRepayments, bool firstRepaymentDatePassed)
		{
			Weight = 0;
			IsWeightFixed = true;
			Value = numOfLateRepayments;
			this.firstRepaymentDatePassed = firstRepaymentDatePassed;
			MinGrade = 0;
			MaxGrade = 5;
		}

		public override void CalculateWeight()
		{
			if (firstRepaymentDatePassed)
			{
				Weight = 2.67m;
			}
		}

		public override void CalculateGrade()
		{
			if (Value < 1)
			{
				Grade = 5;
			}
			else if (Value < 2)
			{
				Grade = 2;
			}
			else
			{
				Grade = 0;
			}
		}
	}

	public class EarlyRepaymentsMedalParameter : MedalParameter
	{
		private readonly bool firstRepaymentDatePassed;
		public EarlyRepaymentsMedalParameter(int numOfEarlyRepayments, bool firstRepaymentDatePassed)
		{
			Weight = 0;
			IsWeightFixed = true;
			Value = numOfEarlyRepayments;
			this.firstRepaymentDatePassed = firstRepaymentDatePassed;
			MinGrade = 2;
			MaxGrade = 5;
		}

		public override void CalculateWeight()
		{
			if (firstRepaymentDatePassed)
			{
				Weight = 2;
			}
		}

		public override void CalculateGrade()
		{
			if (Value < 1)
			{
				Grade = 2;
			}
			else if (Value < 3)
			{
				Grade = 3;
			}
			else
			{
				Grade = 5;
			}
		}
	}

	public class NewMedalScoreCalculator2
	{
		private readonly ASafeLog log;
		private readonly AConnection db;
		private readonly int customerId;
		private int businessScore;
		private decimal tangibleEquity;
		private DateTime? businessSeniority;
		private int consumerScore;
		private decimal freeCashFlow;
		private decimal annualTurnover;
		private decimal netWorth;
		private MaritalStatus maritalStatus;
		private DateTime? ezbobSeniority;
		private int numOfOnTimeLoans;
		private int numOfLateRepayments;
		private int numOfEarlyRepayments;
		private DateTime? firstRepaymentDate;
		private bool hasHmrc;
		private bool firstRepaymentDatePassed;

		public NewMedalScoreCalculator2(AConnection db, ASafeLog log, int customerId)
		{
			this.log = log;
			this.db = db;
			this.customerId = customerId;
		}

		public void CalculateMedalScore()
		{
			GatherData();

			var list = new List<MedalParameter>
				{
					new BusinessScoreMedalParameter(businessScore, hasHmrc, firstRepaymentDatePassed),
					new TangibleEquityMedalParameter(tangibleEquity),
					new BusinessSeniorityMedalParameter(businessSeniority, hasHmrc, firstRepaymentDatePassed),
					new ConsumerScoreMedalParameter(consumerScore, hasHmrc, firstRepaymentDatePassed),
					new MaritalStatusMedalParameter(maritalStatus),
					new FreeCashFlowMedalParameter(freeCashFlow, hasHmrc),
					new AnnualTurnoverMedalParameter(annualTurnover, hasHmrc),
					new NetWorthMedalParameter(netWorth),
					new EzbobSeniorityMedalParameter(ezbobSeniority, firstRepaymentDatePassed),
					new OnTimeLoansMedalParameter(numOfOnTimeLoans, firstRepaymentDatePassed),
					new LateRepaymentsMedalParameter(numOfLateRepayments, firstRepaymentDatePassed),
					new EarlyRepaymentsMedalParameter(numOfEarlyRepayments, firstRepaymentDatePassed)
				};

			foreach (MedalParameter medalParameter in list)
			{
				medalParameter.CalculateWeight();
			}

			AdjustSumOfWeights(list);

			decimal totalScore = 0;
			decimal totalMinScore = 0;
			decimal totalMaxScore = 0;
			foreach (MedalParameter medalParameter in list)
			{
				medalParameter.CalculateGrade();
				medalParameter.CalculateScore();
				totalScore += medalParameter.Score;
				totalMinScore += medalParameter.MinGrade * medalParameter.Weight;
				totalMaxScore += medalParameter.MaxGrade * medalParameter.Weight;
			}

			decimal normalizedTotalScore = (totalScore - totalMinScore) / (totalMaxScore - totalMinScore);
			
			// TODO: Create result as ScoreResult
		}

		private void AdjustSumOfWeights(List<MedalParameter> list)
		{
			decimal sumOfWeights = 0;
			decimal sumOfNonFixedWeights = 0;

			foreach (MedalParameter medalParameter in list)
			{
				sumOfWeights += medalParameter.Weight;
				if (!medalParameter.IsWeightFixed)
				{
					sumOfNonFixedWeights += medalParameter.Weight;
				}
			}

			decimal targetSumOfNonFixedWeights = sumOfNonFixedWeights + 100 - sumOfWeights;
			decimal ratioToGetToTarget = targetSumOfNonFixedWeights / sumOfNonFixedWeights;

			foreach (MedalParameter medalParameter in list)
			{
				if (!medalParameter.IsWeightFixed)
				{
					medalParameter.Weight *= ratioToGetToTarget;
				}
			}
		}

		private void GatherData()
		{
			DataTable dt = db.ExecuteReader("GetDataForMedalCalculation2", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));
			if (dt.Rows.Count != 1)
			{
				throw new Exception("Couldn't gather required data for the medal calculation");
			}
			
			var sr = new SafeReader(dt.Rows[0]);
			businessScore = sr[""];
			tangibleEquity = sr[""];
			businessSeniority = sr[""];
			consumerScore = sr[""];
			freeCashFlow = sr[""];
			annualTurnover = sr[""];
			netWorth = sr[""];
			string maritalStatusString = sr[""];
			maritalStatus = (MaritalStatus)Enum.Parse(typeof(MaritalStatus), maritalStatusString);
			ezbobSeniority = sr[""];
			numOfOnTimeLoans = sr[""];
			numOfLateRepayments = sr[""];
			numOfEarlyRepayments = sr[""];
			firstRepaymentDate = sr[""];

			hasHmrc = sr[""];
			firstRepaymentDatePassed = firstRepaymentDate != null && firstRepaymentDate < DateTime.UtcNow;
		}
	}
}
