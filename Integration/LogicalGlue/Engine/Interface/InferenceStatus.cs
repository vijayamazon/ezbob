namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System.Net;
	using System.Runtime.Serialization;

	[DataContract]
	public class InferenceStatus {
		[DataMember]
		public HttpStatusCode HttpStatus { get; set; }
		[DataMember]
		public HttpStatusCode ResponseStatus { get; set; }
		[DataMember]
		public bool HasEquifaxData { get; set; }
	} // class InferenceStatus
} // namespace
