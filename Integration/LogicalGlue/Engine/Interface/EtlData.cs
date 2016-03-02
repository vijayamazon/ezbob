namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System.Runtime.Serialization;

	[DataContract]
	public class EtlData {
		[DataMember]
		public EtlCode Code { get; set; }

		[DataMember]
		public string Message { get; set; }
	} // class EtlData
} // namespace
