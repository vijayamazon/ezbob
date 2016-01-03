using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobService.Mobile
{
    public class MobilePhoneConfig {
        ///<summary>
        /// Gets or sets the skip code generation number.
        /// </summary>
        /// <value>
        /// The skip code generation number.
        /// </value>
        public string SkipCodeGenerationNumber { get; private set; }

        /// <summary>
        /// Gets or sets the skip code generation number code.
        /// </summary>
        /// <value>
        /// The skip code generation number code.
        /// </value>
        public string SkipCodeGenerationNumberCode { get; private set; }
        /// <summary>
        /// Gets or sets the maximum per number.
        /// </summary>
        /// <value>
        /// The maximum per number.
        /// </value>
        public int MaxPerNumber { get; private set; }

        /// <summary>
        /// Gets or sets the maximum per day.
        /// </summary>
        /// <value>
        /// The maximum per day.
        /// </value>
        public int MaxPerDay { get; private set; }
    }
}
