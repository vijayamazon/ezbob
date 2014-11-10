namespace AutomationCalculator.MedalCalculation
{
	using Common;
	using Ezbob.Logger;

	public class MedalChooser
	{
		protected ASafeLog Log;
		public MedalChooser(ASafeLog log)
		{
			Log = log;
		}

		public MedalOutputModel GetMedal(int customerId) {
			var dbHelper = new DbHelper(Log);
			var medalChooserData = dbHelper.GetMedalChooserData(customerId);

			var type = MedalType.NoMedal;
			if (medalChooserData.IsLimited) {
				if (medalChooserData.HasOnline) {
					type = MedalType.OnlineLimited;
				}
				else if (medalChooserData.NumOfHmrc < 2) {
					type = MedalType.Limited;
				}
			}
			else {
				if (medalChooserData.HasCompanyScore && medalChooserData.NumOfHmrc < 2) {
					type = MedalType.NonLimited;
				}
			}

			if (type == MedalType.NoMedal && medalChooserData.HasPersonalScore && medalChooserData.NumOfHmrc < 2 &&
			    (medalChooserData.HasBank || medalChooserData.HasHmrc)) {
				type = MedalType.SoleTrader;
			}


			IMedalCalulator medalCalulator;
			switch (type) {
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
				default:
					return new MedalOutputModel {
						MedalType = type,
						Medal = Medal.NoMedal,
						Error = "None of the medals match the criteria for medal calculation",
						NumOfHmrcMps = medalChooserData.NumOfHmrc,
						CustomerId = customerId
					};
			}

			var data = medalCalulator.GetInputParameters(customerId);
			return medalCalulator.CalculateMedal(data);
		}
	}
}
