namespace EzBobModels.Amazon {
    public class AmazonFeedbackItem {
        public int Id { get; set; }
        public int AmazonFeedbackId { get; set; }
        public int TimePeriodId { get; set; }
        public int Count { get; set; }
        public int Negative { get; set; }
        public int Positive { get; set; }
        public int Neutral { get; set; }
    }
}
