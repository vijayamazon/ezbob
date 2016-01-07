namespace EzBobRest.Validators {
    using EzBobApi.Commands.Experian;
    using FluentValidation;

    /// <summary>
    /// Validates <see cref="ExperianBusinessTargetingCommand"/>
    /// </summary>
    public class CompanyTargetingValidator : AbstractValidator<ExperianBusinessTargetingCommand> {
        public CompanyTargetingValidator() {
            RuleFor(o => o.CustomerId)
                .NotEmpty()
                .WithMessage("customer id is mandatory")
                .DependentRules(d => d.RuleFor(c => c.CustomerId.ToLowerInvariant())
                    .NotEqual("{customerId}")
                    .WithMessage("customer id is mandatory"));

            When(o => o.IsLimited, () => {
                RuleFor(c => c.RegistrationNumber)
                    .NotEmpty()
                    .WithMessage("registration number is mandatory for limited company");
            });

            When(o => !o.IsLimited, () => {
                RuleFor(c => c.CompanyName)
                    .NotEmpty()
                    .WithMessage("company name is mandatory for not limited company");
            });
        }
    }
}
