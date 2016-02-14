namespace EzBobApi.Commands.DocsUpload
{
    using System.Collections.Generic;
    using EzBobCommon.NSB;

    public class DocsUploadCommand : CommandBase
    {
        public string CustomerId { get; set; }
        public IEnumerable<string> Files { get; set; }
        public bool IsBankDocuments { get; set; }
    }
}
