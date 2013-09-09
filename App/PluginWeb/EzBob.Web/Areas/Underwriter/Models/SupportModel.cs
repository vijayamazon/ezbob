using System;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class SupportModel
    {
        public int Umi { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime? UpdateStartDate { get; set; }
        public string ErrorMessage { get; set; }
        public int CustomerId { get; set; }
        public string Status { get; set; }
    }
}