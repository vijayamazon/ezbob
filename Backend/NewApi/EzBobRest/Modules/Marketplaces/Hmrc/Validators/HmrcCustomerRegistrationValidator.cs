namespace EzBobRest.Modules.Marketplaces.Hmrc.Validators
{
    using EzBobApi.Commands.Hmrc;
    using FluentValidation;

    /// <summary>
    /// Validates <see cref="HmrcRegisterCustomerCommand"/>
    /// </summary>
    public class HmrcCustomerRegistrationValidator : ValidatorBase<HmrcRegisterCustomerCommand> {
        public HmrcCustomerRegistrationValidator() {
            RuleFor(o => o.UserName)
                .NotEmpty()
                .WithMessage("empty user name");

            RuleFor(o => o.Password)
                .NotEmpty()
                .WithMessage("empty password");
        }
    }
}
