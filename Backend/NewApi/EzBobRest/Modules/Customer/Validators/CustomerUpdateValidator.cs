namespace EzBobRest.Modules.Customer.Validators {
    using EzBobApi.Commands.Customer;
    using EzBobCommon;
    using EzBobRest.Modules.Company.Validators;
    using FluentValidation;

    /// <summary>
    /// Validates <see cref="CustomerUpdateCommand"/>
    /// </summary>
    public class CustomerUpdateValidator : ValidatorBase<CustomerUpdateCommand> {
        public CustomerUpdateValidator() {
            RuleFor(o => o.CurrentLivingAddress)
                .SetValidator(new CurrentLivingAddressValidator())
                .When(o => o.CurrentLivingAddress != null);

            RuleFor(o => o.PreviousLivingAddress)
                .SetValidator(new PreviousLivingAddressValidator())
                .When(o => o.PreviousLivingAddress != null);

            RuleForEach(o => o.AdditionalOwnedProperties)
                .SetValidator(new OwnPropertyAddressValidator())
                .When(o => o.AdditionalOwnedProperties.IsNotEmpty());

            RuleFor(o => o.ContactDetails)
                .SetValidator(new ContactDetailsValidator())
                .When(o => o.ContactDetails != null);

            RuleFor(o => o.PersonalDetails)
                .SetValidator(new PersonalDetailsValidator())
                .When(o => o.PersonalDetails != null);
        }
    }
}
