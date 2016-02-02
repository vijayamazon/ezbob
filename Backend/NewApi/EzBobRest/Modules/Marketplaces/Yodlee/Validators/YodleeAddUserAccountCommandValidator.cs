namespace EzBobRest.Modules.Marketplaces.Yodlee.Validators
{
    using EzBobApi.Commands.Yodlee;
    using FluentValidation;

    /// <summary>
    /// Validates <see cref="YodleeAddUserAccountCommand"/>
    /// </summary>
    public class YodleeAddUserAccountCommandValidator : ValidatorBase<YodleeAddUserAccountCommand> {
        public YodleeAddUserAccountCommandValidator() {
            RuleFor(o => o.ContentServiceId)
                .NotEmpty()
                .WithMessage("empty content service id");
        }
    }
}
