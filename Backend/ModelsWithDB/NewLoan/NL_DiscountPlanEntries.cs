namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_DiscountPlanEntries:AStringable {

		[PK]
		[NonTraversable]
		[DataMember]
		public int DiscountPlanEntryID { get; set; }

		[DataMember]
		public int PaymentOrder { get; set; }

		[DataMember]
		public decimal InterestDiscount { get; set; }

		[FK("FK_NL_DiscountPlanEntries_NL_DiscountPlans", "DiscountPlanID")]
		[DataMember]
		public int DiscountPlanID { get; set; }

	}//class NL_DiscountPlanEntries
}//ns
