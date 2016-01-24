using System;

namespace EzBobAcceptanceTests.Infra
{
    using Common.Logging;
    using EzBobCommon;
    using NServiceBus;

    public class FirstHandler : IHandleMessages<Object>, ISpecifyMessageHandlerOrdering {

        [Injected]
        public IBus Bus { get; set; }

        [Injected]
        public ILog Log { get; set; }

        public void Handle(object message) {
            Log.Debug("going to execute " + message.GetType()
                .Name);
        }

    
        public void SpecifyOrder(Order order) {
            order.SpecifyFirst<FirstHandler>();
        }
    }
}
