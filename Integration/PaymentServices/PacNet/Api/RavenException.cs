using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.API
{
    /**
     * <p>A problem has occurred while interacting with the Raven API.</p>
     *
     * @author warren
     */
    [Serializable()]
    public class RavenException : System.Exception
    {
        /**
         * <p>Constructs a default instance of the receiver.</p>
         */
        public RavenException() : base() { }

        /**
         * <p>Constructs an initialized instance of the receiver.</p>
         *
         * @param message the explanation of the error condition
         */
        public RavenException(string message) : base(message) { }

        /**
         * <p>Constructs an initialized instance of the receiver.</p>
         *
         * @param originalException the root exception
         */
        public RavenException(System.Exception originalException) : base("", originalException) { }

        /**
         * <p>Constructs an initialized instance of the receiver.</p>
         *
         * @param message the explanation of the error condition
         * @param originalException the root exception
         */
        public RavenException(string message, System.Exception originalException) : base(message, originalException) { }

        // Constructor needed for serialization when exception propagates from a remoting server to the client. 
        protected RavenException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }

}
