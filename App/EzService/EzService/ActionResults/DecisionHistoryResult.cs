namespace EzService.ActionResults {
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Ezbob.Backend.ModelsWithDB;

	[DataContract]
	public class DecisionHistoryResult : ActionResult {
		[DataMember]
		public IEnumerable<DecisionHistoryDBModel> Model { get; set; }
	} // class DecisionHistoryResult
} // namespace
