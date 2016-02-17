namespace EzBobCommon.NSB {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Security.Policy;
    using NServiceBus;

    /// <summary>
    /// Base class for commands
    /// </summary>
    public abstract class CommandBase {
       
        private IDictionary<string, object> headers;


        /// <summary>
        /// Dictionary to store what ever you want.
        /// </summary>
        /// <value>
        /// Dictionary to store what ever you want
        /// </value>
        public IDictionary<string, object> EzBobHeaders
        {
            get
            {
                if (this.headers == null) {
                    this.headers = new Dictionary<string, object>();
                }

                return this.headers;
            }
            set { this.headers = value; }
        }

        [DefaultValue(false)]
        public bool IsFailed { get; set; }

        public Guid MessageId { get; set; }

        /// <summary>
        /// Exists to support async handlers. <see cref="AsyncHandlerSupport"/>
        /// </summary>
        /// <value>
        /// The reply to address.
        /// </value>
        public Address ReplyToAddress { get; set; }

        /// <summary>
        /// Gets or sets the command originator.
        /// </summary>
        /// <value>
        /// The request originator.
        /// </value>
        public string CommandOriginator { get; set; }

        /// <summary>
        /// Gets or sets the command originator ip.
        /// </summary>
        /// <value>
        /// The command originator ip.
        /// </value>
        public string CommandOriginatorIP { get; set; }
        public Url CommandOriginatorUrl { get; set; }
    }
}
