namespace EzBobRest.Modules.Marketplaces.PayPal.Validators
{
    using EzBobApi.Commands.PayPal;
    using FluentValidation;

    public class PayPalRegisterCustomerValidator : ValidatorBase<PayPalRegisterCustomerCommand> {
        public PayPalRegisterCustomerValidator() {
            RuleFor(o => o.RequestToken)
                .NotEmpty()
                .WithMessage("empty request token");

            RuleFor(o => o.VerificationToken)
                .NotEmpty()
                .WithMessage("empty verification token");

            RuleFor(o => o.CustomerEmailAddress)
                .EmailAddress()
                .WithMessage("invalid email address");
        }
    }
}
