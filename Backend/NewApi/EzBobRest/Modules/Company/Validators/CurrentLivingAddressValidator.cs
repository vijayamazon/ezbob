namespace EzBobRest.Modules.Company.Validators {
    internal class CurrentLivingAddressValidator : LivingAddressValidator {
        public CurrentLivingAddressValidator()
            : base("[Current living address]: ") {}
    }
}
