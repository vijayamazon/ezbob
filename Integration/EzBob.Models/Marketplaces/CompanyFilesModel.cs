namespace EzBob.Models.Marketplaces
{
	using System;
	using System.Collections.Generic;
	public class CompanyFilesModel
	{
		public List<CompanyFile> Files { get; set; }
	}

	public class CompanyFile
	{
		public string FileName { get; set; }
		public DateTime Uploaded { get; set; }
		public int Id { get; set; }
	}
}
