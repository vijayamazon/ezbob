using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.API
{
    /**
     * <p>A requested Raven operation is not defined in the targeted version of
     * the API (as specified by the request parameter "RAPIVersion").</p>
     *
     * @author warren
     */
    [Serializable()]
    public class RavenNoSuchOperationException : RavenException
    {
        /**
         * <p>Constructs an initialized instance of the receiver.</p>
         *
         * @param operationName the name of the requested operation (see
         * <code>RavenOperationType</code> for valid values
         */
        public RavenNoSuchOperationException(string operationName) : base(operationName + " is an unsupported operation.") { }

        // Constructor needed for serialization when exception propagates from a remoting server to the client. 
        protected RavenNoSuchOperationException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}
