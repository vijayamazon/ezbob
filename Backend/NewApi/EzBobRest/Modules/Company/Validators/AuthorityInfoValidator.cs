namespace EzBobRest.Modules.Company.Validators {
    using EzBobApi.Commands.Company;
    using FluentValidation;

    /// <summary>
    /// Validates <see cref="AuthorityInfo"/>.
    /// </summary>
    public class AuthorityInfoValidator : AbstractValidator<AuthorityInfo> {

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorityInfoValidator"/> class.
        /// </summary>
        public AuthorityInfoValidator() {
            RuleFor(o => o.ContactDetails)
                .NotNull()
                .WithMessage("Empty contact details");

            RuleFor(o => o.ContactDetails)
                .SetValidator(new ContactDetailsValidator())
                .When(o => o.ContactDetails != null);
            
            When(o => o.AddressInfo != null, () => {
                RuleFor(o => o.AddressInfo.Line1)
                    .Cascade(CascadeMode.Continue)
                    .NotEmpty()
                    .WithMessage("Empty line1");
                RuleFor(o => o.AddressInfo.Postcode)
                    .Cascade(CascadeMode.Continue)
                    .NotEmpty()
                    .WithMessage("Empty postcode.");
            });
        }
    }
}
