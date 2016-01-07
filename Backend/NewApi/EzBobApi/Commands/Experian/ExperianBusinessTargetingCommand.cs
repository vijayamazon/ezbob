using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobApi.Commands.Experian
{
    using EzBobCommon.NSB;

    public class ExperianBusinessTargetingCommand : CommandBase
    {
        /// <summary>
        /// Not needed for targeting itself, but may be we will use it for requests tracking
        /// </summary>
        /// <value>
        /// The customer identifier.
        /// </value>
        public string CustomerId { get; set; }
        public string CompanyName { get; set; }
        public string PostCode { get; set; }
        public bool IsLimited { get; set; }
        public string RegistrationNumber { get; set; }
    }
}
