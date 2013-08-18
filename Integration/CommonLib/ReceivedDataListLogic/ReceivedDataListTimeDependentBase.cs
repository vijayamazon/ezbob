namespace EzBob.CommonLib.ReceivedDataListLogic
{
	using System;
	using System.Collections.Generic;

	public abstract class ReceivedDataListTimeDependentBase<T> : ReceivedDataListBase<T>, IReceivedDataListFactory<T>
		where T : class
	{
		protected ReceivedDataListTimeDependentBase(DateTime submittedDate, IEnumerable<T> collection = null) 
			: base(submittedDate, collection)
		{
		}

		public abstract ReceivedDataListTimeDependentBase<T> Create(DateTime submittedDate, IEnumerable<T> collection);
	}
}