using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobRest.Validators {
    using EzBobApi.Commands.Company;
    using EzBobCommon;
    using FluentValidation;

    /// <summary>
    /// Validates authority info
    /// </summary>
    public class AuthorityInfoValidator : AbstractValidator<AuthorityInfo> {

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorityInfoValidator"/> class.
        /// </summary>
        public AuthorityInfoValidator() {
            RuleFor(o => o.ContactDetails)
                .SetValidator(new ContactDetailsValidator())
                .When(o => o.ContactDetails != null);
        }
    }
}
