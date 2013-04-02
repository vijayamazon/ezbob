using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.API
{
    /**
     * <p>A Raven API interaction was not properly authenticated.</p>
     *
     * @author warren
     */
    [Serializable()]
    public class RavenAuthenticationException : RavenException
    {
        /**
         * <p>Constructs an initialized instance of the receiver.</p>
         *
         * @param message the explanation of the error condition
         */
        public RavenAuthenticationException(string message) : base(message) { }

        // Constructor needed for serialization when exception propagates from a remoting server to the client. 
        protected RavenAuthenticationException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}
