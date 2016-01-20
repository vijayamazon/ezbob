namespace EzBobRest.Modules.Marketplaces.Ebay.Validators {
    using EzBobApi.Commands.Ebay;
    using FluentValidation;

    /// <summary>
    /// Validates <see cref="EbayRegisterCustomerCommand"/>
    /// </summary>
    public class EbayRegisterCustomerValidator : ValidatorBase<EbayRegisterCustomerCommand> {
        public EbayRegisterCustomerValidator() {
            RuleFor(o => o.MarketplaceName)
                .NotEmpty()
                .WithMessage("empty marketplace name");

            RuleForRestParameter(o => o.SessionId);

            RuleFor(o => o.Token)
                .NotEmpty()
                .WithMessage("empty token");
        }
    }
}
