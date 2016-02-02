namespace EzBobCommon.NSB {
    using System;
    using NServiceBus;
    using StructureMap;

    /// <summary>
    /// Provides NSB handlers, based on message type
    /// </summary>
    public class HandlersProvider : IHandlersProvider {
        private IContainer container;

        public HandlersProvider(IContainer container) {
            this.container = container;
        }

        /// <summary>
        /// Gets the handler.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns></returns>
        public object GetHandler(Type messageType) {
            var handlerGenericType = typeof(IHandleMessages<>).MakeGenericType(messageType);
            return this.container.GetInstance(handlerGenericType);
        }
    }
}
