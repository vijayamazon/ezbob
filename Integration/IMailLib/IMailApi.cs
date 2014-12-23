namespace IMailLib {
	using System;
	using System.IO;
	using System.Xml;
	using Ezbob.Utils.XmlUtils;
	using IMailLib.IMailApiNS;

	public class IMailApi {
		private readonly IMailApiNS.imail_apiSoap client;
		private string errorMessage = "";
		private string namesList = "";
		private string _csvData = "";
		public IMailApi() {
			client = new IMailApiNS.imail_apiSoapClient("imail_apiSoap");
		}

		public bool Authenticate(string username, string password) {
			resetErrorMessage();
			XmlNode apiResponse = client.Authenticate(username, password);
			bool success = parseXmlResponse(apiResponse);
			Console.WriteLine("Authenticate response:\n{0}", apiResponse.ToString(4));
			return success;
		}// Authenticate

		public bool SetEmailPreview(string emailAddress) {
			resetErrorMessage();
			XmlNode apiResponse = client.SetPrintPreviewEmailAddress(emailAddress);
			bool success = parseXmlResponse(apiResponse);
			Console.WriteLine("SetEmailPreview response:\n{0}", apiResponse.ToString(4));
			return success;
		}

		public bool MailMerge(byte[] csvData, string pdfAttachmentName, bool is2DayService) {
			bool success = false;
			resetErrorMessage();

			MailMergeResponse apiResponse = client.MailMerge(new MailMergeRequest {
				CSVdata = csvData,
				PDFattachmentFilename = pdfAttachmentName,
				Is2DayService = is2DayService
			});
			Console.WriteLine("MailMergeResult response:\n{0}", apiResponse.MailMergeResult.ToString(4));
			success = parseXmlResponse(apiResponse.MailMergeResult);

			return success;
		}

		public bool ProcessPrintReadyPDF(string filename, string backgroundFilename, bool is2DayService) {
			bool success = false;
			resetErrorMessage();

			if (System.IO.File.Exists(filename)) {
				FileInfo fInfo = new FileInfo(filename);
				long numBytes = fInfo.Length;
				long MBsize = numBytes / 1000000;

				if (MBsize <= 8) // server has an 8MB file upload limit
					{
					FileStream fStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
					BinaryReader br = new BinaryReader(fStream);

					// convert the file to a byte array
					byte[] filedata = br.ReadBytes((int)numBytes);
					br.Close();

					// pass the filename and data to the web service
					success = ProcessPrintReadyPDF(filedata, backgroundFilename, is2DayService);

					// tidy up
					fStream.Close();
					fStream.Dispose();
					
				} else {
					errorMessage = string.Format("File '{0}' is too big for the API method ProcessPrintReadyPDF", filename);
				}
			} else {
				errorMessage = string.Format("The file '{0}' does not exist", filename);
			}

			return success;
		}

		public bool ProcessPrintReadyPDF(byte[] pdfData, string backgroundFilename, bool is2DayService) {
			bool success = false;
			resetErrorMessage();
			
			ProcessPrintReadyPDFResponse apiResponse = client.ProcessPrintReadyPDF(new ProcessPrintReadyPDFRequest {
				letterData = pdfData,
				backgroundFilename = backgroundFilename,
				Is2DayService = is2DayService
			});

			Console.WriteLine("ProcessPrintReadyPDF response:\n{0}", apiResponse.ProcessPrintReadyPDFResult.ToString(4));
			success = parseXmlResponse(apiResponse.ProcessPrintReadyPDFResult);
			return success;
		}
	

		public bool UpdateAttachment(string filePath, string fileName) {
			resetErrorMessage();
			bool success = false;

			if (System.IO.File.Exists(filePath)) {
				FileInfo fInfo = new FileInfo(filePath);
				long numBytes = fInfo.Length;
				long MBsize = numBytes / 1000000;

				if (MBsize <= 8)        // server has an 8MB file upload limit
                {
					FileStream fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
					BinaryReader br = new BinaryReader(fStream);

					// convert the file to a byte array
					byte[] filedata = br.ReadBytes((int)numBytes);
					br.Close();

					// pass the filename and data to the web service
					UpdateAttachmentResponse apiResponse = client.UpdateAttachment(new UpdateAttachmentRequest() {
						attachmentPDFfilename = fileName,
						attachmentPDFdata = filedata
					});

					Console.WriteLine("UpdateAttachment response:\n{0}", apiResponse.UpdateAttachmentResult.ToString(4));

					// tidy up
					fStream.Close();
					fStream.Dispose();

					success = parseXmlResponse(apiResponse.UpdateAttachmentResult);
				} else
					errorMessage = string.Format("File '{0}' is too big to use as an attachment", filePath);
			}
			return success;
		}

		public bool ListAttachments(out string filenames) {
			resetErrorMessage();
			filenames = "";
			XmlNode apiResponse = client.ListAttachments();
			Console.WriteLine("ListAttachments response:\n{0}", apiResponse.ToString(4));

			bool success = parseXmlResponse(apiResponse);

			if (success) {
				filenames = namesList;
			}
			return success;
		}

		public bool DeleteAttachment(string attachmentFilename) {
			resetErrorMessage();
			XmlNode apiResponse = client.DeleteAttachment(attachmentFilename);
			return (parseXmlResponse(apiResponse));
		}

		public bool UpdateBackground(string filePath, string fileName) {
			resetErrorMessage();
			bool success = false;

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

					// pass the filename and data to the web service
					UpdateBackgroundResponse apiResponse = client.UpdateBackground(new UpdateBackgroundRequest() { backgroundPDFfilename = fileName, backgroundPDFdata = fileData });
					Console.WriteLine("UpdateBackground response:\n{0}", apiResponse.UpdateBackgroundResult.ToString(4));

					// tidy up
					fStream.Close();
					fStream.Dispose();

					success = parseXmlResponse(apiResponse.UpdateBackgroundResult);
				} else
					errorMessage = string.Format("File '{0}' is too big to use as a background PDF", filePath);
			}

			return success;
		}
		public bool GetReturns(out string returns) {
			resetErrorMessage();
			var response = client.GetReturns(DateTime.Today.AddYears(-1).ToString("yyyy-MM-dd"), DateTime.Today.ToString("yyyy-MM-dd"));

			bool success = parseXmlResponse(response);
			returns = _csvData;
			return success;
		}

		public bool ListBackgrounds(out string filenames) {
			resetErrorMessage();
			filenames = "";
			bool success = parseXmlResponse(client.ListBackgrounds());

			if (success == true)
				filenames = namesList;

			return success;
		}

		public bool DeleteBackground(string backgroundFilename) {
			resetErrorMessage();
			XmlNode apiResponse = client.DeleteBackground(backgroundFilename);
			return (parseXmlResponse(apiResponse));
		}

		public string GetErrorMessage() {
			return errorMessage;
		}

		private void resetErrorMessage() {
			errorMessage = "";
		}

		private bool parseXmlResponse(XmlNode node) {
			bool success = false;
			int cc, att;
			errorMessage = "";

			try {
				XmlNodeList nodeList = node.ChildNodes;

				for (cc = 0; cc < nodeList.Count; cc++) {
					XmlNode child = nodeList[cc];
					if (child.Name == "Response") {
						XmlAttributeCollection attribs = child.Attributes;

						for (att = 0; att < attribs.Count; att++) {
							XmlAttribute xAttrib = attribs[att];

							switch (xAttrib.Name) {
							case "success":
								if (xAttrib.Value == "true")
									success = true;
								break;
							case "error_message":
								errorMessage = xAttrib.Value;
								break;
							case "CSVdata":
								_csvData = xAttrib.Value;
								break;
							}
						}

						if (child.HasChildNodes)
							populateSortedListFromNodesList(child.ChildNodes);
					}
				}
			} catch {
			}

			return success;
		}

		private void populateSortedListFromNodesList(XmlNodeList nodeList) {
			int xx, att;

			namesList = "";

			for (xx = 0; xx < nodeList.Count; xx++) {
				XmlNode listItem = nodeList[xx];
				XmlAttributeCollection attribs = listItem.Attributes;

				for (att = 0; att < attribs.Count; att++) {
					XmlAttribute xAttrib = attribs[att];

					switch (xAttrib.Name) {
					case "name":
						if (namesList != "")
							namesList += "?";
						namesList += xAttrib.Value;       // build a string of '?' separated names
						break;
					}
				}
			}
		}
		public bool MailmergeLetterheadPDF(byte[] pdfData, byte[] csvData, string bodyHtml, bool is2DayService) {
			MailmergeLetterheadPDFResponse response = client.MailmergeLetterheadPDF(new MailmergeLetterheadPDFRequest {
				PDFdata = pdfData,
				CSVdata = csvData,
				bodyHTML = bodyHtml,
				Is2DayService = is2DayService
			});

			bool success = parseXmlResponse(response.MailmergeLetterheadPDFResult);
			Console.WriteLine("MailmergeLetterheadPDF response:\n{0}", response.MailmergeLetterheadPDFResult.ToString(4));

			return success;
		}

		
		/*
		public bool Authenticate() {

			XmlNode response = client.Authenticate(Username: "Emma123456", Password: "Ezbob123");
			Console.WriteLine("Authenticate response: {0}", response.ToString(4));
			if (response != null && response["Response"] != null && bool.Parse(response["Response"].Attributes["success"].Value)) {
				return true;
			}

			if (response != null && response["Response"] != null && !bool.Parse(response["Response"].Attributes["success"].Value)) {
				Console.WriteLine("Authenticate failed with error: {0}", response["Response"].Attributes["error_message"].Value);
				return false;
			}
			return false;
		}

		public List<AttachmentModel> ListAttachment() {
			var attachmentModels = new List<AttachmentModel>();
			if (Authenticate()) {
				XmlNode response = client.ListAttachments();
				Console.WriteLine("ListAttachment response:\n{0}", response.ToString(4));
				if (response != null && response["Response"] != null && bool.Parse(response["Response"].Attributes["success"].Value)) {
					var attachments = response["Response"].SelectNodes("Attachment");
					if (attachments != null) {
						foreach (XmlNode attachment in attachments) {
							attachmentModels.Add(new AttachmentModel {
								Name = attachment.Attributes["name"].Value,
								Date = DateTime.ParseExact(attachment.Attributes["date"].Value, "yyyyMMdd HH:mm:ss", CultureInfo.InvariantCulture),
								Size = long.Parse(attachment.Attributes["size"].Value)
							});
						}
					}
				}

				if (response != null && response["Response"] != null && !bool.Parse(response["Response"].Attributes["success"].Value)) {
					Console.WriteLine("ListAttachment failed with error: {0}", response["Response"].Attributes["error_message"].Value);

				}
			}
			return attachmentModels;
		}

		public string GetReturns() {
			if (Authenticate()) {
				XmlNode response = client.GetReturns(DateTime.Today.AddYears(-1).ToString("yyyy-MM-dd"), DateTime.Today.ToString("yyyy-MM-dd"));
				Console.WriteLine("GetReturns response:\n{0}", response.ToString(4));
				if (response != null && response["Response"] != null && bool.Parse(response["Response"].Attributes["success"].Value)) {
					return response["Response"].Attributes["CSVdata"].Value;
				}

				Console.WriteLine("GetReturns failed");
			}

			return null;
		}

		public bool SetPrintPreviewEmailAddress(string email) {
			if (Authenticate()) {
				var response = client.SetPrintPreviewEmailAddress(email);
				Console.WriteLine("SetPrintPreviewEmailAddress response:\n{0}", response.ToString(4));
				if (response != null && response["Response"] != null && bool.Parse(response["Response"].Attributes["success"].Value)) {
					return true;
				}

				Console.WriteLine("SetPrintPreviewEmailAddress failed");
			}
			return false;
		}

		
		 */


	}
}
