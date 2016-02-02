using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.Amazon
{
    using EzBobCommon.NSB;
    using EzBobModels.Amazon;

    /// <summary>
    /// Response to <see cref="AmazonGetProductCategories3dPartyCommand"/>
    /// </summary>
    public class AmazonGetProductCategories3dPartyCommandResponse : CommandResponseBase {
        public IDictionary<string, IEnumerable<AmazonProductCategory>> CategoriesBySku;
    }
}
