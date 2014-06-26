namespace EzService {
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class EsignatureListActionResult : ActionResult {
		[DataMember]
		public List<Esignature> Data { get; set; }
	} // class EsignatureListActionResult
} // namespace
