namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System.Runtime.Serialization;

	[DataContract]
	public class EtlCode : EnumMember<long> {
		[DataMember]
		public virtual bool IsHardReject { get; set; }

		[DataMember]
		public virtual bool IsError { get; set; }
	} // class EtlCode
} // namespace
