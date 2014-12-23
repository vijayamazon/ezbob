
namespace IMailLib {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using Aspose.Words;

	public static class PrepareMail {
		static PrepareMail() {
			var license = new License();

			using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("IMailLib.Aspose.Total.lic")) {
				if (s != null) {
					s.Position = 0;
					license.SetLicense(s);
				} // if
			} // using
		}
		public static byte[] ReplaceParametersAndConvertToPdf(string templatePath, Dictionary<string, string> variables) {
			Document doc = new Document(templatePath);

			foreach (var variable in variables) {
				string parameter = string.Format("@{0}@", variable.Key);
				int result = doc.Range.Replace(parameter, variable.Value, false, false);
				Console.WriteLine("Replace {0} to {1} result: {2}", parameter, variable.Value, result);
			}

			foreach (Section section in doc.Sections) {
				section.PageSetup.PaperSize = PaperSize.A4;
			}
			
			MemoryStream ms = new MemoryStream();
			var output = doc.Save(ms, SaveFormat.Pdf);
			return ms.ToArray();
		}

		public static void SaveFile(byte[] data, string filePath) {
			File.WriteAllBytes(filePath, data);
		}

		public static byte[] GetPdfData(string filePath) {
			if (System.IO.File.Exists(filePath)) {
				FileInfo fInfo = new FileInfo(filePath);
				long numBytes = fInfo.Length;
				long MBsize = numBytes / 1000000;

				if (MBsize <= 8)        // server has an 8MB file upload limit
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
	}
}
