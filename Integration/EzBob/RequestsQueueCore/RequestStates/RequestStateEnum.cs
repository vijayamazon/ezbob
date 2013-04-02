namespace EzBob.RequestsQueueCore.RequestStates
{
	public enum RequestStateEnum
	{
		New,
		InQueue,
		Processing,
		
		Success,
		Canceled,
		Error,
		NotFound
	}
}