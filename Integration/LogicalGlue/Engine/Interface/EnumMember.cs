namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System;
	using System.Runtime.Serialization;

	[DataContract]
	public class EnumMember<IntType> where IntType : struct {
		public static implicit operator IntType?(EnumMember<IntType> e) {
			return e == null ? (IntType?)null : e.Value;
		} // operator IntType?

		public static implicit operator IntType(EnumMember<IntType> e) {
			return e == null ? default(IntType) : e.Value;
		} // operator IntType

		public static implicit operator string(EnumMember<IntType> e) {
			return e == null ? null : e.Name;
		} // operator string

		public EnumMember() {
			if ((typeof(IntType) != typeof(int)) && (typeof(IntType) != typeof(long)))
				throw new Exception("Enum member base type must be int or long while " + typeof(IntType) + " specified.");
		} // constructor

		[DataMember]
		public virtual IntType Value { get; set; }

		[DataMember]
		public virtual string Name { get; set; }

		[DataMember]
		public virtual string CommunicationCode { get; set; }

		public virtual string SearchKey {
			get { return string.IsNullOrEmpty(CommunicationCode) ? Name : CommunicationCode; }
		} // SearchKey
	} // class EnumMember
} // namespace
