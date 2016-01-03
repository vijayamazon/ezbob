namespace EzBob3dPartiesApi.Yodlee
{
    using EzBobCommon.NSB;

    public class YodleeGetFastLinkCommandResponse : CommandResponseBase
    {
        public string FastLinkUrl { get; set; }
        public string FormHtml { get; set; }
    }
}
