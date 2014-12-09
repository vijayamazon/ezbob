using System;
using EzBob.AmazonDbLib;
using EzBob.AmazonServiceLib.UserInfo;
using EzBob.CommonLib;
using EzBob.CommonLib.TimePeriodLogic;

namespace EzBob.AmazonLib
{
	internal static class AmazonDatabaseTypeConverter
	{

		/*public static TimePeriodEnum ConvertToDatabaseTimePeriad( FeedbackPeriod type )
		{
			switch ( type )
			{
				case FeedbackPeriod.Lifetime:
					return TimePeriodEnum.Lifetime;

				case FeedbackPeriod.Days30:
					return TimePeriodEnum.Month;

				case FeedbackPeriod.Days90:
					return TimePeriodEnum.Month3;

				case FeedbackPeriod.Days365:
					return TimePeriodEnum.Year;

				default:
					throw new NotImplementedException();
			}
		}

		public static AmazonDatabaseFunctionType ConvertToDatabaseFunctionType( FeedbackType type )
		{
			switch ( type )
			{
				case FeedbackType.Count:
					return AmazonDatabaseFunctionType.FeedbackCount;

				case FeedbackType.Negative:
					return AmazonDatabaseFunctionType.FeedbackNegative;

				case FeedbackType.Neutral:
					return AmazonDatabaseFunctionType.FeedbackNeutral;

				case FeedbackType.Positive:
					return AmazonDatabaseFunctionType.FeedbackPositive;

				default:
					throw new NotImplementedException();
			}
		}*/

		public static FeedbackPeriod ConvertToAmazonTimePeriod(TimePeriodEnum timePeriod)
		{
			switch (timePeriod)
			{
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
