using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobRest.Modules.PostCode.Validators
{
    using EzBobApi.Commands.SimplyPostcode;
    using FluentValidation;

    /// <summary>
    /// Validates <see cref="SimplyPostcodeGetAddressDetailsCommand"/>.
    /// </summary>
    public class GetAddressDetailsValidator : ValidatorBase<SimplyPostcodeGetAddressDetailsCommand> {
        public GetAddressDetailsValidator() {
            RuleFor(o => o.AddressId)
                .NotEmpty()
                .WithMessage("empty address id");
        }
    }
}
