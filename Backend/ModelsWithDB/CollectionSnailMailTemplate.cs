namespace Ezbob.Backend.ModelsWithDB {
	using Ezbob.Utils.dbutils;

	public class CollectionSnailMailTemplate {
		[PK(true)]
		public int CollectionSnailMailTemplateID { get; set; }

		[Length(30)]
		public string Type { get; set; }
		
		public bool IsLimited { get; set; }

		public bool IsActive { get; set; }
		
		[FK("CustomerOrigin", "CustomerOriginID")]
		public int OriginID { get; set; }

		[Length(100)]
		public string FileName { get; set; }

		[Length(100)]
		public string TemplateName { get; set; }
		
		public byte[] Template { get; set; }
	}
}
