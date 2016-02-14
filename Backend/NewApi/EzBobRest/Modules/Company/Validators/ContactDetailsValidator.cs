namespace EzBobRest.Modules.Company.Validators {
    using EzBobApi.Commands.Customer.Sections;
    using EzBobCommon;
    using FluentValidation;

    /// <summary>
    /// Validates contact details
    /// </summary>
    public class ContactDetailsValidator : AbstractValidator<ContactDetailsInfo> {
        public ContactDetailsValidator() {

            RuleFor(o => o.EmailAddress.IsNotEmpty() || o.MobilePhone.IsNotEmpty())
                .Must(o => o)
                .WithMessage("[Contact Details]: Either email address or mobile phone number should be specified.");

            RuleFor(o => o.EmailAddress)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("[Contact Details]: Invalid email address.")
                .When(o => o.EmailAddress.IsNotEmpty());
        }
    }
}
