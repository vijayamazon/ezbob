namespace EzBobCommon.NSB {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;

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
    }
}
