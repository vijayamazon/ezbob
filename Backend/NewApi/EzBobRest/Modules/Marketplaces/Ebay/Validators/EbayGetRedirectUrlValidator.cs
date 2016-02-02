using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobRest.Modules.Marketplaces.Ebay.Validators
{
    using EzBobApi.Commands.Ebay;

    /// <summary>
    /// Validates <see cref="EbayGetLoginUrlCommand"/>
    /// </summary>
    public class EbayGetRedirectUrlValidator : ValidatorBase<EbayGetLoginUrlCommand>
    {
    }
}
