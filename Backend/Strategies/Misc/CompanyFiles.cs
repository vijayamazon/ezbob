namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using ConfigManager;
	using Ezbob.Database;
	using System.IO;

	public class SaveCompanyFile : AStrategy {
		private int customerId;
		private string fileName;
		private byte[] fileContent;
		private string fileContentType;

		public SaveCompanyFile(int customerId, string fileName, byte[] fileContent, string fileContentType) {
			this.customerId = customerId;
			this.fileName = fileName;
			this.fileContent = fileContent;
			this.fileContentType = fileContentType;
		}

		public override string Name {
			get { return "Company Files Upload"; }
		}

		public override void Execute() {
			try {
				Log.Debug("Saving file {0} to disc...", fileName);

				string sPath = CurrentValues.Instance.CompanyFilesSavePath;
				DirectoryInfo customerDirectory = null;
				if (string.IsNullOrWhiteSpace(sPath))
					Log.Debug("Not saving: operation is disabled (CompanyFilesSavePath is empty).");
				else {
					try {
						var mainDirectory = Directory.CreateDirectory(sPath);
						customerDirectory = mainDirectory.CreateSubdirectory(customerId.ToString());
					}
					catch (Exception e) {
						Log.Warn("Error while creating directory: ", e);
					} // try

					if (customerDirectory != null) {
						string sFileName = Path.Combine(customerDirectory.FullName, Guid.NewGuid().ToString("N") + "." + customerId + "." + fileName);

						Log.Info("Saving file {0} as {1}...", fileName, sFileName);

						File.WriteAllBytes(sFileName, fileContent);

						Log.Info("Saving file metadata {0} {1} {2} {3}", customerId, DateTime.UtcNow, fileName.Length > 300 ? fileName.Substring(0, 300) : fileName, sFileName);
						DB.ExecuteNonQuery("SaveCompanyFileMetadata", CommandSpecies.StoredProcedure,
							new QueryParameter("CustomerId", customerId),
							new QueryParameter("Created", DateTime.UtcNow),
							new QueryParameter("FileName", fileName.Length > 300 ? fileName.Substring(0, 300) : fileName),
							new QueryParameter("FilePath", sFileName),
							new QueryParameter("FileContentType", fileContentType));
					}
				} // if

				Log.Debug("Saving file {0} to disc complete.", fileName);
			}
			catch (Exception e) {
				Log.Error("Error saving file {0} to disc \n {1}", fileName, e);
			} // try
		}
	}

	public class GetCompanyFile : AStrategy {
		private int companyFileId;

		public GetCompanyFile(int companyFileId) {
			this.companyFileId = companyFileId;
		}

		public override string Name {
			get { return "Company File Retrieve"; }
		}

		public byte[] FileContext { get; private set; }

		public override void Execute() {
			try {
				var path = DB.ExecuteScalar<string>("GetCompanyFileMetadata", CommandSpecies.StoredProcedure,
							new QueryParameter("CompanyFileId", companyFileId));

				if (path != null) {
					FileContext = File.ReadAllBytes(path);
				}
			}
			catch (Exception e) {
				Log.Error("Error retrieving file for company, file id: {0} \n {1}", companyFileId, e);
			} // try
		}
	}
}
