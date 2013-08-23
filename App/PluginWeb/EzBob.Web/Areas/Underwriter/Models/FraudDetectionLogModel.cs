using EZBob.DatabaseLib.Model.Fraud;
using EzBob.Web.Code;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class FraudDetectionLogModel
    {
        public FraudDetectionLogModel(FraudDetection fraudDetection)
        {
            Id = fraudDetection.Id;
            CompareField = fraudDetection.CompareField;
            CurrentField = fraudDetection.CurrentField;
            Value = fraudDetection.Value;
            Concurrence = ConcurrencePrepare(fraudDetection);
            Type = fraudDetection.ExternalUser != null ? "External" : "Internal";
            DateOfLastCheck = FormattingUtils.FormatDateTimeToString(fraudDetection.DateOfCheck);
        }

        public int Id { get; set; }
        public string Type { get; set; }    
        public string CurrentField { get; set; }
        public string CompareField { get; set; }
        public string Value { get; set; }
        public string Concurrence { get; set; }
        public string DateOfLastCheck { get; set; }

        private static string ConcurrencePrepare(FraudDetection val)
        {
            if (val.ExternalUser != null)
            {
                return string.Format("{0} {1} (id={2})",
                    val.ExternalUser.FirstName,
                    val.ExternalUser.LastName,
                    val.ExternalUser.Id);
            }

            var fullname = val.InternalCustomer.PersonalInfo != null ? val.InternalCustomer.PersonalInfo.Fullname : "-";
            return string.Format("{0} (id={1})",
                    fullname,
                    val.InternalCustomer.Id);
        }
    }
}