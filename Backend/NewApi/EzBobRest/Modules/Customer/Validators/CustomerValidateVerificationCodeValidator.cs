namespace EzBobRest.Modules.Customer.Validators {
    using EzBobApi.Commands.Customer;
    using FluentValidation;

    /// <summary>
    /// Validates <see cref="CustomerValidateVerificationCodeCommand"/>
    /// </summary>
    public class CustomerValidateVerificationCodeValidator : AbstractValidator<CustomerValidateVerificationCodeCommand> {
        public CustomerValidateVerificationCodeValidator() {
            RuleFor(o => o.CustomerId)
                .NotEmpty()
                .WithMessage("empty customer id")
                .DependentRules(d => d.RuleFor(c => c.CustomerId.ToLowerInvariant())
                    .NotEqual("{customerid}")//when url parameter is not provided Nancy puts default string
                    .WithMessage("customer id is mandatory"));

            RuleFor(o => o.VerificationCode)
                .NotEmpty()
                .WithMessage("empty verification code");

            RuleFor(o => o.VerificationToken)
                .NotEmpty()
                .WithMessage("empty verification token")
                .DependentRules(d => d.RuleFor(c => c.VerificationToken.ToLowerInvariant())
                    .NotEqual("{verificationtoken}")//when url parameter is not provided Nancy puts default string
                    .WithMessage("verification token is mandatory"));
        }
    }
}
