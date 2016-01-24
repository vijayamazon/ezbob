namespace EzBobApi.Commands.Hmrc {
    using System.Collections.Generic;
    using EzBobCommon.NSB;

    public class HmrcProcessUploadedFilesCommand : CommandBase {
        public string CustomerId { get; set; }
        public IEnumerable<string> Files { get; set; }
    }
}
