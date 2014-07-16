namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System.Runtime.Serialization;
	using System.Xml;
	using Logger;
	using Utils;

	[DataContract]
	[LenderDetails]
	public class ExperianLtdLenderDetails : AExperianLtdDataRow {
		public ExperianLtdLenderDetails(XmlNode oRoot = null, ASafeLog oLog = null) : base(oRoot, oLog) {} // constructor

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

		#region method SetParentID

		public override void SetParentID(long nParentID) {
			DL65ID = nParentID;
		} // SetParentID

		#endregion method SetParentID
	} // class ExperianLtdLenderDetails
} // namespace
