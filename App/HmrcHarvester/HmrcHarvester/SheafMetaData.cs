namespace Ezbob.HmrcHarvester {
	#region enum DataType

	/// <summary>
	/// Data types fetched from the Field (VAT return, etc).
	/// </summary>
	public enum DataType {
		VatReturn,
		PayeRtiTaxYears,
	} // enum DataType

	#endregion enum DataType

	#region enum FileType

	/// <summary>
	/// File types fetched from the Field.
	/// </summary>
	public enum FileType {
		Unknown,

		/// <summary>
		/// HTML file
		/// </summary>
		Html,

		/// <summary>
		/// PDF file
		/// </summary>
		Pdf,
	} // enum FileType

	#endregion enum FileType

	#region class SheafMetaData

	/// <summary>
	/// File identifier class (data type, file type, file name).
	/// </summary>
	public class SheafMetaData {
		#region public

		/// <summary>
		/// Data type.
		/// </summary>
		public DataType DataType;

		/// <summary>
		/// File type.
		/// </summary>
		public FileType FileType;

		/// <summary>
		/// File name.
		/// </summary>
		public string BaseFileName;

		/// <summary>
		/// An object to parse retrieved data.
		/// </summary>
		public AThrasher Thrasher;

		public override string ToString() {
			return string.Format("{0} {1} file named {2} with{3} thrasher", DataType, FileType, BaseFileName, Thrasher == null ? "out" : "");
		} // ToString
		#endregion public
	} // class SheafMetaData

	#endregion class SheafMetaData
} // namespace Ezbob.HmrcHarvester
