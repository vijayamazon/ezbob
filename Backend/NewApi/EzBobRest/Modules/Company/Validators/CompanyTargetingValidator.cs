namespace EzBobRest.Modules.Company.Validators {
    using EzBobApi.Commands.Experian;
    using FluentValidation;

    /// <summary>
    /// Validates <see cref="ExperianBusinessTargetingCommand"/>
    /// </summary>
    public class CompanyTargetingValidator : ValidatorBase<ExperianBusinessTargetingCommand> {
        public CompanyTargetingValidator() {
           
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
