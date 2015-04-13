namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditApplicantAddresses {
		[PK]
		[NonTraversable]
		public long CallCreditApplicantAddressesID { get; set; }
		[FK("CallCredit", "CallCreditID")]
		public long? CallCreditID { get; set; }
		[Length(30)]
		public string AbodeNo { get; set; }
		[Length(12)]
		public string BuildingNo { get; set; }
		[Length(50)]
		public string BuildingName { get; set; }
		[Length(50)]
		public string Street1 { get; set; }
		[Length(50)]
		public string Street2 { get; set; }
		[Length(35)]
		public string SubLocality { get; set; }
		[Length(35)]
		public string Locality { get; set; }
		[Length(25)]
		public string PostTown { get; set; }
		[Length(8)]
		public string PostCode { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		[Length(15)]
		public string Duration { get; set; }
	}
}
