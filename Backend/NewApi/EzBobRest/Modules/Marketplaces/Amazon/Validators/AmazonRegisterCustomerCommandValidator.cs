namespace EzBobRest.Modules.Marketplaces.Amazon.Validators {
    using EzBobApi.Commands.Amazon;
    using EzBobCommon;
    using FluentValidation;

    /// <summary>
    /// Validates <see cref="AmazonRegisterCustomerCommand"/>.
    /// </summary>
    public class AmazonRegisterCustomerCommandValidator : ValidatorBase<AmazonRegisterCustomerCommand> {
        public AmazonRegisterCustomerCommandValidator() {
            RuleFor(o => o.SellerId)
                .NotEmpty()
                .WithMessage("empty seller id");

            RuleFor(o => o.AuthorizationToken)
                .NotEmpty()
                .WithMessage("empty authorization token");

            RuleFor(o => o.MarketplaceId)
                .Must(o => o.IsNotEmpty())
                .WithMessage("empty marketplace ids");
        }
    }
}
