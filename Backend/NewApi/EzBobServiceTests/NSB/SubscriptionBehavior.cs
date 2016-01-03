using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobServiceTests.NSB
{
    using global::NServiceBus;
    using global::NServiceBus.Pipeline;
    using global::NServiceBus.Pipeline.Contexts;
    using global::NServiceBus.Unicast.Subscriptions;

    class SubscriptionBehavior : IBehavior<IncomingContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public SubscriptionBehavior() {
            int kk = 0;
        }

        public void Invoke(IncomingContext context, Action next)
        {

            var subscriptionMessageType = GetSubscriptionMessageTypeFrom(context.PhysicalMessage);
            if (EndpointSubscribed != null && subscriptionMessageType != null)
            {
                EndpointSubscribed(new SubscriptionEventArgs
                {
                    MessageType = subscriptionMessageType,
                    SubscriberReturnAddress = context.PhysicalMessage.ReplyToAddress
                });
            }

            next();
        }

        static string GetSubscriptionMessageTypeFrom(TransportMessage msg) {
            foreach (var header in msg.Headers) {
                if (header.Key == Headers.SubscriptionMessageType) {
                    string s = header.Value;
                    return s;
                }
            }
            return null;
        }

        public static Action<SubscriptionEventArgs> EndpointSubscribed;

        public static void OnEndpointSubscribed(Action<SubscriptionEventArgs> action)
        {
            EndpointSubscribed = action;
        }

        internal class Registration : RegisterStep
        {
            public Registration()
                : base("SubscriptionBehavior", typeof(SubscriptionBehavior), "So we can get subscription events")
            {
                InsertBefore(WellKnownStep.CreateChildContainer);
            }
        }
    }
}
