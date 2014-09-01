namespace EZBob.DatabaseLib.Common {
	public class FileInfo {
		public FileInfo(){}
		
		public FileInfo(string fileName, string filePath, string fileContentType) {
			FileName = fileName;
			FilePath = filePath;
			FileContentType = fileContentType;
		}

		public string FileName { get; set; }
		public string FilePath { get; set; }
		public string FileContentType { get; set; }
	}
}
