namespace EzBob.Web.Areas.Underwriter.Models.Fraud {
    using System.Collections.Generic;

    public class IovationDetailsModel {
        public string Result { get; set; }
        public string Reason { get; set; }
        public string TrackingNumber { get; set; }
        public string Created { get; set; }
        public string Origin { get; set; }
        public IEnumerable<IovationDetailModel> Details { get; set; }
    }

    public class IovationDetailModel{
        public string name { get; set; }
        public string value { get; set; }
    }
}