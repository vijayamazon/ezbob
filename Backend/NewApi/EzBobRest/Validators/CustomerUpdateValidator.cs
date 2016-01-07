using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobRest.Validators {
    using EzBobApi.Commands.Customer;
    using FluentValidation;

    /// <summary>
    /// Validates <see cref="CustomerUpdateCommand"/>
    /// </summary>
    public class CustomerUpdateValidator : AbstractValidator<CustomerUpdateCommand> {
        public CustomerUpdateValidator() {
            RuleFor(o => o.CustomerId)
                .NotEmpty()
                .WithMessage("empty customer id")
                .DependentRules(d => d.RuleFor(c => c.CustomerId.ToLowerInvariant())
                    .NotEqual("{customerid}") //when url parameter is not provided Nancy puts default string
                    .WithMessage("customer id is mandatory"));
        }
    }
}
