namespace EchoSignLib {
	using Ezbob.Database;
	using Ezbob.Utils;

	internal class Esigner : ITraversable {
		[FieldName("EsignerID")]
		public int ID { get; set; }

		public int? DirectorID { get; set; }
		public string DirectorEmail { get; set; }
	} // class Esigner
} // namespace
