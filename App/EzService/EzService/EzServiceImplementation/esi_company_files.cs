namespace EzService.EzServiceImplementation {
	using Ezbob.Backend.Strategies.Misc;

	partial class EzServiceImplementation {
		public ActionMetaData CompanyFilesUpload(int customerId, string fileName, byte[] fileContent, string fileContentType, bool isBankStatement) {
			return Execute<SaveCompanyFile>(customerId, null, customerId, fileName, fileContent, fileContentType, isBankStatement);
		} // CompanyFilesUpload

		public byte[] GetCompanyFile(int userId, int companyFileId) {
			GetCompanyFile oInstance;
			ExecuteSync(out oInstance, null, userId, companyFileId);
			return oInstance.FileContext;
		} // GetCompanyFile
	} // class EzServiceImplementation
} // namespace EzService
