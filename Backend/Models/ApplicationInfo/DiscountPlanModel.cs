namespace Ezbob.Backend.Models.ApplicationInfo {
	using System.Runtime.Serialization;

	[DataContract]
	public class DiscountPlanModel {
		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public string Name { get; set; }
	} // class DiscountPlanModel
} // namespace
