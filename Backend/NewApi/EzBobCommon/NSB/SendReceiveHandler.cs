namespace EzBobCommon.NSB {
    using System;
    using Common.Logging;
    using NServiceBus;

    /// <summary>
    /// Imitates 'synchronous' NSB call.<br/>
    /// Used for NSB full duplex calls.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    [Obsolete("instead use SendReceiveAsyncHandler")]
    public class SendReceiveHandler<TResponse> : IHandleMessages<TResponse>
        where TResponse : CommandResponseBase {

        [Injected]
        public IBus Bus { get; set; }

        [Injected]
        public SendReceiveCache Cache { get; set; }

        [Injected]
        public ILog Log { get; set; }

        /// <summary>
        /// Sends the command and blocks calling method until response is received.
        /// </summary>
        /// <param name="address">The address to send command to</param>
        /// <param name="cmd">The command to send</param>
        /// <returns></returns>
        public TResponse SendAndBlockUntilReceive(string address, CommandBase cmd) {
            if (cmd.MessageId == default(Guid)) {
                cmd.MessageId = Guid.NewGuid();
            }

            var locker = new Locker<TResponse>();
            Cache.Set(cmd.MessageId, locker);
            Bus.Send(address, cmd);
            return locker.Lock();
        }


        /// <summary>
        /// Handles the specified response.
        /// </summary>
        /// <param name="response">The response.</param>
        public void Handle(TResponse response) {
            var optional = Cache.Remove(response.MessageId);
            if (optional.HasValue) {
                var locker = optional.Value as Locker<TResponse>;
                if (locker != null) {
                    locker.Unlock(response);
                } else {
                    Log.Error("got wrong type locker");
                }
            } else {
                Log.Warn("could not get locker for response:" + response.MessageId);
            }
        }
    }
}
