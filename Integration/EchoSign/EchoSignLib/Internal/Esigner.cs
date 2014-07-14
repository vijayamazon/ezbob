namespace EchoSignLib {
	using Ezbob.Database;

	internal class Esigner {
		[FieldName("EsignerID")]
		public int ID { get; set; }

		public int? DirectorID { get; set; }
		public string DirectorEmail { get; set; }
	} // class Esigner
} // namespace
