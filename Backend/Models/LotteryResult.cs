namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils;

	[DataContract]
	public class LotteryResult {
		[DataMember]
		public bool PlayedNow { get; set; }

		[DataMember]
		public long? PlayerID { get; set; }

		[DataMember]
		public Guid? UniqueID { get; set; }

		[DataMember]
		public long StatusID {
			get { return (long)(int)PlayerStatus; }
			set {
				int statusID = (int)value;

				PlayerStatus = Enum.IsDefined(typeof(LotteryPlayerStatus), statusID)
					? (LotteryPlayerStatus)value
					: LotteryPlayerStatus.Unknown;
			} // set
		} // StatusID

		[DataMember]
		public string Status { get; set; }

		[DataMember]
		public bool CanWin { get; set; }

		[DataMember]
		public bool HasPlayed { get; set; }

		[DataMember]
		public DateTime? PlayTime { get; set; }

		[DataMember]
		public long? PrizeID { get; set; }

		[DataMember]
		public decimal Amount { get; set; }

		[DataMember]
		[NonTraversable]
		public LotteryPlayerStatus PlayerStatus { get; set; }
	} // class LotteryResult
} // namespace
