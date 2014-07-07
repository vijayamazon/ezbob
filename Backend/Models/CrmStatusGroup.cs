namespace Ezbob.Backend.Models {
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	[DataContract]
	public class CrmStatusGroup {
		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public int Priority { get; set; }

		[DataMember]
		public List<IdNameModel> Statuses { get; set; }
	} // CrmStatusGroup

	[DataContract]
	public class CrmStaticModel {
		[DataMember]
		public IEnumerable<IdNameModel> CrmActions { get; set; }

		[DataMember]
		public IEnumerable<CrmStatusGroup> CrmStatuses { get; set; }

		[DataMember]
		public IEnumerable<IdNameModel> CrmRanks { get; set; }
	} // class CrmStaticModel
} // namespace
