using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobCommon.NSB {
    using NServiceBus;
    using NServiceBus.MessageMutator;

    public class AsyncHandlerSupport : IMutateIncomingMessages {
        [Injected]
        public IBus Bus { get; set; }

        /// <summary>
        ///When using async handler and there is a need to send a reply, the <see cref="NServiceBus.IBus.CurrentMessageContext"/> is null.
        /// (It seems like it's existing only for initial thread on which 'Handle(Command)' is called).
        /// To overcome this limitation this incoming messages mutator stores the reply address in <see cref="CommandBase"/> and this is used by <see cref="HandlerBase{T}.SendReply"/>
        /// </summary>
        public object MutateIncoming(object message) {
            CommandBase command = message as CommandBase;
            if (command != null) {
                command.ReplyToAddress = Bus.CurrentMessageContext.ReplyToAddress;
            }
            return message;
        }
    }
}
