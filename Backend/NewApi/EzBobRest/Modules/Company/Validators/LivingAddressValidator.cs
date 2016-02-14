namespace EzBobRest.Modules.Company.Validators {
    using EzBobApi.Commands.Customer.Sections;
    using FluentValidation;

    /// <summary>
    /// Validates CurrentLivingAddress.
    /// </summary>
    internal abstract class LivingAddressValidator : AbstractValidator<LivingAddressInfo> {
        protected LivingAddressValidator(string prefix, int hosingTypeBottomLimit = 1) {
            RuleFor(o => (int)o.HousingType)
                .Must(h => h >= hosingTypeBottomLimit && h <= 4)
                .WithMessage(prefix + "Invalid housing type.");
            RuleFor(o => o.Line1)
                .NotEmpty()
                .WithMessage(prefix + "Line 1 could not be empty.");
            RuleFor(o => o.Postcode)
                .NotEmpty()
                .WithMessage(prefix + "postcode could not be empty");
        }
    }
}
