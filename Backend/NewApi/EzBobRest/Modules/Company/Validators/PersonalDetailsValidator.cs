namespace EzBobRest.Modules.Company.Validators
{
    using EzBobApi.Commands.Customer.Sections;
    using EzBobModels.Enums;
    using FluentValidation;

    class PersonalDetailsValidator : AbstractValidator<PersonalDetailsInfo> {
        public PersonalDetailsValidator() {
            RuleFor(o => o.MaritalStatus)
                .Must(PassCheck)
                .When(o => o.MaritalStatus != null)
                .WithMessage("Invalid marital status.");
        }

        private bool PassCheck(string maritalStatus) {
            int status;
            bool res = int.TryParse(maritalStatus, out status);
            if (!res) {
                return false;
            }

            return status < (int)MaritalStatus.Married || status > (int)MaritalStatus.Other;
        }
    }
}
