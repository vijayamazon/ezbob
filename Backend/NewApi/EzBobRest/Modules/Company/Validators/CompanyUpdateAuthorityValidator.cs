namespace EzBobRest.Modules.Company.Validators {
    using EzBobApi.Commands.Company;
    using FluentValidation;

    public class CompanyUpdateAuthorityValidator : ValidatorBase<CompanyUpdateAuthorityCommand> {
        public CompanyUpdateAuthorityValidator() {
            RuleFor(o => o.CompanyId)
                .NotEmpty()
                .WithMessage("Empty company id");

            RuleFor(o => o.Authority)
                .NotNull()
                .WithMessage("Empty authority");

            RuleFor(o => o.Authority)
                .SetValidator(new AuthorityInfoValidator())
                .When(o => o.Authority != null);
        }
    }
}
