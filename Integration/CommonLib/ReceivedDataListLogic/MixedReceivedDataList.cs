using System;
using System.Collections.Generic;

namespace EzBob.CommonLib.ReceivedDataListLogic
{
	public class MixedReceivedDataList : ReceivedDataListTimeDependentBase<MixedReceivedDataItem>
	{
		public MixedReceivedDataList(DateTime submittedDate, IEnumerable<MixedReceivedDataItem> collection = null) 
			: base(submittedDate, collection)
		{
		}

		public override ReceivedDataListTimeDependentBase<MixedReceivedDataItem> Create(DateTime submittedDate, IEnumerable<MixedReceivedDataItem> collection)
		{
			return new MixedReceivedDataList( submittedDate, collection );
		}
	}
}