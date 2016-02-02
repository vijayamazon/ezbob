namespace EzBobRest.Modules.PostCode.Validators
{
    using EzBobApi.Commands.SimplyPostcode;
    using FluentValidation;

    /// <summary>
    /// Validates <see cref="SimplyPostcodeGetAddressesCommand"/>.
    /// </summary>
    public class GetAddressesByPostCodeValidator : ValidatorBase<SimplyPostcodeGetAddressesCommand>
    {
        public GetAddressesByPostCodeValidator() {
            RuleFor(o => o.PostCode)
                .NotEmpty()
                .WithMessage("empty postcode.");
        }
    }
}
