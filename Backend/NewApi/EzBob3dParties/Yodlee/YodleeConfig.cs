using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Yodlee
{
    public class YodleeConfig
    {
        /// <summary>
        /// Gets the rest URL.
        /// The value is set automatically from configuration.
        /// </summary>
        /// <value>
        /// The rest URL.
        /// </value>
        public string RestUrl { get; private set; }
        /// <summary>
        /// Gets the node URL.
        /// </summary>
        /// <value>
        /// The node URL.
        /// The value is set automatically from configuration.
        /// </value>
        public string NodeUrl { get; private set; }
    }
}
