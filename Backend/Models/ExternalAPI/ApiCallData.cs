namespace Ezbob.Backend.Models.ExternalAPI {
	using System.Runtime.Serialization;

	[DataContract]
	public class ApiCallData {

		[DataMember]
		public string Url { get; set; }

		[DataMember]
		public string RequestId { get; set; }

		[DataMember]
		public string Request { get; set; }

		[DataMember]
		public string Response { get; set; }

		[DataMember]
		public  string StatusCode { get; set; }

		[DataMember]
		public  string ErrorCode { get; set; }

		[DataMember]
		public string ErrorMessage { get; set; }

		[DataMember]
		public string Comments { get; set; }

		[DataMember]
		public string Source { get; set; }

		[DataMember]
		public int? CustomerID { get; set; }

		
	}
}
