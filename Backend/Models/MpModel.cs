
namespace Ezbob.Backend.Models {
	using System.Runtime.Serialization;
	using System;
	using System.Collections.Generic;

	[Serializable]
	[DataContract(IsReference = true)]
	public class MpModel {
		[DataMember]
		public List<MarketPlaceDataModel> MarketPlaces { get; set; }
		[DataMember]
		public List<AffordabilityData> Affordability { get; set; }
	}
}
