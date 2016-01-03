namespace EzBobCommon.NSB {
    using NServiceBus.Faults;

    public interface IHandleErrors {
        /// <summary>
        /// USED BY ERROR NOTIFICATION MECHANISM, DON'T CALL THIS METHOD BY YOURSELF.
        /// Sends the error response.
        /// </summary>
        /// <param name="failedMessage">The failed message.</param>
        void SendErrorResponse(FailedMessage failedMessage);
    }
}
