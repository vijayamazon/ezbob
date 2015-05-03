namespace EzBob.Web.Areas.Underwriter.Models {
    using System;

    public class AutoDecisionModel {
        public long TrailID { get; set; }
        public DateTime DecisionTime { get; set; }
        public string DecisionStatus { get; set; }
        public string DecisionName { get; set; }
        public string TrailTag { get; set; }
    }
}