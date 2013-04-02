using System;

namespace EzBob.CommonLib.ReceivedDataListLogic
{
	public interface IReceivedDataList
	{
		DateTime SubmittedDate { get; }
		int Count { get; }
		bool HasData { get; }		
	}
}