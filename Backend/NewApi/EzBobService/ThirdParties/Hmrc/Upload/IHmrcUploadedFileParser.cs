namespace EzBobService.ThirdParties.Hmrc.Upload
{
    using EzBobCommon;
    using EzBobModels.Hmrc;

    public interface IHmrcUploadedFileParser {
        Optional<VatReturnsPerBusiness> ParseHmrcVatReturnsPdf(string fileName, InfoAccumulator info);
    }
}
