namespace IMailLib {
	
	public class SnailMailTemplate {
		public string Type { get; set; }
		public bool IsLimited { get; set; }
		public bool IsActive { get; set; }
		public int OriginID { get; set; }
		public string FileName { get; set; }
		public string TemplateName { get; set; }
		public byte[] Template { get; set; }
	}
}
