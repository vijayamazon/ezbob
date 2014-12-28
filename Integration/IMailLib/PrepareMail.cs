namespace IMailLib {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using Aspose.Words;
	using iTextSharp.text;
	using iTextSharp.text.pdf;

	public static class PrepareMail {
		static PrepareMail() {
			var license = new License();

			using (var s = Assembly.GetExecutingAssembly()
				.GetManifestResourceStream("IMailLib.Aspose.Total.lic")) {
				if (s != null) {
					s.Position = 0;
					license.SetLicense(s);
				} // if
			} // using
		}

		public static byte[] ConcatinatePdfFiles(List<byte[]> pdf) {
			byte[] all;

			using (MemoryStream ms = new MemoryStream()) {
				iTextSharp.text.Document doc = new iTextSharp.text.Document();

				PdfWriter writer = PdfWriter.GetInstance(doc, ms);

				doc.SetPageSize(PageSize.LETTER);
				doc.Open();
				PdfContentByte cb = writer.DirectContent;
				PdfImportedPage page;

				PdfReader reader;
				foreach (byte[] p in pdf) {
					reader = new PdfReader(p);
					int pages = reader.NumberOfPages;

					// loop over document pages
					for (int i = 1; i <= pages; i++) {
						doc.SetPageSize(PageSize.A4);
						doc.NewPage();
						page = writer.GetImportedPage(reader, i);
						cb.AddTemplate(page, 0, 0);
					}
				}

				doc.Close();
				all = ms.GetBuffer();
				ms.Flush();
				ms.Dispose();
			}

			return all;
		}

		public static byte[] ExtractResource(string filename) {
			System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
			using (Stream resFilestream = a.GetManifestResourceStream(filename)) {
				if (resFilestream == null)
					return null;
				byte[] ba = new byte[resFilestream.Length];
				resFilestream.Read(ba, 0, ba.Length);
				return ba;
			}
		}

		public static Stream ExtractResourceAsStream(string filename) {
			System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
			Stream resFilestream = a.GetManifestResourceStream(filename);
			return resFilestream;
		}

		public static byte[] GetPdfData(string filePath) {
			if (System.IO.File.Exists(filePath)) {
				FileInfo fInfo = new FileInfo(filePath);
				long numBytes = fInfo.Length;
				long MBsize = numBytes / 1000000;

				if (MBsize <= 8) // server has an 8MB file upload limit
				{
					FileStream fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
					BinaryReader br = new BinaryReader(fStream);

					// convert the file to a byte array
					byte[] fileData = br.ReadBytes((int)numBytes);
					br.Close();

					// tidy up
					fStream.Close();
					fStream.Dispose();

					return fileData;
				}
			}

			Console.WriteLine("File {0}  is not found or too big", filePath);
			return null;
		}

		public static byte[] ReplaceParametersAndConvertToPdf(string templatePath, Dictionary<string, string> variables) {
			Aspose.Words.Document doc = new Aspose.Words.Document(templatePath);

			foreach (var variable in variables) {
				string parameter = string.Format("@{0}@", variable.Key);
				int result = doc.Range.Replace(parameter, variable.Value, false, false);
				Console.WriteLine("Replace {0} to {1} result: {2}", parameter, variable.Value, result);
			}

			foreach (Aspose.Words.Section section in doc.Sections)
				section.PageSetup.PaperSize = PaperSize.A4;

			MemoryStream ms = new MemoryStream();
			var output = doc.Save(ms, SaveFormat.Pdf);
			return ms.ToArray();
		}

		public static byte[] ReplaceParametersAndConvertToPdf(Stream template, Dictionary<string, string> variables) {
			Aspose.Words.Document doc = new Aspose.Words.Document(template);

			foreach (var variable in variables) {
				string parameter = string.Format("@{0}@", variable.Key);
				string value = string.IsNullOrEmpty(variable.Value) ? " " : variable.Value;
				int result = doc.Range.Replace(parameter, value, false, false);
				Console.WriteLine("Replace {0} to {1} result: {2}", parameter, variable.Value, result);
			}

			foreach (Aspose.Words.Section section in doc.Sections)
				section.PageSetup.PaperSize = PaperSize.A4;

			MemoryStream ms = new MemoryStream();
			var output = doc.Save(ms, SaveFormat.Pdf);
			return ms.ToArray();
		}

		public static void SaveFile(byte[] data, string filePath) {
			File.WriteAllBytes(filePath, data);
		}
	}
}
