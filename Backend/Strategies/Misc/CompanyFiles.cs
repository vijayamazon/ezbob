namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using ConfigManager;
	using Ezbob.Database;
	using System.IO;

	public class SaveCompanyFile : AStrategy {
		private readonly int customerId;
		private readonly string fileName;
		private readonly byte[] fileContent;
		private readonly string fileContentType;
		private readonly bool isBankStatement;

		public SaveCompanyFile(int customerId, string fileName, byte[] fileContent, string fileContentType, bool isBankStatement) {
			this.customerId = customerId;
			this.fileName = fileName;
			this.fileContent = fileContent;
			this.fileContentType = fileContentType;
			this.isBankStatement = isBankStatement;
		}

		public override string Name {
			get { return "Company Files Upload"; }
		}

		public override void Execute() {
			try {
				Log.Debug("Saving file {0} to disc...", this.fileName);

				string sPath = CurrentValues.Instance.CompanyFilesSavePath;
				DirectoryInfo customerDirectory = null;
				if (string.IsNullOrWhiteSpace(sPath))
					Log.Debug("Not saving: operation is disabled (CompanyFilesSavePath is empty).");
				else {
					try {
						var mainDirectory = Directory.CreateDirectory(sPath);
						customerDirectory = mainDirectory.CreateSubdirectory(this.customerId.ToString());
					}
					catch (Exception e) {
						Log.Warn("Error while creating directory: {0}", e);
					} // try

					if (customerDirectory != null) {
						string sFileName = Path.Combine(customerDirectory.FullName, Guid.NewGuid().ToString("N") + "." + this.customerId + "." + this.fileName);

						Log.Info("Saving file {0} as {1}...", this.fileName, sFileName);

						File.WriteAllBytes(sFileName, this.fileContent);

						Log.Info("Saving file metadata {0} {1} {2} {3}", this.customerId, DateTime.UtcNow, this.fileName.Length > 300 ? this.fileName.Substring(0, 300) : this.fileName, sFileName);
						DB.ExecuteNonQuery("SaveCompanyFileMetadata", CommandSpecies.StoredProcedure,
							new QueryParameter("CustomerId", this.customerId),
							new QueryParameter("Created", DateTime.UtcNow),
							new QueryParameter("FileName", this.fileName.Length > 300 ? this.fileName.Substring(0, 300) : this.fileName),
							new QueryParameter("FilePath", sFileName),
							new QueryParameter("FileContentType", this.fileContentType),
							new QueryParameter("IsBankStatement", this.isBankStatement));
					}
				} // if

				Log.Debug("Saving file {0} to disc complete.", this.fileName);
			}
			catch (Exception e) {
				Log.Error("Error saving file {0} to disc \n {1}", this.fileName, e);
			} // try
		}
	}

	public class GetCompanyFile : AStrategy {
		private readonly int companyFileId;

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
							new QueryParameter("CompanyFileId", this.companyFileId));

				if (path != null) {
					FileContext = File.ReadAllBytes(path);
				}
			}
			catch (Exception e) {
				Log.Error("Error retrieving file for company, file id: {0} \n {1}", this.companyFileId, e);
			} // try
		}
	}
}
