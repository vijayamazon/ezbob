using System;
using System.Collections.Generic;

namespace EzBob3dParties.Amazon.RatingScraper {
    using EzBob3dParties.Amazon.RatingScraper.Feedback;

    public class AmazonCustomerRatingInfo {
        public double Rating { get; set; }
        public IEnumerable<FeedbackInfo> Feedbacks { get; set; }
        public string Name { get; set; }
        public DateTime SubmittedDate { get; set; }
    }
}
