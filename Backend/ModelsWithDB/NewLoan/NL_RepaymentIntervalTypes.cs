namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_RepaymentIntervalTypes {

		[PK]
		[DataMember]
		public int RepaymentIntervalTypeID { get; set; }

		[DataMember]
		public string RepaymentIntervalType { get; set; }



	}
}
