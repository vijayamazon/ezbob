using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.API
{
    /**
     * <p>A Raven response has not been received directly as a result of sending
     * a Raven request.</p>
     *
     * <p>This does not necessarily mean that the request failed. There are many
     * non-Raven specific reasons why this might happen, including Internet network
     * failures. In the case of a submit, a RESPONSE request should be sent to Raven
     * referencing the request ID of the original submit request in order to
     * re-retrieve the response.</p>
     *
     * @author warren
     */
    [Serializable()]
    public class RavenNoResponseException : RavenException
    {
        /**
         * <p>Constructs a default instance of the receiver.</p>
         */
        public RavenNoResponseException() : base("inquire again") { }

        /**
         * <p>Constructs an initialized instance of the receiver.</p>
         *
         * @param originalException the root exception
         */
        public RavenNoResponseException(System.Exception originalException) : base("Raven server communication error.", originalException) { }

        // Constructor needed for serialization when exception propagates from a remoting server to the client. 
        protected RavenNoResponseException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}
