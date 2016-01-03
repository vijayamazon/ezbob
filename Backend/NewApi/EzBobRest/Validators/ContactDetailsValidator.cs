using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobRest.Validators {
    using EzBobApi.Commands.Customer.Sections;
    using EzBobCommon.Utils;
    using FluentValidation;

    /// <summary>
    /// Validates contact details
    /// </summary>
    public class ContactDetailsValidator : AbstractValidator<ContactDetailsInfo> {
        public ContactDetailsValidator() {
            RuleFor(o => o.EmailAddress)
                .EmailAddress()
                .WithMessage("[Contact Details]: Invalid email address")
                .When(o => StringUtils.IsNotEmpty(o.EmailAddress));
        }
    }
}
