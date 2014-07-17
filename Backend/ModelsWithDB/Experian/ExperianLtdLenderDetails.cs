namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System.Runtime.Serialization;
	using Logger;
	using Utils;

	[DataContract]
	[LenderDetails]
	public class ExperianLtdLenderDetails : AExperianLtdDataRow {
		public ExperianLtdLenderDetails(ASafeLog oLog = null) : base(oLog) {} // constructor

		[DataMember]
		public long DL65ID { get; set; }

		[DataMember]
		[LenderDetails("LENDERNAME")]
		public string LenderName { get; set; }

		/// <summary>
		/// Do not remove.
		/// The only usage of this field is to hide from traversing corresponding field in the base class.
		/// </summary>
		[NonTraversable]
		public override long ExperianLtdID {
			get { return base.ExperianLtdID; }
			set { base.ExperianLtdID = value; }
		} // ExperianLtdID

		#region property ParentID

		[DataMember]
		[NonTraversable]
		public override long ParentID {
			get { return DL65ID; }
			set { DL65ID = value; }
		} // ParentID

		#endregion property ParentID
	} // class ExperianLtdLenderDetails
} // namespace
