namespace EzBobRest.Validators {
    using EzBobApi.Commands.Customer;
    using EzBobCommon.Utils;
    using FluentValidation;

    public class CustomerSignupValidator : AbstractValidator<CustomerSignupCommand> {
        public CustomerSignupValidator() {

            RuleFor(c => c.Account)
                .NotNull()
                .WithMessage("Empty account")
                .DependentRules(r => r.RuleFor(c => c.Account.EmailAddress)
                    .EmailAddress()
                    .WithMessage("Invalid email address")
                    .DependentRules(rr => rr.RuleFor(o => o)
                        .Must(OtherSectionsBeEmpty)
                        .WithMessage("Invalid request format")));
        }


        private bool OtherSectionsBeEmpty(CustomerSignupCommand c) {
            return CollectionUtils.IsEmpty(c.EzBobHeaders) && c.IsFailed == false && c.IsTest == false && c.RequestedAmount == 0;
        }
    }
}