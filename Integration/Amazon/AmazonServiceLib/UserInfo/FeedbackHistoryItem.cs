namespace EzBob.AmazonServiceLib.UserInfo
{
	public class FeedbackHistoryItem
	{
		public FeedbackHistoryItem( FeedbackType type, FeedbackPeriod period, int value )
		{
			Type = type;
			Period = period;
			Value = value;
		}

		public FeedbackType Type { get; private set; }
		public FeedbackPeriod Period { get; private set; }
		public int Value { get; private set; }

		public override string ToString()
		{
			return string.Format( "[ {0}, {1}] = {2}", Type, Period, Value );
		}
	}
}