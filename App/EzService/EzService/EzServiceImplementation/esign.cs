namespace EzService.EzServiceImplementation {
	using System.Collections.Generic;
	using EzBob.Backend.Strategies.Esign;
	using Ezbob.Backend.Models;

	partial class EzServiceImplementation {
		#region method EsignProcessPending

		public ActionMetaData EsignProcessPending(int? nCustomerID) {
			return Execute<EsignProcessPending>(null, null, nCustomerID);
		} // EsignProcessPending

		#endregion method EsignProcessPending

		#region method LoadEsignatures

		public EsignatureListActionResult LoadEsignatures(int? nCustomerID) {
			LoadEsignatures oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, nCustomerID, null, nCustomerID);

			List<Esignature> data = new List<Esignature>();

			oInstance.Result.ForEach((ignored, longIgnored, oSignature) => data.Add(oSignature));

			return new EsignatureListActionResult {
				MetaData = oMetaData,
				Data = data,
			};
		} // LoadEsignatures

		#endregion method LoadEsignatures

		#region method LoadEsignatureFile

		public EsignatureFileActionResult LoadEsignatureFile(long nEsignatureID) {
			LoadEsignatureFile oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, nEsignatureID);

			return new EsignatureFileActionResult {
				MetaData = oMetaData,
				FileName = oInstance.FileName,
				MimeType = oInstance.MimeType,
				Contents = oInstance.Contents,
			};
		} // LoadEsignatureFile

		#endregion method LoadEsignatureFile
	} // class EzServiceImplementation
} // namespace
