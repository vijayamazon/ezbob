using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Twilio
{
    /// <summary>
    /// Twilio configuration class.
    /// Properties are filled by configuration manager at runtime. 
    /// </summary>
    public class TwilioConfig
    {
        /// <summary>
        /// Gets or sets the twilio account sid.
        /// </summary>
        /// <value>
        /// The twilio account sid.
        /// </value>
        public string TwilioAccountSid { get; private set; }
        /// <summary>
        /// Gets or sets the twilio authentication token.
        /// </summary>
        /// <value>
        /// The twilio authentication token.
        /// </value>
        public string TwilioAuthToken { get; private set; }
        /// <summary>
        /// Gets or sets the twilio sending number.
        /// </summary>
        /// <value>
        /// The twilio sending number.
        /// </value>
        public string TwilioSendingNumber { get; private set; }
        
    }
}
