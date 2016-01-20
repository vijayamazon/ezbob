namespace EzBobRest.Modules.Marketplaces.PayPal.Validators {
    using EzBobApi.Commands.PayPal;
    using FluentValidation;

    /// <summary>
    /// Validates <see cref="PayPalGetPermissionsRedirectUrlCommand"/>.
    /// </summary>
    public class PayPalGetRedirectUrlValidator : ValidatorBase<PayPalGetPermissionsRedirectUrlCommand> {
        public PayPalGetRedirectUrlValidator() {
            RuleFor(c => c.Callback)
                .NotEmpty()
                .WithMessage("callback should not be empty");
        }
    }
}
