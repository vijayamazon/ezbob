namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_RepaymentIntervalTypes : AStringable {
		[PK]
		[DataMember]
		public int RepaymentIntervalTypeID { get; set; }

		[DataMember]
		public bool IsMonthly { get; set; }

		[DataMember]
		public int? LengthInDays { get; set; }

		[Length(255)]
		[DataMember]
		public string Description { get; set; }
	} // class NL_RepaymentIntervalTypes
} // namespace
