namespace EzBob.AmazonLib {
	using System;
	using EzBob.AmazonServiceLib.UserInfo;
	using EzBob.CommonLib.TimePeriodLogic;

	internal static class AmazonDatabaseTypeConverter {
		public static FeedbackPeriod ConvertToAmazonTimePeriod(TimePeriodEnum timePeriod) {
			switch (timePeriod) {
			case TimePeriodEnum.Lifetime:
				return FeedbackPeriod.Lifetime;

			case TimePeriodEnum.Month:
				return FeedbackPeriod.Days30;

			case TimePeriodEnum.Month3:
				return FeedbackPeriod.Days90;

			case TimePeriodEnum.Year:
				return FeedbackPeriod.Days365;

			default:
				throw new NotImplementedException();
			}
		}
	}
}
