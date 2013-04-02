namespace EzBob.RequestsQueueCore.RequestStates
{
	public interface IRequestState
	{
		RequestStateEnum State { get; }		
		RequestErorrInfo ErorrInfo { get; }
		bool InProgress();
		bool HasError();
		bool IsDone();
		bool IsCanceled();
		bool IsSuccess();
		bool IsNotFound();
	}
}