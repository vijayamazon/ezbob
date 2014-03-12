namespace EzService {
	using EzBob.Backend.Strategies;

	partial class EzServiceImplementation {
		public ActionMetaData CompanyFilesUpload(int customerId, string fileName, byte[] fileContent, string fileContentType) {
			return Execute(customerId, null, typeof(SaveCompanyFile), customerId, fileName, fileContent, fileContentType);
		} // CompanyFilesUpload

		public byte[] GetCompanyFile(int companyFileId) {
			GetCompanyFile oInstance;
			ExecuteSync(out oInstance, null, null, companyFileId);
			return oInstance.FileContext;
		} // GetCompanyFile
	} // class EzServiceImplementation
} // namespace EzService
