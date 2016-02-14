using System;
using System.Threading.Tasks;

namespace EzBobCommon.NSB {
    using System.Threading;
    using Common.Logging;
    using NServiceBus;

    /// <summary>
    /// Implements asynchronous sender.
    /// Instances of this class are created automatically by NSB
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public class SendReceiveAsyncHandler<TResponse> : IHandleMessages<TResponse>
        where TResponse : CommandResponseBase {

        private static int TimeOutMilis = 10*60*1000;

        [Injected]
        public IBus Bus { get; set; }

        [Injected]
        public SendReceiveCache Cache { get; set; }

        [Injected]
        public ILog Log { get; set; }


        /// <summary>
        /// Sends the command asynchronously, and returns task to wait for
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="cmd">The command.</param>
        /// <param name="cancellationTokenSrc">The cancellation token source.<br/>
        /// if not specified enables automatic task cancellation after 10 min 
        /// </param>
        /// <returns></returns>
        public Task<TResponse> SendAsync(string address, CommandBase cmd, CancellationTokenSource cancellationTokenSrc = null) {
            if (cmd.MessageId == default(Guid)) {
                cmd.MessageId = Guid.NewGuid();
            }

            if (cancellationTokenSrc == null) {
                cancellationTokenSrc = new CancellationTokenSource();
                cancellationTokenSrc.CancelAfter(TimeOutMilis);
            }

            TaskCompletionSource<TResponse> taskSrc = new TaskCompletionSource<TResponse>();
            //registers action to cancel task on timeout
            cancellationTokenSrc.Token.Register(() => taskSrc.TrySetCanceled(), useSynchronizationContext: false);

            Cache.Set(cmd.MessageId, taskSrc);

            Bus.Send(address, cmd);

            return AddCacheClearOnExceptionStage(taskSrc.Task, cmd.MessageId);
        }

        /// <summary>
        /// Handles the specified response.
        /// </summary>
        /// <param name="response">The response.</param>
        public void Handle(TResponse response) {
            var optional = Cache.Remove(response.MessageId);
            if (optional.HasValue) {
                var taskSrc = optional.Value as TaskCompletionSource<TResponse>;
                if (taskSrc != null) {
                    taskSrc.SetResult(response);
                } else {
                    Log.Error("got unknown type in cache");
                }
            } else {
                Log.Warn("could not get task source for response:" + response.MessageId);
            }
        }

        /// <summary>
        /// Wraps original task to clear cache if it's canceled
        /// </summary>
        /// <param name="originalTask">The original task.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <returns></returns>
        private Task<TResponse> AddCacheClearOnExceptionStage(Task<TResponse> originalTask, Guid messageId) {
            return Task.Run(async () => {
                try {
                    return await originalTask;
                } catch (TaskCanceledException) {
                    try {
                        Cache.Remove(messageId);
                    } catch (Exception ex) {
                        Log.Error("error on clear cache for messageId: " + messageId, ex);
                    }
                    Log.Error("Canceled task for messageId: " + messageId);
                    throw; //re-throws the TaskCanceledException exception
                }
            });
        }
    }
}