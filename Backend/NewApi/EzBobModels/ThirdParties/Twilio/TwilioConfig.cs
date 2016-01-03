using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.ThirdParties.Twilio {
    /// <summary>
    /// Represents Twilio configuration
    /// </summary>
    public class TwilioConfig {
        /// <summary>
        /// Gets or sets the twilio account sid.
        /// </summary>
        /// <value>
        /// The twilio account sid.
        /// </value>
        public string TwilioAccountSid { get; set; }
        /// <summary>
        /// Gets or sets the twilio authentication token.
        /// </summary>
        /// <value>
        /// The twilio authentication token.
        /// </value>
        public string TwilioAuthToken { get; set; }
        /// <summary>
        /// Gets or sets the twilio sending number.
        /// </summary>
        /// <value>
        /// The twilio sending number.
        /// </value>
        public string TwilioSendingNumber { get; set; }
        /// <summary>
        /// Gets or sets the maximum per number.
        /// </summary>
        /// <value>
        /// The maximum per number.
        /// </value>
        public string MaxPerNumber { get; set; }
        /// <summary>
        /// Gets or sets the maximum per day.
        /// </summary>
        /// <value>
        /// The maximum per day.
        /// </value>
        public string MaxPerDay { get; set; }
        /// <summary>
        /// Gets or sets the skip code generation number.
        /// </summary>
        /// <value>
        /// The skip code generation number.
        /// </value>
        public string SkipCodeGenerationNumber { get; set; }
        /// <summary>
        /// Gets or sets the skip code generation number code.
        /// </summary>
        /// <value>
        /// The skip code generation number code.
        /// </value>
        public string SkipCodeGenerationNumberCode { get; set; }
    }
}
