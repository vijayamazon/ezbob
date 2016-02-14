using System.Collections.Generic;
using System.Linq;

namespace EzBobRest.Modules.Marketplaces.Hmrc.Validators
{
    using FluentValidation;
    using Nancy;

    public class HmrcUploadPdfValidator : ValidatorBase<FilesUploadModel>
    {
        public HmrcUploadPdfValidator() {
            RuleFor(model => HasValidContentType(model.Files))
                .Must(c => true)
                .WithMessage("some files are not pdfs");
        }

        private bool HasValidContentType(IEnumerable<HttpFile> files)
        {
            return files.All(file => MimeTypes.GetMimeType(file.Name)
                .Equals(file.ContentType) && file.ContentType.ToLowerInvariant()
                    .Contains("pdf"));
        }
    }
}
