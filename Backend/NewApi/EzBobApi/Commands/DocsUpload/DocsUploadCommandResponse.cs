namespace EzBobApi.Commands.DocsUpload
{
    using EzBobCommon.NSB;

    /// <summary>
    /// Response to <see cref="DocsUploadCommandResponse"/>.
    /// </summary>
    public class DocsUploadCommandResponse : CommandResponseBase
    {
        public string CustomerId { get; set; }
    }
}
