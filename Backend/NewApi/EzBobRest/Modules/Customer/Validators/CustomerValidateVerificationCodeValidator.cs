namespace EzBobRest.Modules.Customer.Validators {
    using EzBobApi.Commands.Customer;
    using FluentValidation;

    /// <summary>
    /// Validates <see cref="CustomerValidateVerificationCodeCommand"/>
    /// </summary>
    public class CustomerValidateVerificationCodeValidator : ValidatorBase<CustomerValidateVerificationCodeCommand> {
        public CustomerValidateVerificationCodeValidator() {
           
            RuleFor(o => o.VerificationCode)
                .NotEmpty()
                .WithMessage("empty verification code");

            RuleForRestParameter(o => o.VerificationToken);
        }
    }
}
