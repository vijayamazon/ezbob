using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobPersistence.ThirdParty.Amazon
{
    using EzBobModels.Amazon;

    public interface IAmazonOrderDetailsQueries
    {
        /// <summary>
        /// Saves the order details.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <returns>
        /// order details filled with id and sellerSku
        /// </returns>
        IEnumerable<AmazonOrderItemDetail> SaveOrderDetails(IEnumerable<AmazonOrderItemDetail> details);
    }
}
