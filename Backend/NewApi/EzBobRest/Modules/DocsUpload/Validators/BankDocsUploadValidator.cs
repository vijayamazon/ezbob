namespace EzBobRest.Modules.DocsUpload.Validators {
    using EzBobRest.Modules.Marketplaces;
    using FluentValidation;

    /// <summary>
    /// Validates <see cref="FilesUploadModel"/>.
    /// </summary>
    public class BankDocsUploadValidator : ValidatorBase<FilesUploadModel> {
        public BankDocsUploadValidator() {
            RuleForEach(o => o.Files)
                .Must(o => o.Value.Length < 10000000) //TODO: take from configuration
                .WithMessage("Got file/s large than 10000000");
        }
    }
}
