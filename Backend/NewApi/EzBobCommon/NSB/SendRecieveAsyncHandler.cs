using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobCommon.NSB {
    using log4net;
    using NServiceBus;

    /// <summary>
    /// Implements asynchronous sender
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public class SendRecieveAsyncHandler<TResponse> : IHandleMessages<TResponse>
        where TResponse : CommandResponseBase {

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
        /// <returns></returns>
        public Task<TResponse> SendAsync(string address, CommandBase cmd) {
            if (cmd.MessageId == default(Guid)) {
                cmd.MessageId = Guid.NewGuid();
            }

            TaskCompletionSource<TResponse> taskSrc = new TaskCompletionSource<TResponse>();
            Cache.Set(cmd.MessageId, taskSrc);
            Bus.Send(address, cmd);
            return taskSrc.Task;
        }

        /// <summary>
        /// Handles the specified response.
        /// </summary>
        /// <param name="response">The response.</param>
        public void Handle(TResponse response) {
            var optional = Cache.Remove(response.MessageId);
            if (optional.HasValue) {
                var taskSrc = optional.GetValue() as TaskCompletionSource<TResponse>;
                if (taskSrc != null) {
                    taskSrc.SetResult(response);
                } else {
                    Log.Error("got unknown type in cache");
                }
            } else {
                Log.Warn("could not get task source for response:" + response.MessageId);
            }
        }
    }
}
