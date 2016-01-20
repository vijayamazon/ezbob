namespace EzBobRest.Modules.Customer.Validators {
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
