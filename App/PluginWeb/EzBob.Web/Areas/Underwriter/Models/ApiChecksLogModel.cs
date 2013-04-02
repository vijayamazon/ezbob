namespace EzBob.Web.Areas.Underwriter.Models
{
    public class ApiChecksLogModel
    {
        public string DateTime { get; set; }
        public string ApiType { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public string Marketplace { get; set; }
    }
}