namespace EzBobRest.Modules.Customer.Validators
{
    using EzBobApi.Commands.Customer;
    using FluentValidation;

    /// <summary>
    /// Validates <see cref="CustomerLoginCommand"/>.
    /// </summary>
    public class CustomerLoginCommandValidator : AbstractValidator<CustomerLoginCommand> {
    }
}
