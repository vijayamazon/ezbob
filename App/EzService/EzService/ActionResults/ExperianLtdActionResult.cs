namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB.CompaniesHouse;

	[DataContract]
	public class ExperianLtdActionResult : ActionResult {
		[DataMember]
		public ExperianLtd Value { get; set; }

		[DataMember]
		public List<ScoreAtDate> History { get; set; }

		[DataMember]
		public CompaniesHouseOfficerOrder CompaniesHouse { get; set; }
	} // class ExperianLtdActionResult
} // namespace
