namespace IMailLib {
	using System;
	using System.IO;
	using System.Xml;
	using IMailLib.Helpers;
	using IMailLib.IMailNewApiNS;
	using log4net;

	public class IMailApi {
		public IMailApi() {
			this.client = new IMailNewApiNS.imail_apiSoapClient("imail_newapiSoap");
		}

		public bool Authenticate(string username, string password) {
			resetErrorMessage();
			XmlNode apiResponse = this.client.Authenticate(username, password);
			bool success = parseXmlResponse(apiResponse);
			this.Log.InfoFormat("Authenticate response:\n{0}", apiResponse.ToString(4));
			return success;
		}

		public bool DeleteAttachment(string attachmentFilename) {
			resetErrorMessage();
			XmlNode apiResponse = this.client.DeleteAttachment(attachmentFilename);
			return (parseXmlResponse(apiResponse));
		}

		public bool DeleteBackground(string backgroundFilename) {
			resetErrorMessage();
			XmlNode apiResponse = this.client.DeleteBackground(backgroundFilename);
			return (parseXmlResponse(apiResponse));
		}

		public string GetErrorMessage() {
			return this.errorMessage;
		}

		public bool GetReturns(out string returns) {
			resetErrorMessage();
			var response = this.client.GetReturns(DateTime.Today.AddYears(-1)
				.ToString("yyyy-MM-dd"), DateTime.Today.ToString("yyyy-MM-dd"));

			bool success = parseXmlResponse(response);
			returns = this.csvData;
			return success;
		}

		public bool ListAttachments(out string filenames) {
			resetErrorMessage();
			filenames = "";
			XmlNode apiResponse = this.client.ListAttachments();
			this.Log.InfoFormat("ListAttachments response:\n{0}", apiResponse.ToString(4));

			bool success = parseXmlResponse(apiResponse);

			if (success)
				filenames = this.namesList;
			return success;
		}

		public bool ListBackgrounds(out string filenames) {
			resetErrorMessage();
			filenames = "";
			bool success = parseXmlResponse(this.client.ListBackgrounds());

			if (success)
				filenames = this.namesList;

			return success;
		}

		public bool MailMerge(byte[] csvDataArray, string templateName, string pdfAttachmentName, bool is2DayService) {
			resetErrorMessage();

			MailMergeResponse apiResponse = this.client.MailMerge(new MailMergeRequest {
				CSVdata = csvDataArray,
				PDFattachmentFilename = pdfAttachmentName,
				Is2DayService = is2DayService,
				templateName = templateName
			});

			this.Log.InfoFormat("MailMergeResult response:\n{0}", apiResponse.MailMergeResult.ToString(4));
			bool success = parseXmlResponse(apiResponse.MailMergeResult);

			return success;
		}

		public bool MailmergeLetterheadPDF(byte[] pdfData, byte[] csvDataArray, string bodyHtml, bool is2DayService) {
			MailmergeLetterheadPDFResponse response = this.client.MailmergeLetterheadPDF(new MailmergeLetterheadPDFRequest {
				PDFdata = pdfData,
				CSVdata = csvDataArray,
				bodyHTML = bodyHtml,
				Is2DayService = is2DayService
			});

			bool success = parseXmlResponse(response.MailmergeLetterheadPDFResult);
			this.Log.InfoFormat("MailmergeLetterheadPDF response:\n{0}", response.MailmergeLetterheadPDFResult.ToString(4));

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
				} else
					this.errorMessage = string.Format("File '{0}' is too big for the API method ProcessPrintReadyPDF", filename);
			} else
				this.errorMessage = string.Format("The file '{0}' does not exist", filename);

			return success;
		}

		public bool ProcessPrintReadyPDF(byte[] pdfData, string backgroundFilename, bool is2DayService) {
			bool success = false;
			resetErrorMessage();

			ProcessPrintReadyPDFResponse apiResponse = this.client.ProcessPrintReadyPDF(new ProcessPrintReadyPDFRequest {
				letterData = pdfData,
				backgroundFilename = backgroundFilename,
				Is2DayService = is2DayService
			});

			this.Log.InfoFormat("ProcessPrintReadyPDF response:\n{0}", apiResponse.ProcessPrintReadyPDFResult.ToString(4));
			success = parseXmlResponse(apiResponse.ProcessPrintReadyPDFResult);
			return success;
		}

		public bool SetEmailPreview(string emailAddress) {
			resetErrorMessage();
			XmlNode apiResponse = this.client.SetPrintPreviewEmailAddress(emailAddress);
			bool success = parseXmlResponse(apiResponse);
			this.Log.InfoFormat("SetEmailPreview response:\n{0}", apiResponse.ToString(4));
			return success;
		}

		public bool UpdateAttachment(string filePath, string fileName) {
			resetErrorMessage();
			bool success = false;

			if (System.IO.File.Exists(filePath)) {
				FileInfo fInfo = new FileInfo(filePath);
				long numBytes = fInfo.Length;
				long MBsize = numBytes / 1000000;

				if (MBsize <= 8) // server has an 8MB file upload limit
				{
					FileStream fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
					BinaryReader br = new BinaryReader(fStream);

					// convert the file to a byte array
					byte[] filedata = br.ReadBytes((int)numBytes);
					br.Close();

					// pass the filename and data to the web service
					UpdateAttachmentResponse apiResponse = this.client.UpdateAttachment(new UpdateAttachmentRequest() {
						attachmentPDFfilename = fileName,
						attachmentPDFdata = filedata
					});

					this.Log.InfoFormat("UpdateAttachment response:\n{0}", apiResponse.UpdateAttachmentResult.ToString(4));

					// tidy up
					fStream.Close();
					fStream.Dispose();

					success = parseXmlResponse(apiResponse.UpdateAttachmentResult);
				} else
					this.errorMessage = string.Format("File '{0}' is too big to use as an attachment", filePath);
			}
			return success;
		}

		public bool UpdateBackground(string filePath, string fileName) {
			resetErrorMessage();
			bool success = false;

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

					// pass the filename and data to the web service
					UpdateBackgroundResponse apiResponse = this.client.UpdateBackground(new UpdateBackgroundRequest() {
						backgroundPDFfilename = fileName,
						backgroundPDFdata = fileData
					});
					this.Log.InfoFormat("UpdateBackground response:\n{0}", apiResponse.UpdateBackgroundResult.ToString(4));

					// tidy up
					fStream.Close();
					fStream.Dispose();

					success = parseXmlResponse(apiResponse.UpdateBackgroundResult);
				} else
					this.errorMessage = string.Format("File '{0}' is too big to use as a background PDF", filePath);
			}

			return success;
		}

		private bool parseXmlResponse(XmlNode node) {
			bool success = false;
			this.errorMessage = "";

			try {
				XmlNodeList nodeList = node.ChildNodes;

				int cc;
				for (cc = 0; cc < nodeList.Count; cc++) {
					XmlNode child = nodeList[cc];
					if (child.Name == "Response") {
						XmlAttributeCollection attribs = child.Attributes;

						if (attribs == null) { continue; }

						int att;
						for (att = 0; att < attribs.Count; att++) {
							XmlAttribute xAttrib = attribs[att];

							switch (xAttrib.Name) {
								case "success":
									if (xAttrib.Value == "true")
										success = true;
									break;
								case "error_message":
									this.errorMessage = xAttrib.Value;
									break;
								case "CSVdata":
									this.csvData = xAttrib.Value;
									break;
							}
						}

						if (child.HasChildNodes)
							populateSortedListFromNodesList(child.ChildNodes);
					}
				}
			} catch {
				//Don't care
			}

			return success;
		}

		private void populateSortedListFromNodesList(XmlNodeList nodeList) {
			int xx;

			this.namesList = "";

			for (xx = 0; xx < nodeList.Count; xx++) {
				XmlNode listItem = nodeList[xx];
				XmlAttributeCollection attribs = listItem.Attributes;

				if (attribs == null) { continue; }

				int att;
				for (att = 0; att < attribs.Count; att++) {
					XmlAttribute xAttrib = attribs[att];

					switch (xAttrib.Name) {
						case "name":
							if (this.namesList != "")
								this.namesList += "?";
							this.namesList += xAttrib.Value; // build a string of '?' separated names
							break;
					}
				}
			}
		}

		private void resetErrorMessage() {
			this.errorMessage = "";
		}

		private readonly IMailNewApiNS.imail_apiSoap client;
		private string csvData = "";
		private string errorMessage = "";
		private string namesList = "";
		protected readonly ILog Log = LogManager.GetLogger(typeof(CollectionMail));
	}
}
