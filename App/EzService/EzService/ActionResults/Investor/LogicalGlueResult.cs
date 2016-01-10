namespace EzService.ActionResults.Investor {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;

	[DataContract]
	public class LogicalGlueResult : ActionResult {
		[DataMember]
		public DateTime Date { get; set; }

		[DataMember]
		public Bucket? Bucket { get; set; }

		[DataMember]
		public string BucketStr { get; set; }

		[DataMember]
		public string Error { get; set; }

		[DataMember]
		public decimal? NNScore  { get; set; }

		[DataMember]
		public decimal? FLScore { get; set; }

		[DataMember]
		public decimal MonthlyRepayment { get; set; }

		[DataMember]
		public decimal BucketPercent { get; set; }
	}
}
