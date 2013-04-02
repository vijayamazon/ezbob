using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace EzBob.AmazonServiceLib.UserInfo
{
	public class FeedbackHistoryInfo : IEnumerable<FeedbackHistoryItem>
	{
		private readonly ConcurrentBag<FeedbackHistoryItem> _Items = new ConcurrentBag<FeedbackHistoryItem>();

		public void Add( FeedbackType feedbackType, FeedbackPeriod feedbackPeriod, int value )
		{
			Add( new FeedbackHistoryItem( feedbackType, feedbackPeriod, value ) );
		}

		private void Add( FeedbackHistoryItem item )
		{
			_Items.Add( item );
		}

		public IEnumerator<FeedbackHistoryItem> GetEnumerator()
		{
			return _Items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int? GetFeedbackHistoryValue( FeedbackPeriod amazonTimePeriod, FeedbackType feedbackType )
		{
			var item = _Items.FirstOrDefault( i => i.Period == amazonTimePeriod && i.Type == feedbackType );

			return item == null ? (int?)null : item.Value;
		}
	}
}