using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.API
{
    /**
     * <p>An interaction with the Raven API cannot be signed for authenticity because a subset of the interaction's signature data
     * is incomplete.</p>
     *
     * @author warren
     */
    [Serializable()]
    class RavenIncompleteSignatureException : RavenException
    {
        /**
         * <p>Constructs an initialized instance of the receiver.</p>
         *
         * @param missingConfigParamName the name of the missing configuration parameter
         */
        public RavenIncompleteSignatureException(string missingConfigParamName) : base("Raven signature property " + missingConfigParamName + " not set.") { }
    }
}
