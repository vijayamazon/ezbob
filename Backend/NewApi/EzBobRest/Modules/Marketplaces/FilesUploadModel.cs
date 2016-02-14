namespace EzBobRest.Modules.Marketplaces
{
    using System.Collections.Generic;
    using Nancy;

    public class FilesUploadModel
    {
        public string CustomerId { get; set; }
        public IEnumerable<HttpFile> Files { get; set; } 
    }
}
