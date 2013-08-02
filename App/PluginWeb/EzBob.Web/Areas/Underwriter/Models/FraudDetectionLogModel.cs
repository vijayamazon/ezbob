namespace EzBob.Web.Areas.Underwriter.Models
{
    public class FraudDetectionLogModel
    {
        public int Id { get; set; }
        public string Type { get; set; }    
        public string CurrentField { get; set; }
        public string CompareField { get; set; }
        public string Value { get; set; }
        public string Concurrence { get; set; }
        public string DateOfLastCheck { get; set; }
    }
}