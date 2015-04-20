namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditAmendments {
		[PK]
		[NonTraversable]
		public long CallCreditAmendmentsID { get; set; }
		[FK("CallCredit", "CallCreditID")]
		public long? CallCreditID { get; set; }

		[Length(20)]
		public string AmendmentName { get; set; }
		[Length(6)]
		public string AmendmentType { get; set; }
		public int? Balorlim { get; set; }
		[Length(15)]
		public string Term { get; set; }
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
		public string Sublocality { get; set; }
		[Length(35)]
		public string Locality { get; set; }
		[Length(25)]
		public string PostTown { get; set; }
		[Length(8)]
		public string PostCode { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		[Length(30)]
		public string Duration { get; set; }
		[Length(30)]
		public string Title { get; set; }
		[Length(30)]
		public string Forename { get; set; }
		[Length(40)]
		public string OtherNames { get; set; }
		[Length(30)]
		public string SurName { get; set; }
		[Length(30)]
		public string Suffix { get; set; }
	}
}
