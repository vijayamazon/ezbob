using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobRest.Validators
{
    using EzBobApi.Commands.Customer;
    using FluentValidation;

    /// <summary>
    /// Validates <see cref="CustomerSendVerificationSmsCommand"/>
    /// </summary>
    public class CustomerSendVerificationSmsValidator : AbstractValidator<CustomerSendVerificationSmsCommand> {
        public CustomerSendVerificationSmsValidator() {
            RuleFor(o => o.PhoneNumber)
                .NotEmpty()
                .WithMessage("Invalid phone number");
        }
    }
}
