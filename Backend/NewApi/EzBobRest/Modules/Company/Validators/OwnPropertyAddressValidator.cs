namespace EzBobRest.Modules.Company.Validators {
    internal class OwnPropertyAddressValidator : LivingAddressValidator {
        public OwnPropertyAddressValidator()
            : base("[Own property address]: ", 0) {}
    }
}
