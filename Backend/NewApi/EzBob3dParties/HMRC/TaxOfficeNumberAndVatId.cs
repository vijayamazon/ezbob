using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.HMRC
{
    public class TaxOfficeNumberAndVatId
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaxOfficeNumberAndVatId"/> class.
        /// </summary>
        /// <param name="taxOfficeNumber">The tax office number.</param>
        /// <param name="vatId">The vat identifier.</param>
        public TaxOfficeNumberAndVatId(string taxOfficeNumber, string vatId) {
            TaxOfficeNumber = taxOfficeNumber;
            VatId = vatId;
        }

        /// <summary>
        /// Gets the tax office number.
        /// </summary>
        /// <value>
        /// The tax office number.
        /// </value>
        public string TaxOfficeNumber { get; private set; }
        /// <summary>
        /// Gets the vat identifier.
        /// </summary>
        /// <value>
        /// The vat identifier.
        /// </value>
        public string VatId { get; private set; }
    }
}
