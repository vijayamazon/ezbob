namespace EzBobRest.Modules.Customer.Validators {
    using EzBobApi.Commands.Customer;
    using EzBobCommon;
    using EzBobCommon.Utils;
    using FluentValidation;

    /// <summary>
    /// Validates <see cref="CustomerSignupCommand"/>.
    /// </summary>
    public class CustomerSignupValidator : AbstractValidator<CustomerSignupCommand> {
        public CustomerSignupValidator() {

            RuleFor(o => o)
                .Must(OtherSectionsBeEmpty)
                .WithMessage("Invalid request.");

            RuleFor(o => o.EmailAddress)
                .EmailAddress()
                .NotEmpty()
                .WithMessage("Invalid email address.");

            RuleFor(o => o.Password)
                .NotEmpty()
                .WithMessage("Invalid password.");

            When(o => o.SecurityQuestionAnswer.IsNotEmpty(),
                () => RuleFor(o => o.SequrityQuestionId)
                    .Must(IsSecurityAnswerIdInRange)
                    .WithMessage("Invalid security answer id for security question."));

            When(o => IsSecurityAnswerIdInRange(o.SequrityQuestionId),
                () => RuleFor(o => o.SecurityQuestionAnswer)
                    .NotEmpty()
                    .WithMessage("Empty security answer."));
        }

        /// <summary>
        /// Determines whether the specified security question identifier].
        /// </summary>
        /// <param name="securityQuestionId">The security question identifier.</param>
        /// <returns></returns>
        private bool IsSecurityAnswerIdInRange(int? securityQuestionId) {
            return securityQuestionId.HasValue && securityQuestionId > 0 && securityQuestionId < 4;
        }


        /// <summary>
        /// Checks that other sections are empty.
        /// </summary>
        /// <param name="Command">The Command.</param>
        /// <returns></returns>
        private bool OtherSectionsBeEmpty(CustomerSignupCommand Command) {
            return CollectionUtils.IsEmpty(Command.EzBobHeaders) && Command.IsFailed == false;
        }
    }
}