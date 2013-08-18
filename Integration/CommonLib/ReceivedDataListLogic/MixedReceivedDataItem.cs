namespace EzBob.CommonLib.ReceivedDataListLogic
{
	public class MixedReceivedDataItem
	{
		public MixedReceivedDataItem(object data)
		{
			Data = data;
		}

		public object Data { get; private set; }
	}
}