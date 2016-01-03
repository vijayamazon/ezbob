using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.PayPal
{
    public class PayPalUserPersonalInfo
    {
        public int Id { get; set; }
        public int CustomerMarketPlaceId { get; set; }
        public DateTime Created { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EMail { get; set; }
        public string FullName { get; set; }
        public string BusinessName { get; set; }
        public string Country { get; set; }
        /// <summary>
        /// This actually PayerId (because of table name it will remain PlayerId)
        /// </summary>
        /// <value>
        /// The payer identifier.
        /// </value>
        public string PlayerId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Postcode { get; set; }
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Phone { get; set; }
    }
}
