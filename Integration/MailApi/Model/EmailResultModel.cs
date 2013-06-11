using System.Text;

// ReSharper disable InconsistentNaming
namespace MailApi.Model
{
    public class EmailResultModel
    {
        public string email { get; set; }
        public string status { get; set; }
        public string _id { get; set; }
        public string reject_reason { get; set; }

        public override string ToString()
        {
            var retVal = new StringBuilder();
            retVal.AppendLine("email: " + email);
            retVal.AppendLine("status: " + status);
            retVal.AppendLine("_id: " + _id);
            retVal.AppendLine("reject_reason: " + reject_reason);
            return retVal.ToString();
        }
    }
}