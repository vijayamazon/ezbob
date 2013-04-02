using System.Collections.Concurrent;
using EzBob.CommonLib;

namespace EZBob.DatabaseLib
{
	public interface IUpdateActionResultInfo
	{
		UpdateActionResultType Name { get; }
		object Value { get; }

		RequestsCounterData RequestsCounter { get; }
		ElapsedTimeInfo ElapsedTime { get;  }
	}

	public class UpdateActionResultInfo : IUpdateActionResultInfo
	{
		public UpdateActionResultType Name { get; set; }
		public object Value { get; set; }
		public RequestsCounterData RequestsCounter { get; set; }
		public ElapsedTimeInfo ElapsedTime { get; set; }
	}	

	public enum UpdateActionResultType
	{
		InventoryItemsCount,
		TransactionItemsCount,
		OrdersCount,
		FeedbackRaiting,
		FeedbackRatingStar,
		CurrentBalance,
		TeraPeakOrdersCount,
		eBayOrdersCount,
		GetTokenStatus
	}
}