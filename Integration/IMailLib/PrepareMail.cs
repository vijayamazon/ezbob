namespace IMailLib {
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Reflection;
	using Aspose.Words;
	using Aspose.Words.Tables;
	using iTextSharp.text;
	using iTextSharp.text.pdf;
	using log4net;

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

		public static Stream ByteArrayToStream(byte[] byteArray) {
			return new MemoryStream(byteArray);
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

			Log.InfoFormat("File {0}  is not found or too big", filePath);
			return null;
		}

		public static byte[] ReplaceParametersAndConvertToPdf(string templatePath, Dictionary<string, string> variables) {
			Aspose.Words.Document doc = new Aspose.Words.Document(templatePath);
			
			foreach (var variable in variables) {
				string parameter = string.Format("@{0}@", variable.Key);
				int result = doc.Range.Replace(parameter, variable.Value, false, false);
				Log.InfoFormat("Replace {0} to {1} result: {2}", parameter, variable.Value, result);
			}

			foreach (Aspose.Words.Section section in doc.Sections)
				section.PageSetup.PaperSize = PaperSize.A4;

			MemoryStream ms = new MemoryStream();
			var output = doc.Save(ms, SaveFormat.Pdf);
			return ms.ToArray();
		}

		public static byte[] ReplaceParametersAndConvertToPdf(Aspose.Words.Document doc, Dictionary<string, string> variables) {
			foreach (var variable in variables) {
				string parameter = string.Format("@{0}@", variable.Key);
				string value = string.IsNullOrEmpty(variable.Value) ? " " : variable.Value;
				int result = doc.Range.Replace(parameter, value, false, false);
				Log.InfoFormat("Replace {0} to {1} result: {2}", parameter, variable.Value, result);
			}

			foreach (Aspose.Words.Section section in doc.Sections)
				section.PageSetup.PaperSize = PaperSize.A4;

			MemoryStream ms = new MemoryStream();
			var output = doc.Save(ms, SaveFormat.Pdf);
			return ms.ToArray();
		}

		public static byte[] ReplaceParametersAndConvertToPdf(Stream template, Dictionary<string, string> variables) {
			Aspose.Words.Document doc = new Aspose.Words.Document(template);
			return ReplaceParametersAndConvertToPdf(doc, variables);
		}

		public static Aspose.Words.Document GetDocumentFromTemplate(Stream template) {
			Aspose.Words.Document doc = new Aspose.Words.Document(template);
			return doc;
		}

		public static Aspose.Words.Document ReplaceNodeByAnotherDocument(Aspose.Words.Document doc, string nodeStr, Aspose.Words.Document insertDoc) {
			foreach (Aspose.Words.Section sec in doc.Sections) {
				foreach (Node node in sec.Body) {
					if (node.ToTxt().Contains(nodeStr)) {
						Log.InfoFormat("Node Found");
						InsertDocument(node, insertDoc);
					}
				}
			}

			return doc;
		}

		public static Aspose.Words.Document CreateTable(TableModel tableModel) {
			Aspose.Words.Document doc = new Aspose.Words.Document();
			DocumentBuilder builder = new DocumentBuilder(doc);
			builder.Font.Size = 10;
			
			Table table = builder.StartTable();
			
			builder.CellFormat.Borders.Left.LineWidth = 1.0;
			builder.CellFormat.Borders.Right.LineWidth = 1.0;
			builder.CellFormat.Borders.Top.LineWidth = 1.0;
			builder.CellFormat.Borders.Bottom.LineWidth = 1.0;
			builder.CellFormat.VerticalAlignment = CellVerticalAlignment.Center;

			// Set the cell shading for this cell.
			foreach (var headerCell in tableModel.Header) {
				builder.InsertCell();
				builder.Write(headerCell);
					
			}
			// End this row.
			builder.EndRow();

			builder.RowFormat.HeadingFormat = false;
			// Clear the cell formatting from previous operations.
			//builder.CellFormat.ClearFormatting();
			foreach (var contentRow in tableModel.Content) {
				foreach (var contentCell in contentRow) {
					builder.InsertCell();
					builder.Write(contentCell);
					
				}
				builder.EndRow();
			}

			return doc;
		}

		/// <summary>
		/// Inserts content of the external document after the specified node.
		/// Section breaks and section formatting of the inserted document are ignored.
		/// </summary>
		/// <param name="insertAfterNode">Node in the destination document after which the content
		/// should be inserted. This node should be a block level node (paragraph or table).</param>
		/// <param name="srcDoc">The document to insert.</param>
		static void InsertDocument(Node insertAfterNode, Aspose.Words.Document srcDoc) {
			// Make sure that the node is either a paragraph or table.
			if ((!insertAfterNode.NodeType.Equals(NodeType.Paragraph)) &
			  (!insertAfterNode.NodeType.Equals(NodeType.Table)))
				throw new ArgumentException("The destination node should be either a paragraph or table.");

			// We will be inserting into the parent of the destination paragraph.
			CompositeNode dstStory = insertAfterNode.ParentNode;

			// This object will be translating styles and lists during the import.
			NodeImporter importer = new NodeImporter(srcDoc, insertAfterNode.Document, ImportFormatMode.KeepSourceFormatting);

			// Loop through all sections in the source document.
			foreach (Aspose.Words.Section srcSection in srcDoc.Sections) {
				// Loop through all block level nodes (paragraphs and tables) in the body of the section.
				foreach (Node srcNode in srcSection.Body) {
					// Let's skip the node if it is a last empty paragraph in a section.
					if (srcNode.NodeType.Equals(NodeType.Paragraph)) {
						Aspose.Words.Paragraph para = (Aspose.Words.Paragraph)srcNode;
						if (para.IsEndOfSection && !para.HasChildNodes)
							continue;
					}

					// This creates a clone of the node, suitable for insertion into the destination document.
					Node newNode = importer.ImportNode(srcNode, true);

					// Insert new node after the reference node.
					dstStory.InsertAfter(newNode, insertAfterNode);
					insertAfterNode = newNode;
				}
			}
		}

		public static FileMetadata SaveFile(byte[] data, string filePath, int customerID, string templateName, int templateID) {
			var mainDirectory = Directory.CreateDirectory(filePath);
			var customerDirectory = mainDirectory.CreateSubdirectory(customerID.ToString());
			string fileName = Path.Combine(customerDirectory.FullName, templateName + "." + customerID +"."+ DateTime.Today.ToString("yyyyMMdd") + ".pdf");
			File.WriteAllBytes(fileName, data);
			return new FileMetadata {
				TemplateID = templateID,
				Name = templateName,
				Path = fileName,
				ContentType = "application/pdf"
			};
		}

		public static FileMetadata SaveFile(byte[] data, string filePath, string templateName) {
			File.WriteAllBytes(filePath, data);
			return new FileMetadata {
				Name = templateName,
				Path = filePath,
				ContentType = "application/pdf"
			};
		}

		private static readonly ILog Log = LogManager.GetLogger(typeof(PrepareMail));
	}
}
