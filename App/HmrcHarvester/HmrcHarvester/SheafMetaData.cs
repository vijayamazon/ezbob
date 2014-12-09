namespace Ezbob.HmrcHarvester {

	/// <summary>
	/// Data types fetched from the Field (VAT return, etc).
	/// </summary>
	public enum DataType {
		VatReturn,
		PayeRtiTaxYears,
	} // enum DataType

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

	/// <summary>
	/// File identifier class (data type, file type, file name).
	/// </summary>
	public class SheafMetaData {

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

	} // class SheafMetaData

} // namespace Ezbob.HmrcHarvester
