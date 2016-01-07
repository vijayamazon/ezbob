using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobRest.Validators {
    using EzBobApi.Commands.Company;
    using EzBobCommon;
    using EzBobCommon.Utils;
    using FluentValidation;

    /// <summary>
    /// Validates company update command
    /// </summary>
    public class CompanyUpdateValidator : AbstractValidator<UpdateCompanyCommand> {
        public CompanyUpdateValidator() {
            RuleFor(o => o.CustomerId)
                .NotEmpty()
                .WithMessage("customer id is mandatory")
                .DependentRules(d => d.RuleFor(c => c.CustomerId.ToLowerInvariant())
                    .NotEqual("{customerid}")//if url parameter is not specified Nancy put default string
                    .WithMessage("customer id is mandatory"));

            When(o => o.CompanyDetails != null, () => {
                When(o => CollectionUtils.IsNotEmpty(o.CompanyDetails.Authorities), () => {
                    RuleForEach(o => o.CompanyDetails.Authorities)
                        .SetValidator(new AuthorityInfoValidator());
                });
            });
        }
    }
}
