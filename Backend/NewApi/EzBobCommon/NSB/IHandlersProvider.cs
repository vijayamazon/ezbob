namespace EzBobCommon.NSB
{
    using System;

    public interface IHandlersProvider {
        /// <summary>
        /// Gets the handler.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns></returns>
        object GetHandler(Type messageType);
    }
}
