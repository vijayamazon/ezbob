using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.CommonLib.ReceivedDataListLogic;

namespace EZBob.DatabaseLib.Tests
{
	internal class ReceivedDataListTest : ReceivedDataListTimeMarketTimeDependentBase<TimeDependentDataTest>
	{
		public ReceivedDataListTest(DateTime submittedDate, IEnumerable<TimeDependentDataTest> collection = null) 
			: base(submittedDate, collection)
		{
		}

		public override ReceivedDataListTimeDependentBase<TimeDependentDataTest> Create( DateTime submittedDate, IEnumerable<TimeDependentDataTest> collection )
		{
			return new ReceivedDataListTest( submittedDate, collection );			
		}
		
	}
}