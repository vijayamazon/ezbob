namespace EzBobRest.Modules.Company.Validators
{
    using EzBobApi.Commands.Company;

    /// <summary>
    /// Validates <see cref="CompanyGetDetailsCommand"/>.
    /// </summary>
    public class CompanyGetDetailsValidator : ValidatorBase<CompanyGetDetailsCommand> {
        public CompanyGetDetailsValidator() {
            RuleForRestParameter(o => o.CompanyId);
        }
    }
}
