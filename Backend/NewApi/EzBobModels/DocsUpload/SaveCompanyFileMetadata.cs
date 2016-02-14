using System;

namespace EzBobModels.DocsUpload
{
    /// <summary>
    /// Represents 'MP_CompanyFilesMetaData' table.
    /// </summary>
    public class CompanyFileMetadata
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime? Created { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileContentType { get; set; }
        public bool? IsBankStatement { get; set; }
    }
}
