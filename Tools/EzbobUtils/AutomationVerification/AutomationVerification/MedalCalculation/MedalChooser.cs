namespace AutomationCalculator.MedalCalculation
{
	using System;
	using System.Linq;
	using Common;
	using Ezbob.Logger;

	public class MedalChooser
	{
		protected ASafeLog Log;
		public MedalChooser(ASafeLog log)
		{
			Log = log;
		}

		public MedalOutputModel GetMedal(int customerId, DateTime? calculationDate = null)
		{
			var dbHelper = new DbHelper(Log);
			var medalChooserData = dbHelper.GetMedalChooserData(customerId);
			DateTime today = calculationDate.HasValue ? calculationDate.Value : DateTime.Today;

			if (medalChooserData.LastBankHmrcUpdateDate.HasValue &&
				(today - medalChooserData.LastBankHmrcUpdateDate.Value).TotalDays > medalChooserData.MedalDaysOfMpRelevancy)
			{
				return new MedalOutputModel
				{
					MedalType = MedalType.NoMedal,
					Medal = Medal.NoClassification,
					Error = "Bank or Hmrc data is too old",
					NumOfHmrcMps = medalChooserData.NumOfHmrc,
					CustomerId = customerId,
				};
			}

			if (medalChooserData.NumOfHmrc > 1)
			{
				return new MedalOutputModel
				{
					MedalType = MedalType.NoMedal,
					Medal = Medal.NoClassification,
					Error = string.Format("Customer has {0} HMRC MPs", medalChooserData.NumOfHmrc),
					NumOfHmrcMps = medalChooserData.NumOfHmrc,
					CustomerId = customerId,
				};
			}

			var type = MedalType.NoMedal;

			if (medalChooserData.IsLimited)
			{
				type = medalChooserData.HasOnline ? MedalType.OnlineLimited : MedalType.Limited;
			}
			else if (medalChooserData.HasCompanyScore)
			{
				type = medalChooserData.HasOnline ? MedalType.OnlineNonLimitedWithBusinessScore : MedalType.NonLimited;
			}
			else if (medalChooserData.HasOnline)
			{
				type = MedalType.OnlineNonLimitedNoBusinessScore;
			}

			if (type == MedalType.NoMedal && medalChooserData.HasPersonalScore && (medalChooserData.HasBank || medalChooserData.HasHmrc))
			{
				type = MedalType.SoleTrader;
			}

			IMedalCalulator medalCalulator;
			switch (type)
			{
				case MedalType.Limited:
					medalCalulator = new OfflineLImitedMedalCalculator(Log);
					break;
				case MedalType.NonLimited:
					medalCalulator = new NonLimitedMedalCalculator(Log);
					break;
				case MedalType.OnlineLimited:
					medalCalulator = new OnlineLImitedMedalCalculator(Log);
					break;
				case MedalType.SoleTrader:
					medalCalulator = new SoleTraderMedalCalculator(Log);
					break;
				case MedalType.OnlineNonLimitedNoBusinessScore:
					medalCalulator = new OnlineNonLimitedNoBusinessScoreMedalCalculator(Log);
					break;
				case MedalType.OnlineNonLimitedWithBusinessScore:
					medalCalulator = new OnlineNonLimitedWithBusinessScoreMedalCalculator(Log);
					break;
				default:
					return new MedalOutputModel
					{
						MedalType = type,
						Medal = Medal.NoClassification,
						Error = "None of the medals match the criteria for medal calculation",
						NumOfHmrcMps = medalChooserData.NumOfHmrc,
						CustomerId = customerId
					};
			}

			var data = medalCalulator.GetInputParameters(customerId, calculationDate);
			var medal = medalCalulator.CalculateMedal(data);
			medal.OfferedLoanAmount = GetOfferedAmount(medal, medalChooserData.MinApprovalAmount);
			return medal;
		}

		private int GetOfferedAmount(MedalOutputModel medal, int minApprovalAmount)
		{
			var dbHelper = new DbHelper(Log);
			var medalCoefficients = dbHelper.GetMedalCoefficients();
			var coefficients = medalCoefficients.First(x => x.Medal == medal.Medal);
			var annualTurnoverOffer = decimal.Parse(medal.Dict[Parameter.AnnualTurnover].Value) * coefficients.AnnualTurnover;
			var freeCashflowOffer = medal.UseHmrc ? medal.FreeCashflow * coefficients.FreeCashFlow : 0;
			var valueAddedOffer = medal.UseHmrc ? medal.ValueAdded * coefficients.ValueAdded : 0;

			var offers = new [] { annualTurnoverOffer, freeCashflowOffer, valueAddedOffer };
			var positiveOffers = offers.Where(x => x >= minApprovalAmount).ToList();
			return positiveOffers.Any() ? positiveOffers.Min(x => (int)x) : 0;
		}
	}
}
