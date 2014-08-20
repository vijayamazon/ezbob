﻿namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies.Misc;

	partial class EzServiceImplementation {
		public ActionMetaData CompanyFilesUpload(int customerId, string fileName, byte[] fileContent, string fileContentType) {
			return Execute<SaveCompanyFile>(customerId, null, customerId, fileName, fileContent, fileContentType);
		} // CompanyFilesUpload

		public byte[] GetCompanyFile(int companyFileId) {
			GetCompanyFile oInstance;
			ExecuteSync(out oInstance, null, null, companyFileId);
			return oInstance.FileContext;
		} // GetCompanyFile
	} // class EzServiceImplementation
} // namespace EzService
