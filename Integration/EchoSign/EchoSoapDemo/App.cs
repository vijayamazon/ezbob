using System;
using System.IO;
using EchoSoap.secure.echosign.com;

namespace EchoSoap
{
	/// <summary>
	/// Summary description for App.
	/// </summary>
	class App
	{
		static EchoSignDocumentService18 ES;

		const string UsageText =
			@"Usage:
echosoap.exe <Service URL> <API key> <function> [parameters]
  
where the function is one of:
test
createFormFieldLayerTemplate [<filename>] ('testtemplate.pdf' will be used if <filename> is not provided)
send <filename> <recipient_email>
sendWithFormFieldLayerTemplate <filename> <libraryTemplateKey> <recipient_email>
info <documentKey>
latest <documentKey> <filename>
version <versionKey> <filename>

test                            will run basic tests to make sure you can communicate with the web service
createFormFieldLayerTemplate    will create a form field layer template, and returns a libraryTemplateKey
send                            will create a new agreement in the EchoSign system, and returns a documentKey
sendWithFormFieldLayerTemplate  will create a new agreement with the specified form field layer template applied
info                            returns the current status and all the history events for a given documentKey
latest                          saves the latest version of the document as a PDF with the given filename
version                         saves a specific version of the document (referenced by the history events)
";

        const string testPrefix = "Test from SOAP: ";
        const string testMessage = "This is neat.";

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length < 3) print_usage_and_exit();

            String serviceUrl = args[0];
            String apiKey = args[1];
			String command = args[2];
			ES = new EchoSignDocumentService18();
            ES.Url = serviceUrl;

			switch (command) 
			{
				case "test":
					if (args.Length != 3) print_usage_and_exit();
					test(apiKey);
					break;
                case "createFormFieldLayerTemplate":
                    String filename = (args.Length > 3) ? args[3] : null;
                    createLibraryTemplate(apiKey, filename, LibraryTemplateType.FORM_FIELD_LAYER);
                    break;
				case "send":
					if (args.Length != 5) print_usage_and_exit(); 
					sendDocument(apiKey, args[3], null, args[4]);
					break;
                case "sendWithFormFieldLayerTemplate":
                    if (args.Length != 6) print_usage_and_exit();
                    sendDocument(apiKey, args[3], args[4], args[5]);
                    break;
				case "info":
					if (args.Length != 4) print_usage_and_exit();
					getDocumentInfo(apiKey, args[3]);
					break;
				case "latest":
					if (args.Length != 5) print_usage_and_exit();
					getDocument(apiKey, args[3], null, args[4]);
					break;
				case "version":
					if (args.Length != 5) print_usage_and_exit();
					getDocument(apiKey, null, args[3], args[4]);
					break;
			}
		}

		static void print_usage_and_exit()
		{
			Console.Error.WriteLine(UsageText);
			Environment.Exit(1);
		}
		public static void test(string apiKey)
		{
			Console.WriteLine("Testing basic connectivity...");
			Pong pong = ES.testPing(apiKey);
			Console.WriteLine("Message from server: " + pong.message);
			FileStream file = getTestPdfFile();
			Console.WriteLine("Testing file transfer...");
			Byte[] bytes = new byte[file.Length];
			file.Read(bytes, 0, bytes.Length);
			Byte[] rbytes = ES.testEchoFile(apiKey, bytes);
			
			bool resultOk = true;
			if (bytes.Length == rbytes.Length) {
				int i = bytes.Length;
				while (--i >= 0)
					if (bytes[i] != rbytes[i]) {
						resultOk = false;
						break;
					}
			}
			if (resultOk)
				Console.WriteLine("Woohoo!  Everything seems to work.");
			else 
				Console.WriteLine("ERROR:  Some kind of problem with file transfer, it seems.");
		}

        public static FileStream getTestPdfFile()
        {
            return getTestPdfFile("test.pdf");
        }

        public static FileStream getTestPdfFile(string filename)
        {
            if (File.Exists(".\\" + filename))
                return File.OpenRead(".\\" + filename);
            return File.OpenRead("..\\" + filename);
        }

        public static void createLibraryTemplate(string apiKey, string filename, LibraryTemplateType type)
        {
            FileStream templateFile = (filename != null) ? File.OpenRead(filename) : getTestPdfFile("testtemplate.pdf");
            filename = Path.GetFileName(templateFile.Name);

            secure.echosign.com.FileInfo[] fileInfos = new secure.echosign.com.FileInfo[1];
            fileInfos[0] = new secure.echosign.com.FileInfo(filename, null, templateFile);

            System.Nullable<LibraryTemplateType>[] libraryTemplateTypes = new System.Nullable<LibraryTemplateType>[1];
            libraryTemplateTypes[0] = LibraryTemplateType.FORM_FIELD_LAYER;
            LibraryDocumentCreationInfo libraryInfo = new LibraryDocumentCreationInfo(
                testPrefix + filename,
                fileInfos,
                SignatureType.ESIGN,
                SignatureFlow.SENDER_SIGNATURE_NOT_REQUIRED,
                LibrarySharingMode.USER,
                libraryTemplateTypes);
            LibraryDocumentCreationResult result = ES.createLibraryDocument(apiKey, null, libraryInfo);
            Console.WriteLine("Document key is: " + result.documentKey);
        }

		public static void sendDocument(string apiKey, string fileName, string formFieldLayerTemplateKey, string recipient)
		{
			FileStream file = getTestPdfFile(fileName);
			secure.echosign.com.FileInfo[] fileInfos = new secure.echosign.com.FileInfo[1];
			fileInfos[0] = new secure.echosign.com.FileInfo(fileName, null, file);
			SenderInfo senderInfo = null;
			string[] recipients = new string[1];
			recipients[0] = recipient;
			DocumentCreationInfo documentInfo = new DocumentCreationInfo(
				recipients,
				testPrefix + Path.GetFileName(file.Name),
				testMessage,
				fileInfos,
				SignatureType.ESIGN,
				SignatureFlow.SENDER_SIGNATURE_NOT_REQUIRED
			);
            if (formFieldLayerTemplateKey != null)
            {
                secure.echosign.com.FileInfo[] formFieldLayerTemplates = new secure.echosign.com.FileInfo[1];
                formFieldLayerTemplates[0] = new secure.echosign.com.FileInfo(formFieldLayerTemplateKey);
                documentInfo.formFieldLayerTemplates = formFieldLayerTemplates;
            }
            DocumentKey[] documentKeys;
			documentKeys = ES.sendDocument(apiKey, senderInfo, documentInfo);
			Console.WriteLine("Document key is: " + documentKeys[0].documentKey);
		}

		public static void getDocumentInfo(string apiKey, string documentKey)
		{
			DocumentInfo info = ES.getDocumentInfo(apiKey, documentKey);
			Console.WriteLine("Document is in status: " + info.status);
			Console.WriteLine("Document history: ");
			foreach (DocumentHistoryEvent docEvent in info.events)
			{
				Console.WriteLine(
					"\t" + 
					docEvent.description + 
					" on " + 
					docEvent.date +
					(docEvent.documentVersionKey == null 
						? ""
						:(" (versionKey: " + docEvent.documentVersionKey + ")")
					)
				);
			}
			Console.WriteLine("Latest versionKey: " + info.latestDocumentKey);
		}

		public static void getDocument(string apiKey, string documentKey, string versionKey, string fileName)
		{
			byte[] data = (documentKey != null)
				? ES.getLatestDocument(apiKey, documentKey) 
				: ES.getDocumentByVersion(apiKey, versionKey);
			FileStream file = File.OpenWrite(fileName);
			file.Write(data, 0, data.Length);
		}
	}
}
