namespace EzBobRest.Modules.Company.Validators {
    using EzBobApi.Commands.Company;
    using EzBobCommon;
    using EzBobCommon.Utils;
    using FluentValidation;

    /// <summary>
    /// Validates company update command
    /// </summary>
    public class CompanyUpdateValidator : ValidatorBase<UpdateCompanyCommand> {
        public CompanyUpdateValidator() {

            When(o => o.CompanyDetails != null, () => {
                When(o => CollectionUtils.IsNotEmpty(o.CompanyDetails.Authorities), () => {
                    RuleForEach(o => o.CompanyDetails.Authorities)
                        .SetValidator(new AuthorityInfoValidator());
                });
            });

            RuleFor(o => o.CompanyDetails.TypeOfBusiness)
                .NotEmpty()
                .WithMessage("type of business is mandatory when creating company")
                .When(o => o.CompanyId.IsEmpty());
        }
    }
}
