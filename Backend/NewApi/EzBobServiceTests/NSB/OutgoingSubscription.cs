using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobServiceTests.NSB
{
    using NServiceBus;
    using NServiceBus.Pipeline;
    using NServiceBus.Pipeline.Contexts;
    using NServiceBus.Unicast.Messages;
    using NServiceBus.Unicast.Subscriptions;

    class OutgoingSubscription : IBehavior<OutgoingContext> {
        /// <summary>
        /// Called when the behavior is executed.
        /// </summary>
        /// <param name="context">The current context.</param><param name="next">The next <see cref="T:NServiceBus.Pipeline.IBehavior`1"/> in the chain to execute.</param>
        public void Invoke(OutgoingContext context, Action next) {
            var subscriptionMessageType = GetSubscriptionMessageTypeFrom(context.OutgoingLogicalMessage);
            if (EndpointSubscribed != null && subscriptionMessageType != null)
            {
                EndpointSubscribed(new SubscriptionEventArgs
                {
                    MessageType = subscriptionMessageType,
                    SubscriberReturnAddress = context.OutgoingMessage.ReplyToAddress
                });
            }

            next();
        }

        static string GetSubscriptionMessageTypeFrom(LogicalMessage msg)
        {
            foreach (var header in msg.Headers)
            {
                if (header.Key == Headers.SubscriptionMessageType)
                {
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
                : base("OutgoingSubscription", typeof(OutgoingSubscription), "So we can get subscription events")
            {
                InsertBefore(WellKnownStep.CreatePhysicalMessage);
            }
        }
    }
}
