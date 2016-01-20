namespace EzBobRest.Modules.Customer.Validators
{
    using EzBobApi.Commands.Customer;
    using FluentValidation;

    /// <summary>
    /// Validates <see cref="CustomerSendVerificationSmsCommand"/>
    /// </summary>
    public class CustomerSendVerificationSmsValidator : AbstractValidator<CustomerSendVerificationSmsCommand> {
        public CustomerSendVerificationSmsValidator() {
            RuleFor(o => o.CustomerId)
               .NotEmpty()
               .WithMessage("empty customer id")
               .DependentRules(d => d.RuleFor(c => c.CustomerId.ToLowerInvariant())
                   .NotEqual("{customerid}")//when url parameter is not provided Nancy puts default string
                   .WithMessage("customer id is mandatory"));
            RuleFor(o => o.PhoneNumber)
                .NotEmpty()
                .WithMessage("Invalid phone number");
        }
    }
}
