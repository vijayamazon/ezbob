namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_DiscountPlans {

		[PK]
		[NonTraversable]
		[DataMember]
		public int DiscountPlanID { get; set; }

		[DataMember]
		public string DiscountPlan { get; set; }

		[DataMember]
		public bool? IsDefault { get; set; }

		[DataMember]
		public bool? IsActive { get; set; }

	}//class NL_BlendedOffers

}//ns