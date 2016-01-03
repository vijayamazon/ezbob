namespace EzBobCommon.NSB {
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using log4net;
    using NServiceBus;
    using NServiceBus.Faults;

    /// <summary>
    /// Subscribes to NSB errors notifications and relays the error to appropriate handler
    /// </summary>
    public class ErrorsNotification : IWantToRunWhenBusStartsAndStops, IDisposable {
        private BusNotifications busNotifications;
        private List<IDisposable> unsubscribeStreams = new List<IDisposable>();
        private bool isDisposed;

        public ErrorsNotification(BusNotifications busNotifications) {
            this.busNotifications = busNotifications;
        }

        [Injected]
        public IBus Bus { get; set; }

        [Injected]
        public ILog Log { get; set; }

        [Injected]
        public IHandlersProvider HandlersProvider { get; set; }

        /// <summary>
        /// Method called at startup.
        /// </summary>
        public void Start() {
            CheckIfDisposed();

            this.unsubscribeStreams.Add(
                this.busNotifications.Errors.MessageSentToErrorQueue
                    // It is very important to handle streams on another thread
                    // otherwise the system performance can be impacted
                    .ObserveOn(Scheduler.Default) // Uses a pool-based scheduler
                    .Subscribe(OnFailure)
                );


            // You can also subscribe when messages fail FLR and/or SLR
            // - busNotifications.Errors.MessageHasFailedAFirstLevelRetryAttempt
            // - busNotifications.Errors.MessageHasBeenSentToSecondLevelRetries
        }

        /// <summary>
        /// Method called on shutdown.
        /// </summary>
        public void Stop() {
            CheckIfDisposed();

            foreach (IDisposable unsubscribeStream in this.unsubscribeStreams) {
                unsubscribeStream.Dispose();
            }
            this.unsubscribeStreams.Clear();
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            Stop();
            this.isDisposed = true;
        }

        /// <summary>
        /// Called when [failure].
        /// </summary>
        /// <param name="failedMessage">The failed message.</param>
        private void OnFailure(FailedMessage failedMessage) {
            var messageType = failedMessage.Headers[Headers.EnclosedMessageTypes];
            var handler = HandlersProvider.GetHandler(Type.GetType(messageType)) as IHandleErrors;
            if (handler != null) {
                handler.SendErrorResponse(failedMessage);
            } else {
                Log.Error("could not obtain handler for message type: " + messageType);
            }
        }

        /// <summary>
        /// Checks if disposed.
        /// </summary>
        /// <exception cref="System.Exception">Object has been disposed.</exception>
        private void CheckIfDisposed() {
            if (this.isDisposed) {
                throw new Exception("Object has been disposed.");
            }
        }
    }
}
