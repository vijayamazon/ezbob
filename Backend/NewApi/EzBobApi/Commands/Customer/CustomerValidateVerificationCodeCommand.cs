using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobApi.Commands.Customer
{
    using EzBobCommon.NSB;

    /// <summary>
    /// Requests to validate verification code
    /// </summary>
    public class CustomerValidateVerificationCodeCommand : CommandBase
    {
        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>
        /// The customer identifier.
        /// </value>
        public string CustomerId { get; set; }
        /// <summary>
        /// Gets or sets the verification code.
        /// </summary>
        /// <value>
        /// The verification code.
        /// </value>
        public string VerificationCode { get; set; }
        /// <summary>
        /// Gets or sets the verification token.
        /// </summary>
        /// <value>
        /// The verification token.
        /// </value>
        public string VerificationToken { get; set; }
    }
}
