using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobRest.Validators
{
    using EzBobApi.Commands.Company;
    using EzBobCommon;
    using EzBobCommon.Utils;
    using FluentValidation;

    /// <summary>
    /// Validates company update command
    /// </summary>
    public class CompanyUpdateValidator : AbstractValidator<UpdateCompanyCommand> {
        public CompanyUpdateValidator() {
            When(o => o.CompanyDetails != null, () => {
                When(o => CollectionUtils.IsNotEmpty(o.CompanyDetails.Authorities), () => {
                    RuleForEach(o => o.CompanyDetails.Authorities)
                        .SetValidator(new AuthorityInfoValidator());
                });
            });
        }
    }
}
