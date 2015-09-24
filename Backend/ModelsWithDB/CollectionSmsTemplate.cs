namespace Ezbob.Backend.ModelsWithDB {
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CollectionSmsTemplate {
		[PK]
		[NonTraversable]
		public int CollectionSmsTemplateID { get; set; }

		[Length(30)]
		public string Type { get; set; }

		public bool IsActive { get; set; }

		[FK("CustomerOrigin", "CustomerOriginID")]
		public int OriginID { get; set; }

		[Length(500)]
		public string Template { get; set; }
	}
}
